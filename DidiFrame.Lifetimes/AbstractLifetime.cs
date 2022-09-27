using DidiFrame.Utils;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Lifetimes
{
	/// <summary>
	/// Represents base lifetime class with initial data 
	/// </summary>
	/// <typeparam name="TBase">Type of lifetime base</typeparam>
	public abstract class AbstractLifetime<TBase> : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		/// <summary>
		/// Lifetime update event handler delegate
		/// </summary>
		protected delegate void LifetimeUpdatedHandler();

		/// <summary>
		/// Lifetime run event handler delegate
		/// </summary>
		/// <param name="initialBase">Initial readonly base value</param>
		/// <param name="context">Saved lifetime context</param>
		/// <param name="initialDataBuilder">Initial data builder</param>
		protected delegate void LifetimeRanHandler(TBase initialBase, ILifetimeContext<TBase> context, InitialDataBuilder initialDataBuilder);

		/// <summary>
		/// Lifetime post run event handler delegate
		/// </summary>
		/// <param name="initialBase">Initial readonly base value</param>
		/// <param name="initialData">Builden initial data of lifetime</param>
		protected delegate void LifetimePostRanHandler(TBase initialBase, InitialData initialData);


		private const string ContextField = "context";
		private const string ServerField = "server";
		private const string IdField = "id";
		private InitialData? data;


		/// <summary>
		/// Creates new instance of DidiFrame.Lifetimes.AbstractLifetime`1
		/// </summary>
		/// <param name="logger">Logger to log some lifetime actions</param>
		protected AbstractLifetime(ILogger logger)
		{
			Logger = logger;
		}


		/// <summary>
		/// Event that fires on lifetime run
		/// </summary>
		protected event LifetimeRanHandler? LifetimeRan;

		/// <summary>
		/// Event that fires on lifetime post run
		/// </summary>
		protected event LifetimePostRanHandler? LifetimePostRan;

		/// <summary>
		/// Event that fires on lifetime updated
		/// </summary>
		protected event LifetimeUpdatedHandler? LifetimeUpdated;


		/// <summary>
		/// If lifetime is newly created else lifetime is restored
		/// </summary>
		public bool IsNewlyCreated => LifetimeContext.IsNewlyCreated;

		/// <summary>
		/// If lifetime is initialized
		/// </summary>
		public bool IsInitialized => data is not null;

		/// <summary>
		/// If lifetime is finalized
		/// </summary>
		public bool IsFinalized { get; private set; }

		/// <summary>
		/// Server that contains this lifetime
		/// </summary>
		public IServer Server => Data.Get<IServer>(ServerField);

		/// <summary>
		/// Id of lifetime
		/// </summary>
		public Guid Id => Data.Get<Guid>(IdField);

		/// <summary>
		/// Initialized lifetime data
		/// </summary>
		protected InitialData Data => data ?? throw new InvalidOperationException("Enable to get data before run or in LifetimeRan event handler");

		/// <summary>
		/// Provides current lifetime sync context
		/// </summary>
		protected LifetimeSynchronizationContext SynchronizationContext => LifetimeContext.GetSynchronizationContext();

		private ILifetimeContext<TBase> LifetimeContext => Data.Get<ILifetimeContext<TBase>>(ContextField);

		/// <summary>
		/// Provided logger
		/// </summary>
		protected ILogger Logger { get; }


		/// <inheritdoc/>
		public virtual void Destroy() { }

		/// <inheritdoc/>
		public virtual void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public void Run(TBase initialBase, ILifetimeContext<TBase> context)
		{
			var builder = new InitialDataBuilder();
			builder.AddData(ContextField, context);
			builder.AddData(IdField, initialBase.Id);
			builder.AddData(ServerField, initialBase.Server);

			LifetimeRan?.Invoke(initialBase, context, builder);
			data = builder.Build();
			LifetimePostRan?.Invoke(initialBase, data);
		}

		/// <inheritdoc/>
		public void Update()
		{
			LifetimeUpdated?.Invoke();
		}

		/// <summary>
		/// Finalizes lifetime
		/// </summary>
		protected void FinalizeLifetime()
		{
			IsFinalized = true;
			LifetimeContext.FinalizeLifetime();
		}

		/// <summary>
		/// Crashes lifetime with exception
		/// </summary>
		/// <param name="exception">Exception to log</param>
		/// <param name="isInvalidBase">If base has been invalid</param>
		protected void CrashLifetime(Exception exception, bool isInvalidBase)
		{
			IsFinalized = true;
			LifetimeContext.FinalizeLifetime();
			Logger.Log(LogLevel.Error, exception, "Lifetime {LifetimeId} crashed with error. Base object was {ValidStatus}", Id, isInvalidBase ? "Invalid" : "Valid");
		}

		/// <summary>
		/// Terminates lifetime
		/// </summary>
		/// <param name="reason">Reason of termination</param>
		/// <param name="exception">Optional termination exception</param>
		protected void TerminateLifetime(string reason, Exception? exception = null)
		{
			CrashLifetime(new LifetimeTerminatedException(GetType(), Id, reason, exception), isInvalidBase: false);
		}

		/// <summary>
		/// Throws exception unless lifetime is builden
		/// </summary>
		/// <exception cref="InvalidOperationException">If lifetime is not builden</exception>
		protected void ThrowUnlessBuilden()
		{
			if (IsInitialized == false)
				throw new InvalidOperationException("This operation is blocked, Lifetime isn't builden");
		}

		/// <summary>
		/// Throws exception if lifetime is builden
		/// </summary>
		/// <exception cref="InvalidOperationException">If lifetime is builden</exception>
		protected void ThrowIfBuilden()
		{
			if (IsInitialized)
				throw new InvalidOperationException("This operation is blocked, Lifetime is already builden");
		}

		#region Base access methods
		/// <summary>
		/// Provides base holder in write mode
		/// </summary>
		/// <returns>Object holder with base object</returns>
		protected virtual ObjectHolder<TBase> GetBase() => LifetimeContext.AccessBase().Open();

		/// <summary>
		/// Provides base holder in read only mode
		/// </summary>
		/// <returns>Object holder with base object</returns>
		protected virtual ObjectHolder<TBase> GetReadOnlyBase()
		{
			var objectHolder = LifetimeContext.AccessBase().Open();
			var baseObject = objectHolder.Object;
			var prevHashCode = baseObject.GetHashCode();

			return new ObjectHolder<TBase>(baseObject, _ =>
			{
				objectHolder.Dispose();
				if (prevHashCode != baseObject.GetHashCode())
				{
					var ex = new InvalidOperationException("Error, base was changed in readonly base accessor. Lifetime is collapsing");
					CrashLifetime(ex, false);
					throw ex;
				}
			});
		}

		/// <summary>
		/// Provides some base property
		/// </summary>
		/// <typeparam name="TTarget">Type of property</typeparam>
		/// <param name="selector">Property selector</param>
		/// <returns>Selected value</returns>
		protected TTarget GetBaseProperty<TTarget>(Func<TBase, TTarget> selector)
		{
			TTarget value;
			using (var baseObj = GetReadOnlyBase())
			{
				value = selector(baseObj.Object);
			}

			return value;
		}

		/// <summary>
		/// Get object controller for base obj
		/// </summary>
		/// <param name="asReadOnly">If need get readonly controller</param>
		/// <returns>Object controller</returns>
		protected IObjectController<TBase> GetBaseController(bool asReadOnly = true)
		{
			return new BaseWrapController(this, asReadOnly);
		}
		#endregion


		/// <summary>
		/// Lifetime's initial data, represents string to object dictionary
		/// </summary>
		protected sealed class InitialData
		{
			/// <summary>
			/// Creates new instance InitialData
			/// </summary>
			/// <param name="addititionalData">Initial data dictionary</param>
			public InitialData(IReadOnlyDictionary<string, object> addititionalData)
			{
				AddititionalData = addititionalData;
			}


			/// <summary>
			/// Base initial data dictionary
			/// </summary>
			public IReadOnlyDictionary<string, object> AddititionalData { get; }


			/// <summary>
			/// Gets value from initial data dictionary
			/// </summary>
			/// <typeparam name="T">Waiting value type</typeparam>
			/// <param name="key">Target key</param>
			/// <returns>Casted initial value</returns>
			/// <exception cref="KeyNotFoundException">If initial data doesn't contain target key</exception>
			public T Get<T>(string key) => (T)AddititionalData[key];
		}

		/// <summary>
		/// Builder for InitialData
		/// </summary>
		protected sealed class InitialDataBuilder
		{
			private readonly Dictionary<string, object> data = new();


			/// <summary>
			/// Adds entry to new initial data
			/// </summary>
			/// <param name="key">Key in data</param>
			/// <param name="obj">Target object</param>
			public void AddData(string key, object obj) => data.Add(key, obj);

			/// <summary>
			/// Builds InitialData object
			/// </summary>
			/// <returns></returns>
			public InitialData Build() => new(data);
		}

		private sealed class BaseWrapController : IObjectController<TBase>
		{
			private readonly AbstractLifetime<TBase> lifetime;
			private readonly bool asReadOnly;


			public BaseWrapController(AbstractLifetime<TBase> lifetime, bool asReadOnly)
			{
				this.lifetime = lifetime;
				this.asReadOnly = asReadOnly;
			}


			public ObjectHolder<TBase> Open()
			{
				return asReadOnly ? lifetime.GetReadOnlyBase() : lifetime.GetBase();
			}
		}
	}
}

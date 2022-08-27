using DidiFrame.Utils;

namespace DidiFrame.Lifetimes
{
	public abstract class AbstractLifetime<TBase> : ILifetime<TBase> where TBase : class, ILifetimeBase
	{
		protected delegate void LifetimeUpdatedHandler();

		protected delegate void LifetimeRanHandler(TBase initialBase, ILifetimeContext<TBase> context, InitialDataBuilder initialDataBuilder);

		protected delegate void LifetimePostRanHandler(TBase initialBase, InitialData initialData);


		private const string ContextField = "context";
		private const string ServerField = "server";
		private const string IdField = "id";
		private InitialData? data;
		private bool disposedValue;


		protected AbstractLifetime(ILogger logger)
		{
			Logger = logger;
		}


		protected event LifetimeRanHandler? LifetimeRan;

		protected event LifetimePostRanHandler? LifetimePostRan;

		protected event LifetimeUpdatedHandler? LifetimeUpdated;


		public bool IsNewlyCreated => LifetimeContext.IsNewlyCreated;

		public bool IsInitialized => data is not null;

		public bool IsFinalized { get; private set; }

		public IServer Server => Data.Get<IServer>(ServerField);

		public Guid Id => Data.Get<Guid>(IdField);

		protected InitialData Data => data ?? throw new InvalidOperationException("Enable to get data before run or in LifetimeRan event handler");

		protected LifetimeSynchronizationContext SynchronizationContext => LifetimeContext.GetSynchronizationContext();

		private ILifetimeContext<TBase> LifetimeContext => Data.Get<ILifetimeContext<TBase>>(ContextField);

		protected ILogger Logger { get; }


		public virtual void Destroy() { }

		public virtual void Dispose() { }

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

		public void Update()
		{
			LifetimeUpdated?.Invoke();
		}

		protected void FinalizeLifetime()
		{
			IsFinalized = true;
			LifetimeContext.FinalizeLifetime();
		}

		protected void CrashLifetime(Exception exception, bool isInvalidBase)
		{
			LifetimeContext.FinalizeLifetime();
			Logger.Log(LogLevel.Error, exception, "Lifetime {LifetimeId} crashed with error. Base object was {ValidStatus}", Id, isInvalidBase ? "Invalid" : "Valid");
		}

		protected void TerminateLifetime(string reason, Exception? exception = null)
		{
			CrashLifetime(new LifetimeTerminatedException(GetType(), Id, reason, exception), isInvalidBase: false);
		}

		protected void ThrowUnlessBuilden()
		{
			if (IsInitialized == false)
				throw new InvalidOperationException("This operation is blocked, Lifetime isn't builden");
		}

		protected void ThrowIfBuilden()
		{
			if (IsInitialized)
				throw new InvalidOperationException("This operation is blocked, Lifetime is already builden");
		}

		#region Base access methods
		protected virtual ObjectHolder<TBase> GetBase() => LifetimeContext.AccessBase().Open();

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
					throw new InvalidOperationException("Error, base was changed in readonly base accessor. Lifetime is collapsing");
				}
			});
		}

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


		protected sealed class InitialData
		{
			public InitialData(IReadOnlyDictionary<string, object> addititionalData)
			{
				AddititionalData = addititionalData;
			}


			public IReadOnlyDictionary<string, object> AddititionalData { get; }


			public T Get<T>(string key) => (T)AddititionalData[key];
		}

		protected sealed class InitialDataBuilder
		{
			private readonly Dictionary<string, object> data = new();


			public void AddData(string key, object obj) => data.Add(key, obj);

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

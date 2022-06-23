using System.Reflection;

namespace DidiFrame.ClientExtensions
{
	/// <summary>
	/// Reflection based implementation of DidiFrame.ClientExtensions.IClientExtensionFactory
	/// </summary>
	/// <typeparam name="TExtension">Type of extension interface</typeparam>
	/// <typeparam name="TImplementation">Type of extension implementation</typeparam>
	public class ReflectionClientExtensionFactory<TExtension, TImplementation> : IClientExtensionFactory<TExtension> where TExtension : class where TImplementation : TExtension
	{
		private readonly ConstructorInfo constructor;
		private TExtension? singleton;
		private readonly bool asSingleton;


		/// <summary>
		/// Creates new instance of DidiFrame.ClientExtensions.ReflectionClientExtensionFactory`2 that creates extensions using (TClient, IServiceProvider) ctor
		/// where TClient is IClient's inheritor, but isn't IClient itself
		/// </summary>
		/// <param name="asSingleton">If need create only one instance of extension</param>
		/// <exception cref="ArgumentException">If single ctor of extension is not (TClient, IServiceProvider) where TClient is IClient's inheritor, but isn't IClient itself</exception>
		public ReflectionClientExtensionFactory(bool asSingleton)
		{
			var typeOfImplementation = typeof(TImplementation);

			var ctor = typeOfImplementation.GetConstructors().Single();
			var pars = ctor.GetParameters();

			//Validation
			if (pars.Length != 2 || pars[0].ParameterType == typeof(IClient) ||
				!pars[0].ParameterType.IsAssignableTo(typeof(IClient)) || pars[1].ParameterType != typeof(IServiceProvider))
				throw new ArgumentException("Invalid ctor paramters. Excepted ctor: (TClient, IServiceProvider) where TClient is IClient's inheritor, but isn't IClient itself");

			TargetClientType = pars[1].ParameterType;
			constructor = ctor;
			this.asSingleton = asSingleton;
		}


		/// <inheritdoc/>
		public Type TargetClientType { get; }


		/// <inheritdoc/>
		public TExtension Create(IServiceProvider services, IClient client)
		{
			if (singleton is not null) return singleton;
			else
			{
				var obj = (TExtension)constructor.Invoke(new object[] { client, services });
				if (asSingleton) singleton = obj;
				return obj;
			}
		}
	}
}

using System.Reflection;

namespace DidiFrame.ClientExtensions
{
	public class ReflectionServerExtensionFactory<TExtension, TImplementation> : IServerExtensionFactory<TExtension> where TExtension : class where TImplementation : TExtension
	{
		private readonly ConstructorInfo constructor;
		private readonly bool asSingleton;
		private TExtension? singleton;


		public ReflectionServerExtensionFactory(bool asSingleton)
		{
			var typeOfImplementation = typeof(TImplementation);

			var ctor = typeOfImplementation.GetConstructors().Single();
			var pars = ctor.GetParameters();

			//Validation
			if (pars.Length != 2 || pars[0].ParameterType == typeof(IClient) ||
				!pars[0].ParameterType.IsAssignableTo(typeof(IServer)) || pars[1].ParameterType != typeof(IServiceProvider))
                throw new ArgumentException("Invalid ctor paramters. Excepted ctor: (TServer, IServiceProvider) where TServer is IServer's inheritor, but isn't IServer itself", nameof(TImplementation));

			TargetServerType = pars[1].ParameterType;
			constructor = ctor;
			this.asSingleton = asSingleton;
		}


		/// <inheritdoc/>
		public Type? TargetServerType { get; }


		/// <inheritdoc/>
		public TExtension Create(IServiceProvider services, IServer server)
		{
			//If works in singleton mode
			if (singleton is not null) return singleton;
			else
			{
				var obj = (TExtension)constructor.Invoke(new object[] { server, services });
				if (asSingleton) singleton = obj;
				return obj;
			}
		}
	}
}

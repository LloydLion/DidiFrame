using System.Reflection;

namespace DidiFrame.ClientExtensions
{
	public class ReflectionClientExtensionFactory<TExtension, TImplementation> : IClientExtensionFactory<TExtension> where TExtension : class where TImplementation : TExtension
	{
		private readonly ConstructorInfo constructor;
		private TExtension? singleton;
		private readonly bool asSingleton;


		public ReflectionClientExtensionFactory(bool asSingleton)
		{
			var typeOfImplementation = typeof(TImplementation);

			var ctor = typeOfImplementation.GetConstructors().Single();
			var pars = ctor.GetParameters();

			//Validation
			if (pars.Length != 2 || pars[0].ParameterType == typeof(IClient) ||
				!pars[0].ParameterType.IsAssignableTo(typeof(IClient)) || pars[1].ParameterType != typeof(IServiceProvider))
				throw new ArgumentException("Invalid ctor paramters. Excepted ctor: (TClient, IServiceProvider) where TClient is IClient's inheritor, but isn't IClient itself", nameof(TImplementation));

			TargetClientType = pars[1].ParameterType;
			constructor = ctor;
			this.asSingleton = asSingleton;
		}


		/// <inheritdoc/>
		public Type? TargetClientType { get; }


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

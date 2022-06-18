using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.ClientExtensions
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddClientExtension<TExtension, TImplementation>(this IServiceCollection services, bool asSingleton = true) where TExtension : class where TImplementation : TExtension
		{
			services.AddTransient<IClientExtensionFactory<TExtension>, ReflectionClientExtensionFactory<TExtension, TImplementation>>((s) => new(asSingleton));
			return services;
		}

		public static IServiceCollection AddServerExtension<TExtension, TImplementation>(this IServiceCollection services, bool asSingleton = true) where TExtension : class where TImplementation : TExtension
		{
			services.AddTransient<IServerExtensionFactory<TExtension>, ReflectionServerExtensionFactory<TExtension, TImplementation>>((s) => new(asSingleton));
			return services;
		}
	}
}

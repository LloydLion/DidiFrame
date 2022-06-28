using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.ClientExtensions
{
	/// <summary>
	/// Extensions for DidiFrame.ClientExtensions namespace
	/// </summary>
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds client extension to di container using DidiFrame.ClientExtensions.ReflectionClientExtensionFactory`2 factory
		/// </summary>
		/// <typeparam name="TExtension">Type of extension interface</typeparam>
		/// <typeparam name="TImplementation">Type of extension implementation</typeparam>
		/// <param name="services">Service collection</param>
		/// <param name="asSingleton">If need create only one instance of extension</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddClientExtension<TExtension, TImplementation>(this IServiceCollection services, bool asSingleton = true) where TExtension : class where TImplementation : TExtension
		{
			services.AddTransient<IClientExtensionFactory<TExtension>, ReflectionClientExtensionFactory<TExtension, TImplementation>>((s) => new(asSingleton));
			return services;
		}

		/// <summary>
		/// Adds server extension to di container using DidiFrame.ClientExtensions.ReflectionServerExtensionFactory`2 factory
		/// </summary>
		/// <typeparam name="TExtension">Type of extension interface</typeparam>
		/// <typeparam name="TImplementation">Type of extension implementation</typeparam>
		/// <param name="services">Service collection</param>
		/// <param name="asSingleton">If need create only one instance of extension</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddServerExtension<TExtension, TImplementation>(this IServiceCollection services, bool asSingleton = true) where TExtension : class where TImplementation : TExtension
		{
			services.AddTransient<IServerExtensionFactory<TExtension>, ReflectionServerExtensionFactory<TExtension, TImplementation>>((s) => new(asSingleton));
			return services;
		}
	}
}

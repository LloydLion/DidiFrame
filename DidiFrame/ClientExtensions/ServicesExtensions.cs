using DidiFrame.ClientExtensions.Reflection;
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
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddClientExtension<TExtension, TImplementation>(this IServiceCollection services) where TExtension : class where TImplementation : TExtension
		{
			services.AddSingleton<IClientExtensionFactory<TExtension>, ReflectionClientExtensionFactory<TExtension, TImplementation>>();
			services.AddSingleton<IClientExtensionFactory>(sp => sp.GetRequiredService<IClientExtensionFactory<TExtension>>());
			return services;
		}

		public static IServiceCollection AddClientExtensionCustom<TExtension, TFactory>(this IServiceCollection services) where TExtension : class where TFactory : class, IClientExtensionFactory<TExtension>
		{
			services.AddSingleton<IClientExtensionFactory<TExtension>, TFactory>();
			services.AddSingleton<IClientExtensionFactory>(sp => sp.GetRequiredService<IClientExtensionFactory<TExtension>>());
			return services;
		}

		/// <summary>
		/// Adds server extension to di container using DidiFrame.ClientExtensions.ReflectionServerExtensionFactory`2 factory
		/// </summary>
		/// <typeparam name="TExtension">Type of extension interface</typeparam>
		/// <typeparam name="TImplementation">Type of extension implementation</typeparam>
		/// <param name="services">Service collection</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddServerExtension<TExtension, TImplementation>(this IServiceCollection services) where TExtension : class where TImplementation : TExtension
		{
			services.AddSingleton<IServerExtensionFactory<TExtension>, ReflectionServerExtensionFactory<TExtension, TImplementation>>();
			services.AddSingleton<IServerExtensionFactory>(sp => sp.GetRequiredService<IServerExtensionFactory<TExtension>>());
			return services;
		}
		public static IServiceCollection AddServerExtensionCustom<TExtension, TFactory>(this IServiceCollection services) where TExtension : class where TFactory : class, IServerExtensionFactory<TExtension>
		{
			services.AddSingleton<IServerExtensionFactory<TExtension>, TFactory>();
			services.AddSingleton<IServerExtensionFactory>(sp => sp.GetRequiredService<IServerExtensionFactory<TExtension>>());
			return services;
		}
	}
}

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DidiFrame.Localization
{
	/// <summary>
	/// DidiFrame localization part. But you can use it :)
	/// </summary>
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds didiframe localization module into di container
		/// </summary>
		/// <param name="services">Services to add localization</param>
		/// <param name="optionsAction">Options action to configure localization</param>
		/// <param name="delegativeAssemblyName">Assmebly that can override didiframe localization, by default name of calling assembly</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddDidiFrameLocalization(this IServiceCollection services, Action<LocalizationOptions> optionsAction, string? delegativeAssemblyName = null)
		{
			if (delegativeAssemblyName is null) delegativeAssemblyName = Assembly.GetCallingAssembly().GetName().Name ?? throw new ImpossibleVariantException();

			services.Configure(optionsAction);
			services.AddSingleton<ResourceManagerStringLocalizerFactory>();
			services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
			services.AddSingleton<IStringLocalizerFactory, DidiFrameLocalizerFactory>(sp => new DidiFrameLocalizerFactory(sp.GetRequiredService<ResourceManagerStringLocalizerFactory>(), delegativeAssemblyName));

			return services;
		}

		/// <summary>
		/// Adds didiframe localization module into di container
		/// </summary>
		/// <typeparam name="TLocalizerFactory">Type of base localizer factory</typeparam>
		/// <param name="services">Services to add localization</param>
		/// <param name="delegativeAssemblyName">Assmebly that can override didiframe localization, by default name of calling assembly</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddDidiFrameLocalization<TLocalizerFactory>(this IServiceCollection services, string? delegativeAssemblyName = null)
			where TLocalizerFactory : class, IStringLocalizerFactory
		{
			if (delegativeAssemblyName is null) delegativeAssemblyName = Assembly.GetCallingAssembly().GetName().Name ?? throw new ImpossibleVariantException();

			services.AddSingleton<TLocalizerFactory>();
			services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
			services.AddSingleton<IStringLocalizerFactory, DidiFrameLocalizerFactory>(sp => new DidiFrameLocalizerFactory(sp.GetRequiredService<TLocalizerFactory>(), delegativeAssemblyName));

			return services;
		}
	}
}

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Localization
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddDidiFrameLocalization(this IServiceCollection services, Action<LocalizationOptions> optionsAction, string? delegativeAssemblyName = null)
		{
			if (delegativeAssemblyName is null) delegativeAssemblyName = Assembly.GetCallingAssembly().GetName().Name ?? throw new ImpossibleVariantException();

			services.Configure(optionsAction);
			services.AddSingleton<ResourceManagerStringLocalizerFactory>();
			services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
			services.AddSingleton<IStringLocalizerFactory, DidiFrameLocalizerFactory>(sp => new DidiFrameLocalizerFactory(sp.GetRequiredService<ResourceManagerStringLocalizerFactory>(), delegativeAssemblyName));

			return services;
		}
	}
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CGZBot3.Data.Json
{
	internal static class Extensions
	{
		public static string Serialize<T>(this JsonSerializer serializer, T? obj)
		{
			var strWriter = new StringWriter();
			using var json = new JsonTextWriter(strWriter);

			serializer.Serialize(json, obj);

			return strWriter.GetStringBuilder().ToString();
		}

		public static T Deserialize<T>(this JsonSerializer serializer, string str)
		{
			if (string.IsNullOrWhiteSpace(str)) str = "{}";
			var stringReader = new StringReader(str);
			using var json = new JsonTextReader(stringReader);

			return serializer.Deserialize<T>(json) ?? throw new ImpossibleVariantException();
		}

		public static IServiceCollection AddDataManagement(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<DataOptions>(configuration);
			services.AddSingleton<IServersSettingsRepositoryFactory, ServersSettingsRepositoryFactory>();
			services.AddSingleton<IServersStatesRepositoryFactory, ServersStatesRepositoryFactory>();
			return services;
		}
	}
}

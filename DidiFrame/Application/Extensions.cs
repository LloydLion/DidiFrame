using Microsoft.Extensions.Configuration;

namespace DidiFrame.Application
{
	public static class Extensions
	{
		public static void AddJsonConfiguration(this IDiscordApplicationBuilder builder, string fileName)
		{
			builder.AddConfiguration(new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(fileName).Build());
		}
	}
}

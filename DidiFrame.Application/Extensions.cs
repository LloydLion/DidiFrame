using Microsoft.Extensions.Configuration;

namespace DidiFrame.Application
{
	/// <summary>
	/// Extensions for DidiFrame.Application namespace
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Adds file json configuration to builder
		/// </summary>
		/// <param name="builder">Builder itself</param>
		/// <param name="fileName">Json file path relative current work directory</param>
		public static void AddJsonConfiguration(this IDiscordApplicationBuilder builder, string fileName)
		{
			builder.AddConfiguration(new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(fileName).Build());
		}
	}
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Application
{
	/// <summary>
	/// Builder for IDiscordApplication object
	/// </summary>
	public interface IDiscordApplicationBuilder
	{
		/// <summary>
		///	Creates a application instance
		/// </summary>
		/// <returns>Application that associated with builder</returns>
		public IDiscordApplication Build();

		/// <summary>
		/// Adds services into builder using Microsoft.Extensions.DependencyInjection.IServiceCollection class
		/// </summary>
		/// <param name="buildAction">Action under service collection</param>
		/// <returns>Instance of that object to be chained</returns>
		public IDiscordApplicationBuilder AddServices(Action<IServiceCollection> buildAction);

		/// <summary>
		/// Adds services into builder using Microsoft.Extensions.DependencyInjection.IServiceCollection class
		/// and configuration that must be added by AddConfiguration method
		/// </summary>
		/// <param name="buildAction">Action under service collection and configuration object</param>
		/// <returns>Instance of that object to be chained</returns>
		public IDiscordApplicationBuilder AddServices(Action<IServiceCollection, IConfiguration> buildAction);

		/// <summary>
		/// Adds configuration object to builder, can be used in AddServices(Action<IServiceCollection, IConfiguration>)
		/// to add configurated services
		/// </summary>
		/// <param name="configuration">Configuration itself</param>
		public void AddConfiguration(IConfigurationRoot configuration);

		/// <summary>
		/// Provides configuration objected that has added to builder using AddConfiguration(IConfigurationRoot)
		/// </summary>
		/// <returns>Provided configuration</returns>
		public IConfiguration GetConfiguration();

		/// <summary>
		/// Proivdes the time when builder was created
		/// </summary>
		/// <returns>DateTime object</returns>
		public DateTime GetStartupTime();

		/// <summary>
		/// Adds logging into services. It can be replaced by calling AddLogging(Action<ILoggingBuilder>) extension method at services object
		/// </summary>
		/// <param name="buildAction">Action under Microsoft.Extensions.Logging.ILoggingBuilder</param>
		public void AddLogging(Action<ILoggingBuilder> buildAction);
	}
}

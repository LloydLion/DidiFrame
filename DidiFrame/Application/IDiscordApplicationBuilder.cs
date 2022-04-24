using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Application
{
	public interface IDiscordApplicationBuilder
	{
		public IDiscordApplication Build();

		public IDiscordApplicationBuilder AddServices(Action<IServiceCollection> buildAction);

		public IDiscordApplicationBuilder AddServices(Action<IServiceCollection, IConfiguration> buildAction);

		public void AddConfiguration(IConfigurationRoot configuration);

		public IConfiguration GetConfiguration();

		public DateTime GetStartupTime();

		public void AddLogging(Action<ILoggingBuilder> buildAction);
	}
}

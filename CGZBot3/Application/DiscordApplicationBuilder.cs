using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Application
{
	public class DiscordApplicationBuilder : IDiscordApplicationBuilder
	{
		private readonly IServiceCollection services = new ServiceCollection();
		private readonly DateTime now;
		private IConfigurationRoot? configuration;


		private DiscordApplicationBuilder()
		{
			now = DateTime.Now;
		}


		public IDiscordApplication Build()
		{
			return new DiscordApplication(services.BuildServiceProvider(), now);
		}

		public IConfiguration GetConfiguration()
		{
			if (configuration is null)
				throw new InvalidOperationException("No configuration added");

			return configuration;
		}

		public void AddConfiguration(IConfigurationRoot configuration)
		{
			if (this.configuration is not null)
				throw new InvalidOperationException("Enable to add configuration twice");
			this.configuration = configuration;
		}

		public IDiscordApplicationBuilder AddServices(Action<IServiceCollection, IConfiguration> buildAction)
		{
			if (configuration is null) throw new InvalidOperationException("Add configuration itself before add services using configuration or add services without configuration");

			buildAction(services, configuration);
			return this;
		}

		public IDiscordApplicationBuilder AddServices(Action<IServiceCollection> buildAction)
		{
			buildAction(services);
			return this;
		}

		public void AddLogging(Action<ILoggingBuilder> buildAction)
		{
			services.AddLogging(buildAction);
		}

		public DateTime GetStartupTime() => now;


		public static IDiscordApplicationBuilder Create()
		{
			return new DiscordApplicationBuilder();
		}
	}
}

using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.Application
{
	/// <summary>
	/// Default bilder for didiFrame project. Uses a provided in services client
	/// </summary>
	public class DiscordApplicationBuilder : IDiscordApplicationBuilder
	{
		private readonly IServiceCollection services = new ServiceCollection();
		private readonly DateTime now;
		private IConfigurationRoot? configuration;


		private DiscordApplicationBuilder()
		{
			now = DateTime.Now;
			services.AddValidatorsFromAssemblyContaining<DiscordApplicationBuilder>(ServiceLifetime.Transient, includeInternalTypes: true);
		}


		/// <inheritdoc/>
		public IDiscordApplication Build()
		{
			return new DiscordApplication(services.BuildServiceProvider(), now);
		}

		/// <inheritdoc/>
		public IConfiguration GetConfiguration()
		{
			if (configuration is null)
				throw new InvalidOperationException("No configuration added");

			return configuration;
		}

		/// <inheritdoc/>
		public void AddConfiguration(IConfigurationRoot configuration)
		{
			if (this.configuration is not null)
				throw new InvalidOperationException("Enable to add configuration twice");
			this.configuration = configuration;
		}

		/// <inheritdoc/>
		public IDiscordApplicationBuilder AddServices(Action<IServiceCollection, IConfiguration> buildAction)
		{
			if (configuration is null) throw new InvalidOperationException("Add configuration itself before add services using configuration or add services without configuration");

			buildAction(services, configuration);
			return this;
		}

		/// <inheritdoc/>
		public IDiscordApplicationBuilder AddServices(Action<IServiceCollection> buildAction)
		{
			buildAction(services);
			return this;
		}

		/// <inheritdoc/>
		public void AddLogging(Action<ILoggingBuilder> buildAction)
		{
			services.AddLogging(buildAction);
		}

		/// <inheritdoc/>
		public DateTime GetStartupTime() => now;


		/// <summary>
		/// Creates a default DidiFrame.Application.DiscordApplicationBuilder
		/// </summary>
		/// <returns>Instance of builder</returns>
		public static IDiscordApplicationBuilder Create()
		{
			return new DiscordApplicationBuilder();
		}
	}
}

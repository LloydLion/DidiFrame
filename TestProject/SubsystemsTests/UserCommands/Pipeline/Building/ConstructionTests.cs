using DidiFrame.Culture;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.Executing;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline.Building;
using DidiFrame.UserCommands.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using TestProject.Environment.Culture;
using TestProject.Environment.Locale;
using TestProject.Environment.UserCommands;

namespace TestProject.SubsystemsTests.UserCommands.Pipeline.Building
{
	public class ConstructionTests
	{
		[Fact]
		public void SimplePipeline()
		{
			var services = new ServiceCollection()

				.AddLogging()
				.AddTransient<IServerCultureProvider, TestCultureProvider>()
				.AddSingleton(Options.Create(new DefaultUserCommandsExecutor.Options()
				{ UnspecifiedErrorMessage = DefaultUserCommandsExecutor.Options.UnspecifiedErrorMessageBehavior.Disable }))

				.AddUserCommandPipeline(builder =>
				{
					builder
						.SetSource(_ => new NullDispatcher<ValidatedUserCommandContext>())
						.AddMiddleware<ValidatedUserCommandContext, UserCommandResult, DefaultUserCommandsExecutor>(true)
						.Build();
				})
				.BuildServiceProvider();

			_ = services.GetRequiredService<IUserCommandPipelineBuilder>().Build(services);
		}

		[Fact]
		public void ClassicPipeline()
		{
			var config = new ConfigurationBuilder().Add(new MemoryConfigurationSource()
			{
				InitialData = new Dictionary<string, string>()
				{
					{ "Parser.Prefixes", "\\"},
					{ "Executor.UnspecifiedErrorMessage", "Disable"},
				}
			}).Build();

			var services = new ServiceCollection()
				.AddSingleton<IClient, Client>()
				.AddSingleton<IUserCommandsRepository, SimpleUserCommandsRepository>()
				.AddLogging()
				.AddTransient<IServerCultureProvider, TestCultureProvider>()
				.AddTransient(typeof(IStringLocalizer<>), typeof(TestLocalizer<>))

				.AddClassicMessageUserCommandPipeline(config.GetSection("Parser"), config.GetSection("Executor"))
				.BuildServiceProvider();

			_ = services.GetRequiredService<IUserCommandPipelineBuilder>().Build(services);
		}
	}
}

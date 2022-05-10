using DidiFrame.Culture;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ContextValidation.Arguments;
using DidiFrame.UserCommands.ContextValidation.Invoker;
using DidiFrame.UserCommands.Executing;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.Pipeline.Building;
using DidiFrame.UserCommands.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestProject.Environment.Culture;
using TestProject.Environment.Locale;
using TestProject.Environment.UserCommands;

namespace TestProject.SubsystemsTests.UserCommands.Pipeline
{
	public class ExecutionTests
	{
		[Fact]
		public void BuildAndExecutePipeline()
		{
			var client = new Client();
			var server = new Server(client, "The server");
			var user = new User(client, "The people", false);
			var user2 = new User(client, "The people 2", false);
			var member = server.AddMember(user, Permissions.All);
			var member2 = server.AddMember(user, Permissions.All);
			var gcat = server.Categories.First();
			var channel = new TextChannel("the-channel", gcat);
			gcat.BaseChannels.Add(channel);

			var rep = (IUserCommandsRepository)new SimpleUserCommandsRepository();

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "argument", Array.Empty<IUserCommandArgumentValidator>());
			var command = new UserCommandInfo("command with-no-standart-name", CommandHandler, new UserCommandArgument[] { argument }, new TestLocalizer(), Array.Empty<IUserCommandInvokerFilter>());
			rep.AddCommand(command);

			rep.Bulk();


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

			//-----------------

			var ctx = new UserCommandContext(member, channel, command, new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>() { { argument, new(argument, member2, new[] { member2 }) } });
			var executor = services.GetRequiredService<IUserCommandPipelineExecutor>();
			var ucr = executor.Process(services.GetRequiredService<IUserCommandPipelineBuilder>().Build(services), new ValidatedUserCommandContext(ctx), new(member, channel));

			//-----------------

			Assert.NotNull(ucr);
			Assert.NotNull(ucr?.RespondMessage);
			Assert.Equal($"Hello {user.Mention}!", ucr?.RespondMessage?.Content);
		}


		private Task<UserCommandResult> CommandHandler(UserCommandContext ctx)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new DidiFrame.Entities.Message.MessageSendModel($"Hello {ctx.Invoker.Mention}!") });
		}
	}
}

using DidiFrame.Culture;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ContextValidation.Arguments;
using DidiFrame.UserCommands.ContextValidation.Invoker;
using DidiFrame.UserCommands.Executing;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.UserCommands.Pipeline.Building;
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
	public class LocalServicesTests
	{
		[Fact]
		public void SimplePipeline()
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


			var services = new ServiceCollection()
				.AddLogging()
				.AddTransient<IServerCultureProvider, TestCultureProvider>()
				.AddSingleton(typeof(GlobalVar<>))
				.AddSingleton(Options.Create(new DefaultUserCommandsExecutor.Options()
				{ UnspecifiedErrorMessage = DefaultUserCommandsExecutor.Options.UnspecifiedErrorMessageBehavior.Disable }))

				.AddUserCommandPipeline(builder =>
				{
					builder
						.SetSource(_ => new NullDispatcher<ValidatedUserCommandContext>())
						.AddMiddleware<ValidatedUserCommandContext, UserCommandResult, DefaultUserCommandsExecutor>(true)
						.Build();
				})

				.AddUserCommandLocalService<MyService>()
				.BuildServiceProvider();

			var pipeline = services.GetRequiredService<IUserCommandPipelineBuilder>().Build(services);

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "argument", Array.Empty<IUserCommandArgumentValidator>());
			var command = new UserCommandInfo("command with-no-standart-name", CommandHandler, new UserCommandArgument[] { argument }, new TestLocalizer(), Array.Empty<IUserCommandInvokerFilter>());

			//----------------

			Assert.Equal(0, services.GetRequiredService<GlobalVar<int>>().Value);
			Assert.False(services.GetRequiredService<GlobalVar<bool>>().Value);

			//----------------

			var ctx = new UserCommandContext(member, channel, command, new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>() { { argument, new(argument, member2, new[] { member2 }) } });
			services.GetRequiredService<IUserCommandPipelineExecutor>().Process(pipeline, new ValidatedUserCommandContext(ctx), new(ctx.Invoker, ctx.Channel));

			//----------------

			Assert.Equal(3, services.GetRequiredService<GlobalVar<int>>().Value);
			Assert.True(services.GetRequiredService<GlobalVar<bool>>().Value);
		}

		private static Task<UserCommandResult> CommandHandler(UserCommandContext ctx)
		{
			ctx.GetLocalServices().GetRequiredService<MyService>().SetFlag();
			ctx.GetLocalServices().GetRequiredService<MyService>().ResetFlag();
			ctx.GetLocalServices().GetRequiredService<MyService>().SetFlag();
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful));
		}


		private class MyService : IDisposable
		{
			private readonly GlobalVar<bool> gv;
			private readonly GlobalVar<int> fc;


			public bool Flag { get => gv.Value; private set => gv.Value = value; }


			public MyService(IServiceProvider sp)
			{
				gv = sp.GetRequiredService<GlobalVar<bool>>();
				fc = sp.GetRequiredService<GlobalVar<int>>();
			}


			public void SetFlag()
			{
				Flag = true;
			}

			public void ResetFlag()
			{
				Flag = false;
			}

			public void Dispose()
			{
				fc.Value = 3;
			}
		}

		private class GlobalVar<T>
		{
			public T? Value { get; set; }
		}
	}
}

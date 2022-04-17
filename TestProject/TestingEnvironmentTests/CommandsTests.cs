using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation;
using CGZBot3.UserCommands.InvokerFiltartion;
using CGZBot3.UserCommands.Loader.Reflection;
using CGZBot3.UserCommands.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestProject.Environment.Culture;
using TestProject.Environment.Locale;

namespace TestProject.TestingEnvironmentTests
{
	public class CommandsTests
	{
		[Fact]
		public void RegisterCommand()
		{
			var client = new Client();
			var server = new Server(client, "The server");

			var rep = (IUserCommandsRepository)new SimpleUserCommandsRepository(new CommandInfoValidator(new CommandArgumentValidator()));
			rep.AddCommand(new UserCommandInfo("command with-no-standart-name", CommandHandler, new UserCommandInfo.Argument[]
			{
				new UserCommandInfo.Argument(false, UserCommandInfo.Argument.Type.Member, "argument", Array.Empty<IUserCommandArgumentValidator>())
			}, new TestLocalizer(), Array.Empty<IUserCommandInvokerFilter>()));

			rep.Bulk();

			//-----------------

			var cmds = rep.GetCommandsFor(server);
			Assert.Equal("command with-no-standart-name", cmds.Single().Name);
		}

		[Fact]
		public void LoadCommand()
		{
			var client = new Client();
			var server = new Server(client, "The server");

			var handlers = new ServiceCollection()
			.AddSingleton<ICommandsHandler, CommandsHanlder>()
			.BuildServiceProvider();

			var loader = new ReflectionUserCommandsLoader(handlers, new NullLogger<ReflectionUserCommandsLoader>(), new TestLocalizerFactory());
			
			var rep = (IUserCommandsRepository)new SimpleUserCommandsRepository(new CommandInfoValidator(new CommandArgumentValidator()));

			loader.LoadTo(rep);

			rep.Bulk();

			//-----------------

			var cmds = rep.GetCommandsFor(server);
			var cmd = cmds.Single();

			Assert.Equal("do sth", cmd.Name);
			Assert.Equal(1, cmd.Arguments.Count);
			Assert.Equal("theStr", cmd.Arguments.Single().Name);
			Assert.Equal(UserCommandInfo.Argument.Type.String, cmd.Arguments.Single().ArgumentType);
			Assert.False(cmd.Arguments.Single().IsArray);
		}

		[Fact]
		public void ExecuteCommand()
		{
			var client = new Client();
			var server = new Server(client, "The server");
			var user = new User(client, "The people", false);
			var member = server.AddMember(user, Permissions.All);
			var gcat = server.Categories.First();
			var channel = new TextChannel("the-channel", gcat);
			gcat.BaseChannels.Add(channel);

			var rep = (IUserCommandsRepository)new SimpleUserCommandsRepository(new CommandInfoValidator(new CommandArgumentValidator()));

			var argument = new UserCommandInfo.Argument(false, UserCommandInfo.Argument.Type.Member, "argument", Array.Empty<IUserCommandArgumentValidator>());
			var command = new UserCommandInfo("command with-no-standart-name", CommandHandler, new UserCommandInfo.Argument[] { argument }, new TestLocalizer(), Array.Empty<IUserCommandInvokerFilter>());
			rep.AddCommand(command);

			rep.Bulk();

			var handler = new DefaultUserCommandsHandler(Options.Create(new DefaultUserCommandsHandler.Options()), new NullLogger<DefaultUserCommandsHandler>(),
				new CultureProvider(), new TestLocalizer<DefaultUserCommandsHandler>(), new ServiceCollection().BuildServiceProvider());

			client.CommandsDispatcher.CommandWritten += async (ctx, callback) => { await handler.HandleAsync(ctx, callback); };

			//-----------------

			var result = client.BaseCommandsDispatcher.SendCommand(new UserCommandContext(member, channel, command, new Dictionary<UserCommandInfo.Argument, object> { { argument, member } }));
			Assert.NotNull(result.RespondMessage);
			Assert.Equal($"Hello {user.Mention}!", result.RespondMessage?.Content);
		}


		private Task<UserCommandResult> CommandHandler(UserCommandContext ctx)
		{
			return Task.FromResult(new UserCommandResult(UserCommandCode.Sucssesful) { RespondMessage = new CGZBot3.Entities.Message.MessageSendModel($"Hello {ctx.Invoker.Mention}!") });
		}


		private class CommandsHanlder : ICommandsHandler
		{
			[Command("do sth")]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822")]
			public UserCommandResult CommandHandler(UserCommandContext _1, string _2)
			{
				return new UserCommandResult(UserCommandCode.Sucssesful);
			}
		}
	}
}

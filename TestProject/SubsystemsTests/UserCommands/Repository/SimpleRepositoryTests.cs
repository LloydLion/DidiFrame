#define DisableLocalCommandTest
using DidiFrame.UserCommands.ContextValidation.Invoker;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Repository;
using System;
using System.Linq;
using TestProject.Environment.Locale;
using TestProject.Environment.UserCommands;

namespace TestProject.SubsystemsTests.UserCommands.Repository
{
	public class SimpleRepositoryTests
	{
		[Fact]
		public void AddGlobalCommand()
		{
			var client = new Client();
			var server1 = new Server(client, "The server 1");
			var server2 = new Server(client, "The server 2");

			var rep = (IUserCommandsRepository)new SimpleUserCommandsRepository();

			var cmd1 = new UserCommandInfo("the cmd-a", NoHandler.Handler, Array.Empty<UserCommandArgument>(), new TestLocalizer(), Array.Empty<IUserCommandInvokerFilter>());
			var cmd2 = new UserCommandInfo("the cmd-b", NoHandler.Handler, Array.Empty<UserCommandArgument>(), new TestLocalizer(), Array.Empty<IUserCommandInvokerFilter>());

			//-------------------

			rep.AddCommand(cmd1);
			rep.AddCommand(cmd2);

			rep.Bulk();

			//-------------------

			var list1 = rep.GetCommandsFor(server1);
			var list2 = rep.GetCommandsFor(server2);

			Assert.True(list1.SequenceEqual(list2));
		}

#if !DisableLocalCommandTest
		[Fact]
		public void AddLocalCommand()
		{
			var client = new Client();
			var server1 = new Server(client, "The server 1");
			var server2 = new Server(client, "The server 2");

			var rep = (IUserCommandsRepository)new SimpleUserCommandsRepository();

			var cmd1 = new UserCommandInfo("the cmd-a", NoHandler.Handler, Array.Empty<UserCommandArgument>(), new TestLocalizer(), Array.Empty<IUserCommandInvokerFilter>());
			var cmd2 = new UserCommandInfo("the cmd-b", NoHandler.Handler, Array.Empty<UserCommandArgument>(), new TestLocalizer(), Array.Empty<IUserCommandInvokerFilter>());
			var cmd3 = new UserCommandInfo("the cmd-c", NoHandler.Handler, Array.Empty<UserCommandArgument>(), new TestLocalizer(), Array.Empty<IUserCommandInvokerFilter>());

			//-------------------

			rep.AddCommand(cmd1, server1);
			rep.AddCommand(cmd2, server2);
			rep.AddCommand(cmd3);

			rep.Bulk();

			//-------------------

			var list1 = rep.GetCommandsFor(server1);
			var list2 = rep.GetCommandsFor(server2);

			Assert.Equal(2, list1.Count);
			Assert.Equal(2, list2.Count);
			Assert.Single(list2.Intersect(list1));
			Assert.Single(list2.Except(list1));
			Assert.Single(list1.Except(list2));
		}
#endif

		[Fact]
		public void FailtureAdd()
		{
			var client = new Client();

			var rep = (IUserCommandsRepository)new SimpleUserCommandsRepository();

			var cmd = new UserCommandInfo("the cmd", NoHandler.Handler, Array.Empty<UserCommandArgument>(), new TestLocalizer(), Array.Empty<IUserCommandInvokerFilter>());

			//-------------------

			rep.Bulk();

			Assert.Throws<InvalidOperationException>(() => rep.AddCommand(cmd));
		}

		//[Fact]
		//public void LoadCommand()
		//{
		//	var client = new Client();
		//	var server = new Server(client, "The server");

		//	var handlers = new ServiceCollection()
		//		.AddSingleton<ICommandsHandler, CommandsHanlder>()
		//		.BuildServiceProvider();

		//	var converter = new DefaultUserCommandContextConverter(handlers, Array.Empty<IDefaultContextConveterSubConverter>(), new TestLocalizer<DefaultUserCommandContextConverter>());

		//	var loader = new ReflectionUserCommandsLoader(handlers, new NullLogger<ReflectionUserCommandsLoader>(), new TestLocalizerFactory(), converter);

		//	var rep = (IUserCommandsRepository)new SimpleUserCommandsRepository();

		//	loader.LoadTo(rep);

		//	rep.Bulk();

		//	//-----------------

		//	var cmds = rep.GetCommandsFor(server);
		//	var cmd = cmds.Single();

		//	Assert.Equal("do sth", cmd.Name);
		//	Assert.Equal(1, cmd.Arguments.Count);
		//	Assert.Equal("theStr", cmd.Arguments.Single().Name);
		//	Assert.Equal(UserCommandArgument.Type.String, cmd.Arguments.Single().OriginTypes.Single());
		//	Assert.Equal(typeof(string), cmd.Arguments.Single().TargetType);
		//	Assert.False(cmd.Arguments.Single().IsArray);
		//}
	}
}

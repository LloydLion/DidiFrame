using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Repository;
using DidiFrame.Utils.Collections;
using DidiFrame.Utils.ExtendableModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestProject.SubsystemsTests.UserCommands.Repository
{
	public abstract class IUserCommandsRepositoryTests
	{
		public abstract IUserCommandsRepository CreateRepository();


		[Test]
		public void AddGlobalCommand()
		{
			var client = new Client();
			var server1 = client.CreateServer();
			var server2 = client.CreateServer();

			var rep = CreateRepository();

			var cmd1 = new UserCommandInfo("the cmd-a", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd2 = new UserCommandInfo("the cmd-b", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			//-------------------

			rep.AddCommand(cmd1);
			rep.AddCommand(cmd2);

			//-------------------

			CollectionAssert.AreEquivalent(rep.GetFullCommandList(server2), rep.GetFullCommandList(server1));

			Assert.That(rep.GetCommandsFor(server1), Is.Empty);
			Assert.That(rep.GetCommandsFor(server2), Is.Empty);
		}

		[Test]
		public void AddLocalCommand()
		{
			var client = new Client();
			var server1 = client.CreateServer();
			var server2 = client.CreateServer();
			var server3 = client.CreateServer();

			var rep = CreateRepository();

			var cmd1 = new UserCommandInfo("the cmd-a", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd2 = new UserCommandInfo("the cmd-b", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd3 = new UserCommandInfo("the cmd-c", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			//-------------------

			rep.AddCommand(cmd1, server1);
			rep.AddCommand(cmd2, server2);
			rep.AddCommand(cmd3);

			//-------------------

			CollectionAssert.AreEquivalent(new[] { cmd1, cmd3 }, rep.GetFullCommandList(server1));
			CollectionAssert.AreEquivalent(new[] { cmd2, cmd3 }, rep.GetFullCommandList(server2));
			CollectionAssert.AreEquivalent(cmd3.StoreSingle(), rep.GetFullCommandList(server3));

			CollectionAssert.AreEquivalent(cmd1.StoreSingle(), rep.GetCommandsFor(server1));
			CollectionAssert.AreEquivalent(cmd2.StoreSingle(), rep.GetCommandsFor(server2));
			Assert.That(rep.GetCommandsFor(server3), Is.Empty);
		}


		[Test]
		public void FailtureAdd()
		{
			var client = new Client();
			var server = client.CreateServer();


			//Same name
			var cmd1 = new UserCommandInfo("the cmd-b", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd2 = new UserCommandInfo("the cmd-b", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			//-------------------

			var rep1 = CreateRepository();

			rep1.AddCommand(cmd1, server);
			Assert.Throws<ArgumentException>(() => rep1.AddCommand(cmd2, server));

			//-------------------

			var rep2 = CreateRepository();

			rep2.AddCommand(cmd1);
			Assert.Throws<ArgumentException>(() => rep2.AddCommand(cmd2));

			//-------------------

			var rep3 = CreateRepository();

			rep3.AddCommand(cmd1);
			Assert.Throws<ArgumentException>(() => rep2.AddCommand(cmd2, server));

			//-------------------

			var rep4 = CreateRepository();

			rep4.AddCommand(cmd1, server);
			Assert.Throws<ArgumentException>(() => rep2.AddCommand(cmd2));
		}

		[Test]
		public void GetCommandByName()
		{
			var client = new Client();
			var server1 = client.CreateServer();
			var server2 = client.CreateServer();
			var server3 = client.CreateServer();

			var rep = CreateRepository();

			var cmd1 = new UserCommandInfo("the cmd-a", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd2 = new UserCommandInfo("the cmd-b", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd3 = new UserCommandInfo("the cmd-c", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			//-------------------

			rep.AddCommand(cmd1, server1);
			rep.AddCommand(cmd2, server2);
			rep.AddCommand(cmd3);

			//-------------------

			//Global
			Assert.Throws<KeyNotFoundException>(() => rep.GetGlobalCommands().GetCommad("the cmd-a"));
			Assert.Throws<KeyNotFoundException>(() => rep.GetGlobalCommands().GetCommad("the cmd-b"));
			Assert.That(rep.GetGlobalCommands().GetCommad("the cmd-c"), Is.EqualTo(cmd3));

			//Server 1
			Assert.That(rep.GetFullCommandList(server1).GetCommad("the cmd-a"), Is.EqualTo(cmd1));
			Assert.Throws<KeyNotFoundException>(() => rep.GetFullCommandList(server1).GetCommad("the cmd-b"));
			Assert.That(rep.GetFullCommandList(server1).GetCommad("the cmd-c"), Is.EqualTo(cmd3));

			Assert.That(rep.GetCommandsFor(server1).GetCommad("the cmd-a"), Is.EqualTo(cmd1));
			Assert.Throws<KeyNotFoundException>(() => rep.GetCommandsFor(server1).GetCommad("the cmd-b"));
			Assert.Throws<KeyNotFoundException>(() => rep.GetCommandsFor(server1).GetCommad("the cmd-c"));

			//Server 2
			Assert.Throws<KeyNotFoundException>(() => rep.GetFullCommandList(server2).GetCommad("the cmd-a"));
			Assert.That(rep.GetFullCommandList(server2).GetCommad("the cmd-b"), Is.EqualTo(cmd2));
			Assert.That(rep.GetFullCommandList(server2).GetCommad("the cmd-c"), Is.EqualTo(cmd3));

			Assert.Throws<KeyNotFoundException>(() => rep.GetCommandsFor(server2).GetCommad("the cmd-a"));
			Assert.That(rep.GetCommandsFor(server2).GetCommad("the cmd-b"), Is.EqualTo(cmd2));
			Assert.Throws<KeyNotFoundException>(() => rep.GetCommandsFor(server2).GetCommad("the cmd-c"));

			//Server 3
			Assert.Throws<KeyNotFoundException>(() => rep.GetFullCommandList(server3).GetCommad("the cmd-a"));
			Assert.Throws<KeyNotFoundException>(() => rep.GetFullCommandList(server3).GetCommad("the cmd-b"));
			Assert.That(rep.GetFullCommandList(server3).GetCommad("the cmd-c"), Is.EqualTo(cmd3));

			Assert.Throws<KeyNotFoundException>(() => rep.GetCommandsFor(server3).GetCommad("the cmd-a"));
			Assert.Throws<KeyNotFoundException>(() => rep.GetCommandsFor(server3).GetCommad("the cmd-b"));
			Assert.Throws<KeyNotFoundException>(() => rep.GetCommandsFor(server3).GetCommad("the cmd-c"));
		}

		[Test]
		public void TryGetCommand()
		{
			var rep = CreateRepository();

			var cmd1 = new UserCommandInfo("the cmd-a", new NoHandler().InstanceHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			rep.AddCommand(cmd1);

			var collection = rep.GetGlobalCommands();

			//-------------------

			Assert.Throws<KeyNotFoundException>(() => collection.GetCommad("the cmd-b"));
			Assert.That(collection.TryGetCommad("the cmd-b", out _), Is.False);

			var result = collection.TryGetCommad("the cmd-a", out var existingCommand);
			Assert.That(result, Is.True);
			Assert.That(existingCommand, Is.EqualTo(cmd1));
		}
	}
}

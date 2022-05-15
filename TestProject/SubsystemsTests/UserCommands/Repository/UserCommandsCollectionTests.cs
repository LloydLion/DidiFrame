using DidiFrame.UserCommands.ContextValidation.Invoker;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Repository;
using DidiFrame.Utils.ExtendableModels;
using System;
using System.Collections.Generic;
using TestProject.Environment.Locale;
using TestProject.Environment.UserCommands;

namespace TestProject.SubsystemsTests.UserCommands.Repository
{
	public class UserCommandsCollectionTests
	{
		[Fact]
		public void Construction()
		{
			var cmd1 = new UserCommandInfo("the cmd-a", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd2 = new UserCommandInfo("the cmd-b", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd3 = new UserCommandInfo("the cmd-c", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			var baseCollection = new[] { cmd1, cmd2, cmd3 };

			//--------------

			var collection = new UserCommandsCollection(baseCollection);
		}
		
		[Fact]
		public void FailtureConstruction()
		{
			//Same name
			var cmd1 = new UserCommandInfo("the cmd-a", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd2 = new UserCommandInfo("the cmd-a", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd3 = new UserCommandInfo("the cmd-a", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			var baseCollection = new[] { cmd1, cmd2, cmd3 };

			//--------------

			Assert.Throws<ArgumentException>(() => new UserCommandsCollection(baseCollection));
		}

		[Fact]
		public void Getting()
		{
			var cmd1 = new UserCommandInfo("the cmd-a", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd2 = new UserCommandInfo("the cmd-b", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd3 = new UserCommandInfo("the cmd-c", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			var baseCollection = new[] { cmd1, cmd2, cmd3 };

			//--------------

			var collection = new UserCommandsCollection(baseCollection);

			//--------------

			Assert.Equal(cmd1, collection.GetCommad(cmd1.Name));
			Assert.Equal(cmd2, collection.GetCommad(cmd2.Name));
			Assert.Equal(cmd3, collection.GetCommad(cmd3.Name));
		}

		[Fact]
		public void FailtureGetting()
		{
			var cmd1 = new UserCommandInfo("the cmd-a", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd2 = new UserCommandInfo("the cmd-b", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);
			var cmd3 = new UserCommandInfo("the cmd-c", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			var baseCollection = new[] { cmd1, cmd2, cmd3 };

			//--------------

			var collection = new UserCommandsCollection(baseCollection);

			//--------------

			Assert.Throws<KeyNotFoundException>(() => collection.GetCommad("no name"));
			Assert.False(collection.TryGetCommad("no name", out _));
		}
	}
}

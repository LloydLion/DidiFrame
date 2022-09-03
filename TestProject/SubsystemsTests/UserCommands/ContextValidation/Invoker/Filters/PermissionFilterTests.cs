using DidiFrame.UserCommands.ContextValidation.Invoker.Filters;
using DidiFrame.UserCommands.Models;
using DidiFrame.Utils;
using DidiFrame.Utils.ExtendableModels;
using System;
using System.Collections.Generic;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Invoker.Filters
{
	public class PermissionFilterTests
	{
		public PermissionFilter CreateInstance()
		{
			return new PermissionFilter(Permissions.AccessChannels);
		}

		[Test]
		public void ValidMember()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));


			var command = new UserCommandInfo("thecmd", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>(), SimpleModelAdditionalInfoProvider.Empty));

			var filter = CreateInstance();

			//-------------------

			var workResult = filter.Filter(commandContext, EmptyServiceProvider.Instance);

			//-------------------

			Assert.That(workResult, Is.Null);
		}

		[Test]
		public void UnvalidMember()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.None);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));


			var command = new UserCommandInfo("thecmd", NoHandler.Handler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>(), SimpleModelAdditionalInfoProvider.Empty));

			var filter = CreateInstance();

			//-------------------

			var workResult = filter.Filter(commandContext, EmptyServiceProvider.Instance);

			//-------------------

			Assert.That(workResult, Is.Not.Null);
			Assert.That(workResult.ErrorCode, Is.EqualTo("NoPermissions"));
		}

	}
}

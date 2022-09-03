using DidiFrame.UserCommands.ContextValidation.Invoker.Filters;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.Utils.ExtendableModels;
using DidiFrame.Utils;
using System;
using System.Collections.Generic;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Invoker.Filters
{
	public class DisableTests
	{
		public Disable CreateInstance()
		{
			return new Disable();
		}

		[Test]
		public void MainTest()
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

			Assert.That(workResult, Is.Not.Null);
			Assert.That(workResult.ErrorCode, Is.EqualTo("CommandDisabled"));
		}
	}
}

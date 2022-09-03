using DidiFrame.UserCommands.ContextValidation.Arguments.Validators;
using DidiFrame.UserCommands.Models;
using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using DidiFrame.Utils.ExtendableModels;
using System.Collections.Generic;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Arguments.Validators
{
	public class NoBotTests
	{
		public NoBot CreateInstance()
		{
			return new NoBot();
		}

		[Test]
		public void ArgumentIsBot()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var otherMember = server.AddMember("Other", true, Permissions.All); //Not member
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "arg", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, otherMember, new[] { otherMember }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var validator = CreateInstance();

			//-------------------

			var workResult = validator.Validate(commandContext, new(argument, otherMember, new[] { otherMember }), EmptyServiceProvider.Instance);

			//-------------------

			Assert.That(workResult, Is.Not.Null);
			Assert.That(workResult.ErrorCode, Is.EqualTo("IsBot"));
		}

		[Test]
		public void ArgumentIsNotBot()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var otherMember = server.AddMember("Other", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "arg", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, otherMember, new[] { otherMember }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var validator = CreateInstance();

			//-------------------

			var workResult = validator.Validate(commandContext, new(argument, otherMember, new[] { otherMember }), EmptyServiceProvider.Instance);

			//-------------------

			Assert.That(workResult, Is.Null);
		}
	}
}

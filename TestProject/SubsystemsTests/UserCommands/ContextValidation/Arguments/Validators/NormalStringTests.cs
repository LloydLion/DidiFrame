using DidiFrame.UserCommands.ContextValidation.Arguments.Validators;
using DidiFrame.UserCommands.Models;
using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using DidiFrame.Utils.ExtendableModels;
using System.Collections.Generic;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Arguments.Validators
{
	public class NormalStringTests
	{
		public NormalString CreateInstance()
		{
			return new NormalString();
		}

		[Test]
		public void InvalidString()
		{
			const string emptyString = "\t\n   \n	";

			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.String }, typeof(string), "arg", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, emptyString, new[] { emptyString }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var validator = CreateInstance();

			//-------------------

			var workResult = validator.Validate(commandContext, new(argument, emptyString, new[] { emptyString }), EmptyServiceProvider.Instance);

			//-------------------

			Assert.That(workResult, Is.Not.Null);
			Assert.That(workResult.ErrorCode, Is.EqualTo("WhiteSpace"));
		}

		[Test]
		public void NormalString()
		{
			const string validString = "absd DNMSDE dsa@  3@ ! @ # # # ";

			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.String }, typeof(string), "arg", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, validString, new[] { validString }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var validator = CreateInstance();

			//-------------------

			var workResult = validator.Validate(commandContext, new(argument, validString, new[] { validString }), EmptyServiceProvider.Instance);

			//-------------------

			Assert.That(workResult, Is.Null);
		}
	}
}

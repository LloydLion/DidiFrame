using DidiFrame.UserCommands.ContextValidation.Arguments.Validators;
using DidiFrame.UserCommands.Models;
using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using DidiFrame.Utils.ExtendableModels;
using System;
using System.Collections.Generic;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Arguments.Validators
{
	public class StringCaseTests
	{
		public StringCase CreateInstance(bool onlyUpperNotLower)
		{
			return new StringCase(onlyUpperNotLower);
		}

		[Test]
		public void ValidString()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.String }, typeof(string), "arg", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			const string stringInLowerCase = "string in lower case";
			const string stringInUpperCase = "STRING IN UPPER CASE";

			var commandContextLower = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, stringInLowerCase, new[] { stringInLowerCase }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var commandContextUpper = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, stringInUpperCase, new[] { stringInUpperCase }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var validatorLower = CreateInstance(false);
			var validatorUpper = CreateInstance(true);

			//-------------------

			var workResult1 = validatorLower.Validate(commandContextLower, new(argument, stringInLowerCase, new[] { stringInLowerCase }), EmptyServiceProvider.Instance);
			var workResult2 = validatorUpper.Validate(commandContextUpper, new(argument, stringInUpperCase, new[] { stringInUpperCase }), EmptyServiceProvider.Instance);

			//-------------------

			Assert.That(workResult1, Is.Null);
			Assert.That(workResult2, Is.Null);
		}

		[Test]
		public void InvalidString()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.String }, typeof(string), "arg", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			const string stringInLowerCase = "string in lower case";
			const string stringInUpperCase = "STRING IN UPPER CASE";
			const string stringWithoutCase = "sTrInG iN UPPER cAsE aNd lower cAsE";

			var commandContextLower = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, stringInLowerCase, new[] { stringInLowerCase }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var commandContextUpper = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, stringInUpperCase, new[] { stringInUpperCase }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var commandContextWithout = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, stringWithoutCase, new[] { stringWithoutCase }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var validatorLower = CreateInstance(false);
			var validatorUpper = CreateInstance(true);

			//-------------------

			var workResult1 = validatorLower.Validate(commandContextUpper, new(argument, stringInUpperCase, new[] { stringInUpperCase }), EmptyServiceProvider.Instance);
			var workResult2 = validatorUpper.Validate(commandContextLower, new(argument, stringInLowerCase, new[] { stringInLowerCase }), EmptyServiceProvider.Instance);

			var workResult3 = validatorLower.Validate(commandContextWithout, new(argument, stringWithoutCase, new[] { stringWithoutCase }), EmptyServiceProvider.Instance);
			var workResult4 = validatorUpper.Validate(commandContextWithout, new(argument, stringWithoutCase, new[] { stringWithoutCase }), EmptyServiceProvider.Instance);

			//-------------------

			Assert.That(workResult1, Is.Not.Null);
			Assert.That(workResult1.ErrorCode, Is.EqualTo("OnlyInLowerCase"));

			Assert.That(workResult2, Is.Not.Null);
			Assert.That(workResult2.ErrorCode, Is.EqualTo("OnlyInUpperCase"));

			Assert.That(workResult3, Is.Not.Null);
			Assert.That(workResult3.ErrorCode, Is.EqualTo("OnlyInLowerCase"));

			Assert.That(workResult4, Is.Not.Null);
			Assert.That(workResult4.ErrorCode, Is.EqualTo("OnlyInUpperCase"));
		}
	}
}

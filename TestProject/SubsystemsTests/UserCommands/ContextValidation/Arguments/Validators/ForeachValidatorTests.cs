using DidiFrame.Testing.Utils;
using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.UserCommands.ContextValidation.Arguments;
using DidiFrame.UserCommands.ContextValidation.Arguments.Validators;
using DidiFrame.UserCommands.Models;
using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using DidiFrame.Utils.ExtendableModels;
using System;
using System.Collections.Generic;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Arguments.Validators
{
	public class ForeachValidatorTests
	{
		[Test]
		public void PassMultiValidation()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var invokationKey = new InvokationKey();

			var argument = new UserCommandArgument(true, new[] { UserCommandArgument.Type.Integer }, typeof(int[]), "arg", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, new int[] { 100, 12, 14, 15 }, new object[] { 100, 12, 14, 15 }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var validator = new ForeachValidator(EmptyServiceProvider.Instance, typeof(TargetValidator), new object[] { string.Empty, invokationKey });

			//------------------

			var workResult = validator.Validate(commandContext, commandContext.Arguments[argument], EmptyServiceProvider.Instance);

			//------------------

			Assert.That(workResult, Is.Null);
			Assert.That(invokationKey.SetCount, Is.EqualTo(4));
		}

		[Test]
		public void FailMultiValidation()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var invokationKey = new InvokationKey();

			var argument = new UserCommandArgument(true, new[] { UserCommandArgument.Type.Integer }, typeof(int[]), "arg", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{ { argument, new(argument, new int[] { 100, 12, 14, 15 }, new object[] { 100, 12, 14, 15 }) } }, SimpleModelAdditionalInfoProvider.Empty));

			var validator = new ForeachValidator(EmptyServiceProvider.Instance, typeof(TargetValidator), new object[] { "SomeString", invokationKey });

			//------------------

			var workResult = validator.Validate(commandContext, commandContext.Arguments[argument], EmptyServiceProvider.Instance);

			//------------------

			Assert.That(workResult, Is.Not.Null);
			Assert.That(workResult.ErrorCode, Is.EqualTo("SomeString"));
			Assert.That(invokationKey.SetCount, Is.EqualTo(1));
		}


		private class TargetValidator : IUserCommandArgumentValidator
		{
			private readonly string? targetErrorCode;
			private readonly InvokationKey invokationKey;


			public TargetValidator(string? targetErrorCode, InvokationKey invokationKey)
			{
				this.targetErrorCode = targetErrorCode;
				this.invokationKey = invokationKey;
			}


			public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
			{
				invokationKey.Set();
				if (string.IsNullOrWhiteSpace(targetErrorCode)) return null;
				else return new(targetErrorCode, UserCommandCode.OtherUserError);
			}
		}
	}
}

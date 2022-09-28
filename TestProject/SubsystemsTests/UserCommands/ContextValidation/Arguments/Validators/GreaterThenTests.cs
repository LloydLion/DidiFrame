using DidiFrame.UserCommands.ContextValidation.Arguments.Validators;
using DidiFrame.UserCommands.Models;
using DidiFrame.Utils;
using DidiFrame.Utils.Collections;
using DidiFrame.Utils.ExtendableModels;
using System.Collections.Generic;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation.Arguments.Validators
{
	public class GreaterThenTests
	{

		[Test]
		public void PassSimpleComporation()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var argumentValue = new UserCommandContext.ArgumentValue(argument, 1.0, new object[] { 1.0 });
			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>() { { argument, argumentValue } }, SimpleModelAdditionalInfoProvider.Empty));

			var validator1 = new GreaterThen(0.9, false, false);
			var validator2 = new GreaterThen(1.1, true, false);
			var validator3 = new GreaterThen(1.0, false, true);
			var validator4 = new GreaterThen(1.0, true, true);

			//------------------

			var workResult1 = validator1.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult2 = validator2.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult3 = validator3.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult4 = validator4.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);

			//------------------

			Assert.That(workResult1, Is.Null);
			Assert.That(workResult2, Is.Null);
			Assert.That(workResult3, Is.Null);
			Assert.That(workResult4, Is.Null);
		}

		[Test]
		public void FailSimpleComporation()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var argument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, argument.StoreSingle(), SimpleModelAdditionalInfoProvider.Empty);

			var argumentValue = new UserCommandContext.ArgumentValue(argument, 1.0, new object[] { 1.0 });
			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>() { { argument, argumentValue } }, SimpleModelAdditionalInfoProvider.Empty));

			var validator1 = new GreaterThen(0.9, true, true);
			var validator2 = new GreaterThen(1.1, false, true);
			var validator3 = new GreaterThen(1.0, true, false);
			var validator4 = new GreaterThen(1.0, false, false);

			//------------------

			var workResult1 = validator1.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult2 = validator2.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult3 = validator3.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult4 = validator4.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);

			//------------------

			Assert.That(workResult1, Is.Not.Null);
			Assert.That(workResult1.ErrorCode, Is.EqualTo("TooGreat"));

			Assert.That(workResult2, Is.Not.Null);
			Assert.That(workResult2.ErrorCode, Is.EqualTo("TooSmall"));

			Assert.That(workResult3, Is.Not.Null);
			Assert.That(workResult3.ErrorCode, Is.EqualTo("TooGreat"));

			Assert.That(workResult4, Is.Not.Null);
			Assert.That(workResult4.ErrorCode, Is.EqualTo("TooSmall"));
		}

		[Test]
		public void PassArgumentComporation()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var validatingArgument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg", SimpleModelAdditionalInfoProvider.Empty);
			var sourceArgument1 = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "source1", SimpleModelAdditionalInfoProvider.Empty);
			var sourceArgument2 = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "source2", SimpleModelAdditionalInfoProvider.Empty);
			var sourceArgument3 = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "source3", SimpleModelAdditionalInfoProvider.Empty);
			var sourceArgument4 = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "source4", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, new[]
			{
				validatingArgument, sourceArgument1, sourceArgument2, sourceArgument3, sourceArgument4
			}, SimpleModelAdditionalInfoProvider.Empty);

			var argumentValue = new UserCommandContext.ArgumentValue(validatingArgument, 1.0, new object[] { 1.0 });
			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{
					{ validatingArgument, argumentValue },
					{ sourceArgument1, new(sourceArgument1, 0.9, new object[] { 0.9 }) },
					{ sourceArgument2, new(sourceArgument2, 1.1, new object[] { 1.1 }) },
					{ sourceArgument3, new(sourceArgument3, 1.0, new object[] { 1.0 }) },
					{ sourceArgument4, new(sourceArgument4, 1.0, new object[] { 1.0 }) }
				}, SimpleModelAdditionalInfoProvider.Empty));

			var validator1 = new GreaterThen(sourceArgument1.Name, false, false);
			var validator2 = new GreaterThen(sourceArgument2.Name, true, false);
			var validator3 = new GreaterThen(sourceArgument3.Name, false, true);
			var validator4 = new GreaterThen(sourceArgument4.Name, true, true);

			//------------------

			var workResult1 = validator1.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult2 = validator2.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult3 = validator3.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult4 = validator4.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);

			//------------------

			Assert.That(workResult1, Is.Null);
			Assert.That(workResult2, Is.Null);
			Assert.That(workResult3, Is.Null);
			Assert.That(workResult4, Is.Null);
		}

		[Test]
		public void FailArgumentComporation()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var validatingArgument = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg", SimpleModelAdditionalInfoProvider.Empty);
			var sourceArgument1 = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "source1", SimpleModelAdditionalInfoProvider.Empty);
			var sourceArgument2 = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "source2", SimpleModelAdditionalInfoProvider.Empty);
			var sourceArgument3 = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "source3", SimpleModelAdditionalInfoProvider.Empty);
			var sourceArgument4 = new UserCommandArgument(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "source4", SimpleModelAdditionalInfoProvider.Empty);

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, new[]
			{
				validatingArgument, sourceArgument1, sourceArgument2, sourceArgument3, sourceArgument4
			}, SimpleModelAdditionalInfoProvider.Empty);

			var argumentValue = new UserCommandContext.ArgumentValue(validatingArgument, 1.0, new object[] { 1.0 });
			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{
					{ validatingArgument, argumentValue },
					{ sourceArgument1, new(sourceArgument1, 0.9, new object[] { 0.9 }) },
					{ sourceArgument2, new(sourceArgument2, 1.1, new object[] { 1.1 }) },
					{ sourceArgument3, new(sourceArgument3, 1.0, new object[] { 1.0 }) },
					{ sourceArgument4, new(sourceArgument4, 1.0, new object[] { 1.0 }) }
				}, SimpleModelAdditionalInfoProvider.Empty));

			var validator1 = new GreaterThen(sourceArgument1.Name, true, true);
			var validator2 = new GreaterThen(sourceArgument2.Name, false, true);
			var validator3 = new GreaterThen(sourceArgument3.Name, true, false);
			var validator4 = new GreaterThen(sourceArgument4.Name, false, false);

			//------------------

			var workResult1 = validator1.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult2 = validator2.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult3 = validator3.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);
			var workResult4 = validator4.Validate(commandContext, argumentValue, EmptyServiceProvider.Instance);

			//------------------

			Assert.That(workResult1, Is.Not.Null);
			Assert.That(workResult1.ErrorCode, Is.EqualTo("TooGreat"));

			Assert.That(workResult2, Is.Not.Null);
			Assert.That(workResult2.ErrorCode, Is.EqualTo("TooSmall"));

			Assert.That(workResult3, Is.Not.Null);
			Assert.That(workResult3.ErrorCode, Is.EqualTo("TooGreat"));

			Assert.That(workResult4, Is.Not.Null);
			Assert.That(workResult4.ErrorCode, Is.EqualTo("TooSmall"));
		}
	}
}

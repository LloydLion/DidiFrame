using DidiFrame.Testing.Localization;
using DidiFrame.Testing.Utils;
using DidiFrame.UserCommands.ContextValidation;
using DidiFrame.UserCommands.ContextValidation.Arguments;
using DidiFrame.UserCommands.ContextValidation.Arguments.Providers;
using DidiFrame.UserCommands.ContextValidation.Invoker;
using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.Utils.Collections;
using DidiFrame.Utils.ExtendableModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestProject.SubsystemsTests.UserCommands.ContextValidation
{
	public class ContextValidatorTests
	{
		public IUserCommandPipelineMiddleware<UserCommandContext, ValidatedUserCommandContext> CreateValidator()
		{
			return new ContextValidator(new TestLocalizer<ContextValidator>());
		}

		[Test]
		public async Task DirectPass()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var otherMember = server.AddMember("Other", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));


			var arguments = new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "arg1", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "arg2", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg3", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.DateTime, UserCommandArgument.Type.String }, typeof(decimal), "arg4", SimpleModelAdditionalInfoProvider.Empty)
			};

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, arguments, SimpleModelAdditionalInfoProvider.Empty);


			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{
					{ arguments[0], new(arguments[0], 100, new object[] { 100 }) },
					{ arguments[1], new(arguments[1], otherMember, new object[] { otherMember }) },
					{ arguments[2], new(arguments[2], 100.11, new object[] { 100.11 }) },
					{ arguments[3], new(arguments[3], 100m, new object[] { new DateTime(10000), "100" }) }
				}, SimpleModelAdditionalInfoProvider.Empty));

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);


			var validator = CreateValidator();

			//-------------------

			var workResult = await validator.ProcessAsync(commandContext, pipelineContext);

			//-------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));
			Assert.That(workResult.GetOutput(), Is.EqualTo(new ValidatedUserCommandContext(commandContext)));
		}

		[Test]
		public async Task PassWithFiltersAndValidators()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var otherMember = server.AddMember("Other", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));


			var argumentsInvokationKey = new InvokationKey();
			var commandInvokationKey = new InvokationKey();


			var validatorForArguments = new StupValidator(argumentsInvokationKey);
			var providerForArguments = new SimpleModelAdditionalInfoProvider((validatorForArguments.StoreSingle(), typeof(IReadOnlyCollection<IUserCommandArgumentValidator>)));
			var arguments = new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "arg1", providerForArguments),
				new(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "arg2", providerForArguments),
				new(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg3", providerForArguments),
				new(false, new[] { UserCommandArgument.Type.DateTime, UserCommandArgument.Type.String }, typeof(decimal), "arg4", providerForArguments)
			};

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, arguments,
				new SimpleModelAdditionalInfoProvider((new StupFilter(commandInvokationKey).StoreSingle(), typeof(IReadOnlyCollection<IUserCommandInvokerFilter>))));


			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{
					{ arguments[0], new(arguments[0], 100, new object[] { 100 }) },
					{ arguments[1], new(arguments[1], otherMember, new object[] { otherMember }) },
					{ arguments[2], new(arguments[2], 100.11, new object[] { 100.11 }) },
					{ arguments[3], new(arguments[3], 100m, new object[] { new DateTime(10000), "100" }) }
				}, SimpleModelAdditionalInfoProvider.Empty));

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);


			var validator = CreateValidator();

			//-------------------

			var workResult = await validator.ProcessAsync(commandContext, pipelineContext);

			//-------------------

			Assert.That(argumentsInvokationKey.SetCount, Is.EqualTo(4));
			Assert.That(commandInvokationKey.SetCount, Is.EqualTo(1));
			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));
			Assert.That(workResult.GetOutput(), Is.EqualTo(new ValidatedUserCommandContext(commandContext)));
		}

		[Test]
		public async Task FailWithFilters()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var otherMember = server.AddMember("Other", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));


			var commandInvokationKey = new InvokationKey();


			var arguments = new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "arg1", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "arg2", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg3", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.DateTime, UserCommandArgument.Type.String }, typeof(decimal), "arg4", SimpleModelAdditionalInfoProvider.Empty)
			};

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, arguments,
				new SimpleModelAdditionalInfoProvider((new StupFilter(commandInvokationKey, "SomeKey").StoreSingle(), typeof(IReadOnlyCollection<IUserCommandInvokerFilter>))));


			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{
					{ arguments[0], new(arguments[0], 100, new object[] { 100 }) },
					{ arguments[1], new(arguments[1], otherMember, new object[] { otherMember }) },
					{ arguments[2], new(arguments[2], 100.11, new object[] { 100.11 }) },
					{ arguments[3], new(arguments[3], 100m, new object[] { new DateTime(10000), "100" }) }
				}, SimpleModelAdditionalInfoProvider.Empty));

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);


			var validator = CreateValidator();

			//-------------------

			var workResult = await validator.ProcessAsync(commandContext, pipelineContext);

			//-------------------

			Assert.That(commandInvokationKey.SetCount, Is.EqualTo(1));
			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Finalization));
			Assert.That(workResult.GetFinalizationResult().ResultType, Is.EqualTo(UserCommandResult.Type.Message));
		}

		[Test]
		public async Task FailWithValidators()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var otherMember = server.AddMember("Other", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));


			var argumentsInvokationKey = new InvokationKey();


			var validatorForOneArgument = new StupValidator(argumentsInvokationKey, "SomeKey");
			var providerForOneArgument = new SimpleModelAdditionalInfoProvider((validatorForOneArgument.StoreSingle(), typeof(IReadOnlyCollection<IUserCommandArgumentValidator>)));
			var arguments = new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "arg1", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "arg2", providerForOneArgument),
				new(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg3", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.DateTime, UserCommandArgument.Type.String }, typeof(decimal), "arg4", SimpleModelAdditionalInfoProvider.Empty)
			};

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, arguments, SimpleModelAdditionalInfoProvider.Empty);


			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{
					{ arguments[0], new(arguments[0], 100, new object[] { 100 }) },
					{ arguments[1], new(arguments[1], otherMember, new object[] { otherMember }) },
					{ arguments[2], new(arguments[2], 100.11, new object[] { 100.11 }) },
					{ arguments[3], new(arguments[3], 100m, new object[] { new DateTime(10000), "100" }) }
				}, SimpleModelAdditionalInfoProvider.Empty));

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);


			var validator = CreateValidator();

			//-------------------

			var workResult = await validator.ProcessAsync(commandContext, pipelineContext);

			//-------------------

			Assert.That(argumentsInvokationKey.SetCount, Is.EqualTo(1));
			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Finalization));
			Assert.That(workResult.GetFinalizationResult().ResultType, Is.EqualTo(UserCommandResult.Type.Message));
		}

		[Test]
		public async Task FailWithFiltersAndValidators()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var otherMember = server.AddMember("Other", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));


			var argumentsInvokationKey = new InvokationKey();
			var commandInvokationKey = new InvokationKey();


			var validatorForArguments = new StupValidator(argumentsInvokationKey, "SomeKey");
			var providerForArguments = new SimpleModelAdditionalInfoProvider((validatorForArguments.StoreSingle(), typeof(IReadOnlyCollection<IUserCommandArgumentValidator>)));
			var arguments = new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "arg1", providerForArguments),
				new(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "arg2", providerForArguments),
				new(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg3", providerForArguments),
				new(false, new[] { UserCommandArgument.Type.DateTime, UserCommandArgument.Type.String }, typeof(decimal), "arg4", providerForArguments)
			};

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, arguments,
				new SimpleModelAdditionalInfoProvider((new StupFilter(commandInvokationKey, "SomeKey").StoreSingle(), typeof(IReadOnlyCollection<IUserCommandInvokerFilter>))));


			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{
					{ arguments[0], new(arguments[0], 100, new object[] { 100 }) },
					{ arguments[1], new(arguments[1], otherMember, new object[] { otherMember }) },
					{ arguments[2], new(arguments[2], 100.11, new object[] { 100.11 }) },
					{ arguments[3], new(arguments[3], 100m, new object[] { new DateTime(10000), "100" }) }
				}, SimpleModelAdditionalInfoProvider.Empty));

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);


			var validator = CreateValidator();

			//-------------------

			var workResult = await validator.ProcessAsync(commandContext, pipelineContext);

			//-------------------

			Assert.That(argumentsInvokationKey.SetCount, Is.EqualTo(0));
			Assert.That(commandInvokationKey.SetCount, Is.EqualTo(1));
			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Finalization));
			Assert.That(workResult.GetFinalizationResult().ResultType, Is.EqualTo(UserCommandResult.Type.Message));
		}

		[Test]
		public async Task PassWithProvider()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var otherMember = server.AddMember("Other", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));


			var valuesProviderForIntArgument = new FixedProvider<int>(0, 1, 2, 3, 4, 5, 6, 7);
			var providerForIntArgument = new SimpleModelAdditionalInfoProvider((valuesProviderForIntArgument.StoreSingle(), typeof(IReadOnlyCollection<IUserCommandArgumentValuesProvider>)));
			var arguments = new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "arg1", providerForIntArgument),
				new(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "arg2", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg3", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.DateTime, UserCommandArgument.Type.String }, typeof(decimal), "arg4", SimpleModelAdditionalInfoProvider.Empty)
			};

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, arguments, SimpleModelAdditionalInfoProvider.Empty);


			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{
					{ arguments[0], new(arguments[0], 3, new object[] { 3 }) },
					{ arguments[1], new(arguments[1], otherMember, new object[] { otherMember }) },
					{ arguments[2], new(arguments[2], 100.11, new object[] { 100.11 }) },
					{ arguments[3], new(arguments[3], 100m, new object[] { new DateTime(10000), "100" }) }
				}, SimpleModelAdditionalInfoProvider.Empty));

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);


			var validator = CreateValidator();

			//-------------------

			var workResult = await validator.ProcessAsync(commandContext, pipelineContext);

			//-------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));
			Assert.That(workResult.GetOutput(), Is.EqualTo(new ValidatedUserCommandContext(commandContext)));
		}

		[Test]
		public async Task FailWithProvider()
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var otherMember = server.AddMember("Other", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));


			var valuesProviderForIntArgument = new FixedProvider<int>(0, 1, 2, 3, 4, 5, 6, 7);
			var providerForIntArgument = new SimpleModelAdditionalInfoProvider((valuesProviderForIntArgument.StoreSingle(), typeof(IReadOnlyCollection<IUserCommandArgumentValuesProvider>)));
			var arguments = new UserCommandArgument[]
			{
				new(false, new[] { UserCommandArgument.Type.Integer }, typeof(int), "arg1", providerForIntArgument),
				new(false, new[] { UserCommandArgument.Type.Member }, typeof(IMember), "arg2", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.Double }, typeof(double), "arg3", SimpleModelAdditionalInfoProvider.Empty),
				new(false, new[] { UserCommandArgument.Type.DateTime, UserCommandArgument.Type.String }, typeof(decimal), "arg4", SimpleModelAdditionalInfoProvider.Empty)
			};

			var command = new UserCommandInfo("thecmd", NoHandler.Handler, arguments, SimpleModelAdditionalInfoProvider.Empty);


			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>()
				{
					{ arguments[0], new(arguments[0], 100, new object[] { 30 }) },
					{ arguments[1], new(arguments[1], otherMember, new object[] { otherMember }) },
					{ arguments[2], new(arguments[2], 100.11, new object[] { 100.11 }) },
					{ arguments[3], new(arguments[3], 100m, new object[] { new DateTime(10000), "100" }) }
				}, SimpleModelAdditionalInfoProvider.Empty));

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);


			var validator = CreateValidator();

			//-------------------

			var workResult = await validator.ProcessAsync(commandContext, pipelineContext);

			//-------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Finalization));
			Assert.That(workResult.GetFinalizationResult().ResultType, Is.EqualTo(UserCommandResult.Type.Message));
		}

		private static Task SendResponceDropHandler(UserCommandResult _)
		{
			Assert.Fail("Converter tries send responce");
			return Task.CompletedTask;
		}


		private class StupFilter : IUserCommandInvokerFilter
		{
			private readonly InvokationKey key;
			private readonly string? errorKeyIfNeedBaseEverything;


			public StupFilter(InvokationKey key, string? errorKeyIfNeedBaseEverything = null)
			{
				this.key = key;
				this.errorKeyIfNeedBaseEverything = errorKeyIfNeedBaseEverything;
			}


			public ValidationFailResult? Filter(UserCommandContext ctx, IServiceProvider localServices)
			{
				key.Set();
				if (errorKeyIfNeedBaseEverything is null) return null;
				else return new(errorKeyIfNeedBaseEverything, UserCommandCode.OtherUserError);
			}
		}

		private class StupValidator : IUserCommandArgumentValidator
		{
			private readonly InvokationKey key;
			private readonly string? errorKeyIfNeedBaseEverything;


			public StupValidator(InvokationKey key, string? errorKeyIfNeedBaseEverything = null)
			{
				this.key = key;
				this.errorKeyIfNeedBaseEverything = errorKeyIfNeedBaseEverything;
			}


			public ValidationFailResult? Validate(UserCommandContext context, UserCommandContext.ArgumentValue value, IServiceProvider localServices)
			{
				key.Set();
				if (errorKeyIfNeedBaseEverything is null) return null;
				else return new(errorKeyIfNeedBaseEverything, UserCommandCode.OtherUserError);
			}
		}
	}
}

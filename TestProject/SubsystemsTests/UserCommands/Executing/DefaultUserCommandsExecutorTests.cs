using DidiFrame.UserCommands.Executing;
using DidiFrame.Testing.Logging;
using Microsoft.Extensions.Options;
using DidiFrame.UserCommands.Models;
using DidiFrame.Utils.ExtendableModels;
using System.Threading.Tasks;
using System;
using DidiFrame.UserCommands.Pipeline;
using System.Collections.Generic;
using DidiFrame.Entities.Message;

namespace TestProject.SubsystemsTests.UserCommands.Executing
{
	public class DefaultUserCommandsExecutorTests
	{
		public IUserCommandPipelineMiddleware<ValidatedUserCommandContext, UserCommandResult> CreateExecutor(bool sendMessageIfUnspecifiedError)
		{
			var options = Options.Create<DefaultUserCommandsExecutor.Options>(
				new()
				{
					UnspecifiedErrorMessage = sendMessageIfUnspecifiedError ?
						DefaultUserCommandsExecutor.Options.UnspecifiedErrorMessageBehavior.EnableWithoutDebugInfo :
						DefaultUserCommandsExecutor.Options.UnspecifiedErrorMessageBehavior.Disable
				}
			);

			return new DefaultUserCommandsExecutor(options, new DebugConsoleLogger<DefaultUserCommandsExecutor>());
		}

		[Test]
		public async Task SendEmptyHandler()
		{
			var (executor, pipelineContext, commandContext) = CreateEnviroment(handler);

			//------------------

			var workResult = await executor.ProcessAsync(commandContext, pipelineContext);

			//------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));
			Assert.That(workResult.GetOutput().ResultType, Is.EqualTo(UserCommandResult.Type.None));
			Assert.That(workResult.GetOutput().Code, Is.EqualTo(UserCommandCode.Sucssesful));



			static Task<UserCommandResult> handler(UserCommandContext ctx)
			{
				return Task.FromResult(UserCommandResult.CreateEmpty(UserCommandCode.Sucssesful));
			}
		}

		[Test]
		public async Task SendHandlerWithMessage()
		{
			var (executor, pipelineContext, commandContext) = CreateEnviroment(handler);

			//------------------

			var workResult = await executor.ProcessAsync(commandContext, pipelineContext);

			//------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));
			Assert.That(workResult.GetOutput().ResultType, Is.EqualTo(UserCommandResult.Type.Message));
			Assert.That(workResult.GetOutput().Code, Is.EqualTo(UserCommandCode.Sucssesful));
			Assert.That(workResult.GetOutput().GetRespondMessage(), Is.EqualTo(new MessageSendModel("STR")));



			static Task<UserCommandResult> handler(UserCommandContext ctx)
			{
				return Task.FromResult(UserCommandResult.CreateWithMessage(UserCommandCode.Sucssesful, new("STR")));
			}
		}

		[Test]
		public async Task SendEmptyHandlerWithNonSucssesfulResult()
		{
			var (executor, pipelineContext, commandContext) = CreateEnviroment(handler);

			//------------------

			var workResult = await executor.ProcessAsync(commandContext, pipelineContext);

			//------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));
			Assert.That(workResult.GetOutput().ResultType, Is.EqualTo(UserCommandResult.Type.None));
			Assert.That(workResult.GetOutput().Code, Is.EqualTo(UserCommandCode.OtherUserError));



			static Task<UserCommandResult> handler(UserCommandContext ctx)
			{
				return Task.FromResult(UserCommandResult.CreateEmpty(UserCommandCode.OtherUserError));
			}
		}

		[Test]
		public async Task SendHandlerWithNonSucssesfulResultAndMessage()
		{
			var (executor, pipelineContext, commandContext) = CreateEnviroment(handler);

			//------------------

			var workResult = await executor.ProcessAsync(commandContext, pipelineContext);

			//------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));
			Assert.That(workResult.GetOutput().ResultType, Is.EqualTo(UserCommandResult.Type.Message));
			Assert.That(workResult.GetOutput().Code, Is.EqualTo(UserCommandCode.OtherUserError));
			Assert.That(workResult.GetOutput().GetRespondMessage(), Is.EqualTo(new MessageSendModel("STR")));



			static Task<UserCommandResult> handler(UserCommandContext ctx)
			{
				return Task.FromResult(UserCommandResult.CreateWithMessage(UserCommandCode.OtherUserError, new("STR")));
			}
		}

		[Test]
		public async Task SendHandlerWithException()
		{
			var (executor, pipelineContext, commandContext) = CreateEnviroment(handler, sendMessageIfUnspecifiedError: true);

			//------------------

			var workResult = await executor.ProcessAsync(commandContext, pipelineContext);

			//------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));
			Assert.That(workResult.GetOutput().ResultType, Is.EqualTo(UserCommandResult.Type.Message));
			Assert.That(workResult.GetOutput().Code, Is.EqualTo(UserCommandCode.UnspecifiedError));



			static Task<UserCommandResult> handler(UserCommandContext ctx)
			{
				throw new Exception("Some exception in user commadn handler");
			}
		}

		[Test]
		public async Task SendHandlerWithExceptionCaseWithOtherOptions()
		{
			var (executor, pipelineContext, commandContext) = CreateEnviroment(handler, sendMessageIfUnspecifiedError: false);

			//------------------

			var workResult = await executor.ProcessAsync(commandContext, pipelineContext);

			//------------------

			Assert.That(workResult.ResultType, Is.EqualTo(UserCommandMiddlewareExcutionResult.Type.Output));
			Assert.That(workResult.GetOutput().ResultType, Is.EqualTo(UserCommandResult.Type.None));
			Assert.That(workResult.GetOutput().Code, Is.EqualTo(UserCommandCode.UnspecifiedError));



			static Task<UserCommandResult> handler(UserCommandContext ctx)
			{
				throw new Exception("Some exception in user commadn handler");
			}
		}

		private static Task SendResponceDropHandler(UserCommandResult _)
		{
			Assert.Fail("Converter tries send responce");
			return Task.CompletedTask;
		}

		private (IUserCommandPipelineMiddleware<ValidatedUserCommandContext, UserCommandResult>, UserCommandPipelineContext, ValidatedUserCommandContext) CreateEnviroment
			(UserCommandHandler userCommandHandler, bool sendMessageIfUnspecifiedError = false)
		{
			var client = new Client();
			var server = client.CreateServer();
			var member = server.AddMember("Who?", false, Permissions.All);
			var textChannel = (TextChannelBase)server.AddChannel(new("where", ChannelType.TextCompatible));

			var command = new UserCommandInfo("thecmd", userCommandHandler, Array.Empty<UserCommandArgument>(), SimpleModelAdditionalInfoProvider.Empty);

			var commandContext = new ValidatedUserCommandContext(new(new UserCommandSendData(member, textChannel), command,
				new Dictionary<UserCommandArgument, UserCommandContext.ArgumentValue>(), SimpleModelAdditionalInfoProvider.Empty));

			var pipelineContext = new UserCommandPipelineContext(new UserCommandSendData(member, textChannel), SendResponceDropHandler);

			var executor = CreateExecutor(sendMessageIfUnspecifiedError);

			return (executor, pipelineContext, commandContext);
		}
	}
}

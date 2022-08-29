using DidiFrame.Culture;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Simple implementation for DidiFrame.UserCommands.Pipeline.IUserCommandPipelineExecutor
	/// </summary>
	public class UserCommandPipelineExecutor : IUserCommandPipelineExecutor
	{
		private static readonly EventId PipelineStartID = new(91, "PipelineStart");
		private static readonly EventId PipelineDroppedID = new(92, "PipelineDropped");
		private static readonly EventId PipelineFinalizedID = new(93, "PipelineFinalized");
		private static readonly EventId PipelineInforationID = new(94, "PipelineInforation");
		private static readonly EventId PipelineExceptionID = new(95, "PipelineException");


		private readonly ILogger<UserCommandPipelineExecutor> logger;
		private readonly IServiceScopeFactory? scopeFactory;
		private readonly IServerCultureProvider? cultureProvider;
		private readonly IValidator<UserCommandSendData>? sendDataValidator;
		private readonly IValidator<UserCommandResult>? resultValidator;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.UserCommandPipelineExecutor with local services
		/// </summary>
		/// <param name="logger">Logger for executor</param>
		/// <param name="sendDataValidator">Validator for DidiFrame.UserCommands.Models.UserCommandSendData</param>
		/// <param name="resultValidator">Validator for DidiFrame.UserCommands.Models.UserCommandResult</param>
		public UserCommandPipelineExecutor(
			ILogger<UserCommandPipelineExecutor> logger,
			IServerCultureProvider? cultureProvider = null,
			IServiceScopeFactory? scopeFactory = null,
			IValidator<UserCommandSendData>? sendDataValidator = null,
			IValidator<UserCommandResult>? resultValidator = null)
		{
			this.scopeFactory = scopeFactory;
			this.logger = logger;
			this.cultureProvider = cultureProvider;
			this.sendDataValidator = sendDataValidator;
			this.resultValidator = resultValidator;
		}


		/// <inheritdoc/>
		public async Task<UserCommandResult?> ProcessAsync(UserCommandPipeline pipeline, object input, UserCommandSendData sendData, object dispatcherState)
		{
			sendDataValidator?.ValidateAndThrow(sendData);

			cultureProvider?.SetupCulture(sendData.Channel.Server);

			var guid = Guid.NewGuid();

			using (var scope = scopeFactory?.CreateScope())
			{
				try
				{
					logger.Log(LogLevel.Information, PipelineStartID, "({PipelineExecutionId}) Command pipeline executing started in {ServerId} #{ChannelName} by {MemberName}",
						guid, sendData.Channel.Server.Id, sendData.Channel.Name, sendData.Invoker.UserName);

					var result = await wrap(input, pipeline, sendData, dispatcherState, scope);

					resultValidator?.ValidateAndThrow(result);

					logger.Log(LogLevel.Debug, PipelineFinalizedID, "({PipelineExecutionId}) Command pipeline finalized successfully", guid);

					if (result.DebugMessage is not null)
						logger.Log(LogLevel.Information, PipelineInforationID, result.Exception, "({PipelineExecutionId}) Executed pipeline informs: {Message}", guid, result.DebugMessage);

					if (result.Exception is not null)
						logger.Log(LogLevel.Error, PipelineExceptionID, result.Exception, "({PipelineExecutionId}) Executed pipeline finished with exception", guid);

					return result;
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Debug, PipelineDroppedID, ex, "({PipelineExecutionId}) Command pipeline dropped with error", guid);
					return null;
				}
			}


			static async ValueTask<UserCommandResult> wrap(object input, UserCommandPipeline pipeline, UserCommandSendData sendData, object dispatcherState, IServiceScope? scope)
			{
				object currentValue = input;

				foreach (var middleware in pipeline.Middlewares)
				{
					var context = new UserCommandPipelineContext(scope?.ServiceProvider ?? new EmptyServicesProvider(), sendData, (result) => pipeline.Origin.RespondAsync(dispatcherState, result));
					var result = await middleware.ProcessAsync(currentValue, context);

					if (result.ResultType == UserCommandMiddlewareExcutionResult.Type.Finalization) return result.GetFinalizationResult();
					else currentValue = result.GetOutput();
				}

				return (UserCommandResult)currentValue;
			}
		}


		private sealed class EmptyServicesProvider : IServiceProvider
		{
			public object? GetService(Type serviceType) => null;
		}
	}
}

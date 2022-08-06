using FluentValidation;

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


		private readonly IReadOnlyCollection<IUserCommandLocalServiceDescriptor> descriptors;
		private readonly IServiceProvider serviceProvider;
		private readonly ILogger<UserCommandPipelineExecutor> logger;
		private readonly IValidator<UserCommandSendData> sendDataValidator;
		private readonly IValidator<UserCommandResult> resultValidator;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.UserCommandPipelineExecutor with local services
		/// </summary>
		/// <param name="descriptors">Enumerable of local services' descriptors</param>
		/// <param name="serviceProvider">Global service provider</param>
		/// <param name="logger">Logger for executor</param>
		/// <param name="sendDataValidator">Validator for DidiFrame.UserCommands.Models.UserCommandSendData</param>
		/// <param name="resultValidator">Validator for DidiFrame.UserCommands.Models.UserCommandResult</param>
		public UserCommandPipelineExecutor(IEnumerable<IUserCommandLocalServiceDescriptor> descriptors,
			IServiceProvider serviceProvider,
			ILogger<UserCommandPipelineExecutor> logger,
			IValidator<UserCommandSendData> sendDataValidator,
			IValidator<UserCommandResult> resultValidator)
		{
			this.descriptors = descriptors.ToArray();
			this.serviceProvider = serviceProvider;
			this.logger = logger;
			this.sendDataValidator = sendDataValidator;
			this.resultValidator = resultValidator;
		}


		/// <inheritdoc/>
		public async Task<UserCommandResult?> ProcessAsync(UserCommandPipeline pipeline, object input, UserCommandSendData sendData, object dispatcherState)
		{
			sendDataValidator.ValidateAndThrow(sendData);


			var guid = Guid.NewGuid();

			logger.Log(LogLevel.Information, PipelineStartID, "({PipelineExecutionId}) Command pipeline executing started in {ServerId} #{ChannelName} by {MemberName}",
				guid, sendData.Channel.Server.Id, sendData.Channel.Name, sendData.Invoker.UserName);

			using var services = new UserCommandLocalServicesProvider(descriptors.Select(s => s.CreateInstance(serviceProvider)).ToArray());

			var result = await wrap(input, pipeline, serviceProvider, sendData, dispatcherState);

			if (result is null)
				logger.Log(LogLevel.Debug, PipelineDroppedID, "({PipelineExecutionId}) Command pipeline dropped", guid);
			else
			{
				resultValidator.ValidateAndThrow(result);

				logger.Log(LogLevel.Debug, PipelineFinalizedID, "({PipelineExecutionId}) Command pipeline finalized successfully", guid);

				if (result.DebugMessage is not null)
					logger.Log(LogLevel.Information, PipelineInforationID, result.Exception, "({PipelineExecutionId}) Executed pipeline informs: {Message}", guid, result.DebugMessage);

				if (result.Exception is not null)
					logger.Log(LogLevel.Error, PipelineExceptionID, result.Exception, "({PipelineExecutionId}) Executed pipeline finished with exception", guid);
			}

			return result;


			static async ValueTask<UserCommandResult?> wrap(object input, UserCommandPipeline pipeline, IServiceProvider services, UserCommandSendData sendData, object dispatcherState)
			{
				object currentValue = input;

				foreach (var middleware in pipeline.Middlewares)
				{
					var context = new UserCommandPipelineContext(services, sendData, (result) => pipeline.Origin.RespondAsync(dispatcherState, result));

					try
					{
						var result = await middleware.ProcessAsync(currentValue, context);

						if (result.ResultType == UserCommandMiddlewareExcutionResult.Type.Finalization) return result.GetFinalizationResult();
						else currentValue = result.GetOutput();
					}
					catch (Exception)
					{
						return null;
					}
				}

				return (UserCommandResult)currentValue;
			}
		}
	}
}

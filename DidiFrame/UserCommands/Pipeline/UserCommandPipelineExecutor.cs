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
		private readonly IServiceProvider sp;
		private readonly ILogger<UserCommandPipelineExecutor> logger;
		private readonly IValidator<UserCommandSendData> sendDataValidator;
		private readonly IValidator<UserCommandResult> resultValidator;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.UserCommandPipelineExecutor with local services
		/// </summary>
		/// <param name="descriptors">Enumerable of local services' descriptors</param>
		/// <param name="sp">Global service provider</param>
		public UserCommandPipelineExecutor(IEnumerable<IUserCommandLocalServiceDescriptor> descriptors,
			IServiceProvider sp,
			ILogger<UserCommandPipelineExecutor> logger,
			IValidator<UserCommandSendData> sendDataValidator,
			IValidator<UserCommandResult> resultValidator)
		{
			this.descriptors = descriptors.ToArray();
			this.sp = sp;
			this.logger = logger;
			this.sendDataValidator = sendDataValidator;
			this.resultValidator = resultValidator;
		}


		/// <inheritdoc/>
		public Task<UserCommandResult?> ProcessAsync(UserCommandPipeline pipeline, object input, UserCommandSendData sendData, object dispatcherState)
		{
			sendDataValidator.ValidateAndThrow(sendData);

			return Task.Run(() =>
			{
				var guid = Guid.NewGuid();

				logger.Log(LogLevel.Information, "({Id}) Command pipeline executing started in {ServerId} #{ChannelName} by {MemberName}", guid, sendData.Channel.Server.Id, sendData.Channel.Name, sendData.Invoker.UserName);

				using var services = new UserCommandLocalServicesProvider(descriptors.Select(s => s.CreateInstance(sp)).ToArray());

				var result = wrap(input, pipeline, sp, sendData, dispatcherState);

				if (result is null)
					logger.Log(LogLevel.Debug, "({Id}) Command pipeline dropped", guid);
				else
				{
					resultValidator.ValidateAndThrow(result);

					logger.Log(LogLevel.Debug, "({Id}) Command pipeline finalized successfully", guid);

					if (result.DebugMessage is not null)
						logger.Log(LogLevel.Information, result.Exception, "({Id}) Executed pipeline informs: {Message}", guid, result.DebugMessage);

					if (result.Exception is not null)
						logger.Log(LogLevel.Error, result.Exception, "({Id}) Executed pipeline finished with exception", guid);
				}

				return result;


				static UserCommandResult? wrap(object input, UserCommandPipeline pipeline, IServiceProvider services, UserCommandSendData sendData, object dispatcherState)
				{
					object currentValue = input;

					foreach (var middleware in pipeline.Middlewares)
					{
						var context = new UserCommandPipelineContext(services, sendData, (result) => pipeline.Origin.Respond(dispatcherState, result));

						var newValue = middleware.Process(currentValue, context);

						if (newValue is null)
						{
							if (context.CurrentStatus == UserCommandPipelineContext.Status.BeginDrop) return null;
							else if (context.CurrentStatus == UserCommandPipelineContext.Status.BeginFinalize) return context.GetExecutionResult();
							else throw new InvalidOperationException("Enable to return null and don't set any status in context");
						}
						else currentValue = newValue;
					}

					return (UserCommandResult)currentValue;
				}
			});
		}
	}
}

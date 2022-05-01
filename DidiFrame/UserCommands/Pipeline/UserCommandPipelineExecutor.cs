namespace DidiFrame.UserCommands.Pipeline
{
	public class UserCommandPipelineExecutor : IUserCommandPipelineExecutor
	{
		private readonly IReadOnlyList<IUserCommandPipelineMiddleware> pipeline;


		public UserCommandPipelineExecutor(UserCommandPipeline pipeline)
		{
			this.pipeline = pipeline.Middlewares;
		}


		public UserCommandResult? Process<TInput>(TInput input, UserCommandSendData sendData) where TInput : notnull
		{
			object currentValue = input;

			foreach (var middleware in pipeline)
			{
				var context = new UserCommandPipelineContext(sendData);

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
	}
}

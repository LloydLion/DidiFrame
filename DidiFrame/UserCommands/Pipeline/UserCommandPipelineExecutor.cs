namespace DidiFrame.UserCommands.Pipeline
{
	public class UserCommandPipelineExecutor : IUserCommandPipelineExecutor
	{
		public UserCommandResult? Process(UserCommandPipeline pipeline, object input, UserCommandSendData sendData)
		{
			object currentValue = input;

			foreach (var middleware in pipeline.Middlewares)
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

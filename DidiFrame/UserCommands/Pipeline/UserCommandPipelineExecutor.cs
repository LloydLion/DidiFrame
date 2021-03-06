namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Simple implementation for DidiFrame.UserCommands.Pipeline.IUserCommandPipelineExecutor
	/// </summary>
	public class UserCommandPipelineExecutor : IUserCommandPipelineExecutor
	{
		private readonly IReadOnlyCollection<IUserCommandLocalServiceDescriptor> descriptors;
		private readonly IServiceProvider sp;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.UserCommandPipelineExecutor with local services
		/// </summary>
		/// <param name="descriptors">Enumerable of local services' descriptors</param>
		/// <param name="sp">Global service provider</param>
		public UserCommandPipelineExecutor(IEnumerable<IUserCommandLocalServiceDescriptor> descriptors, IServiceProvider sp)
		{
			this.descriptors = descriptors.ToArray();
			this.sp = sp;
		}



		/// <inheritdoc/>
		public Task<UserCommandResult?> ProcessAsync(UserCommandPipeline pipeline, object input, UserCommandSendData sendData, object dispatcherState)
		{
			return Task.Run(() =>
			{
				using var services = new UserCommandLocalServicesProvider(descriptors.Select(s => s.CreateInstance(sp)).ToArray());

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
			});
		}
	}
}

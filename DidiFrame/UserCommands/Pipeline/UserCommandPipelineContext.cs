using DidiFrame.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Pipeline
{
	/// <summary>
	/// Context for item that executing in user command pipeline
	/// </summary>
	public class UserCommandPipelineContext
	{
		private readonly Func<UserCommandResult, Task> sendResponce;


		/// <summary>
		/// Send data from dispatcher
		/// </summary>
		public UserCommandSendData SendData { get; }

		/// <summary>
		/// Provided local services
		/// </summary>
		public IServiceProvider LocalServices { get; }


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.UserCommandPipelineContext
		/// </summary>
		/// <param name="localServices">Local services</param>
		/// <param name="sendData">Send data from dispatcher</param>
		/// <param name="sendResponce">Delegate that sends responce to dispatcher</param>
		public UserCommandPipelineContext(IServiceProvider localServices, UserCommandSendData sendData, Func<UserCommandResult, Task> sendResponce)
		{
			LocalServices = localServices;
			SendData = sendData;
			this.sendResponce = sendResponce;
		}

		public UserCommandPipelineContext(UserCommandSendData sendData, Func<UserCommandResult, Task> sendResponce) : this(EmptyServiceProvider.Instance, sendData, sendResponce) { }

		/// <summary>
		/// Send responce to dispatcher
		/// </summary>
		/// <param name="result">Responce itself</param>
		public Task SendResponceAsync(UserCommandResult result) => sendResponce(result);
	}
}

using DidiFrame.UserCommands.Models;
using DidiFrame.UserCommands.Pipeline;
using System;

namespace TestProject.Environment.UserCommands
{
	internal class NullDispatcher<T> : IUserCommandPipelineDispatcher<T> where T : notnull
	{
		public void SetSyncCallback(Action<T, UserCommandSendData, Action<UserCommandResult>> actionWithCallback)
		{
			
		}
	}
}

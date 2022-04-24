using DidiFrame.Exceptions;
using DidiFrame.UserCommands;

namespace TestProject.Environment.Client
{
	internal class CommandsDispatcher : ICommandsDispatcher
	{
		public event CommandWrittenHandler? CommandWritten;


		public UserCommandResult SendCommand(UserCommandContext context)
		{
			UserCommandResult? result = null;
			CommandWritten?.Invoke(context, (res) => result = res);

			return result ?? throw new ImpossibleVariantException();
		}
	}
}

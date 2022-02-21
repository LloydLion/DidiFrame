using CGZBot3.Exceptions;
using CGZBot3.UserCommands;

namespace TestProject.TestAdapter
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

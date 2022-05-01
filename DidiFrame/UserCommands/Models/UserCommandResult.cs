using DidiFrame.Entities.Message;

namespace DidiFrame.UserCommands.Models
{
	public class UserCommandResult
	{
		public UserCommandResult(UserCommandCode code)
		{
			Code = code;
		}


		public MessageSendModel? RespondMessage { get; init; }

		public UserCommandCode Code { get; }
	}
}

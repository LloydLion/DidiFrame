using CGZBot3.Entities.Message;

namespace CGZBot3.UserCommands
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

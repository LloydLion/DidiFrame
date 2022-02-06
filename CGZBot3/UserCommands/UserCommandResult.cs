namespace CGZBot3.UserCommands
{
	internal class UserCommandResult
	{
		public UserCommandResult(UserCommandCode code)
		{
			Code = code;
		}


		public MessageSendModel? RespondMessage { get; init; }

		public UserCommandCode Code { get; }
	}
}

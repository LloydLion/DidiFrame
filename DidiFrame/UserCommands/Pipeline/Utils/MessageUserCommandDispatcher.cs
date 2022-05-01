using DidiFrame.UserCommands.Pipeline;

namespace DidiFrame.UserCommands.Pipeline.Utils
{
	public class MessageUserCommandDispatcher : IUserCommandPipelineDispatcher<IMessage>, IDisposable
	{
		private readonly IClient client;
		private Action<IMessage, UserCommandSendData, Action<UserCommandResult>>? action = null;


		public MessageUserCommandDispatcher(IClient client)
		{
			this.client = client;
			client.MessageSent += Client_MessageSent;
		}

		public void Dispose()
		{
			client.MessageSent -= Client_MessageSent;
			GC.SuppressFinalize(this);
		}

		public void SetSyncCallback(Action<IMessage, UserCommandSendData, Action<UserCommandResult>> action)
		{
			this.action = action;
		}

		private void Client_MessageSent(IClient sender, IMessage message)
		{
			action?.Invoke(message, new(message.Author, message.TextChannel), callback);


			async void callback(UserCommandResult result)
			{
				IMessage? msg = null;

				if (result.RespondMessage is not null)
					msg = await message.TextChannel.SendMessageAsync(result.RespondMessage);
				else
				{
					if (result.Code != UserCommandCode.Sucssesful)
					{
						msg = await message.TextChannel.SendMessageAsync(new MessageSendModel("Error, command finished with code: " + result.Code));
					}
				}

				await Task.Delay(10000);
				if (msg is not null) try { await msg.DeleteAsync(); } catch (Exception) { }
				try { await message.DeleteAsync(); } catch (Exception) { }
			}
		}
	}
}

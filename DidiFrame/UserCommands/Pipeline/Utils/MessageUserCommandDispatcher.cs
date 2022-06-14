namespace DidiFrame.UserCommands.Pipeline.Utils
{
	/// <summary>
	/// Dispatcher that based on simple discord messages
	/// </summary>
	public class MessageUserCommandDispatcher : IUserCommandPipelineDispatcher<IMessage>, IDisposable
	{
		private readonly IClient client;
		private Action<IMessage, UserCommandSendData, Action<UserCommandResult>>? action = null;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.Utils.MessageUserCommandDispatcher
		/// </summary>
		/// <param name="client">Discord clint to access to discord</param>
		public MessageUserCommandDispatcher(IClient client)
		{
			this.client = client;
			client.MessageSent += Client_MessageSent;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			client.MessageSent -= Client_MessageSent;
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public void SetSyncCallback(Action<IMessage, UserCommandSendData, Action<UserCommandResult>> action)
		{
			this.action = action;
		}

		private void Client_MessageSent(IClient sender, IMessage message)
		{
			action?.Invoke(message, new(message.Author, message.TextChannel), callback);


			void callback(UserCommandResult result)
			{
				IMessage? msg = null;

				if (result.RespondMessage is not null)
					msg = message.TextChannel.SendMessageAsync(result.RespondMessage).Result;
				else
				{
					if (result.Code != UserCommandCode.Sucssesful)
					{
						msg = message.TextChannel.SendMessageAsync(new MessageSendModel("Error, command finished with code: " + result.Code)).Result;
					}
				}

				Thread.Sleep(60000);
				if (msg is not null) try { msg.DeleteAsync(); } catch (Exception) { }
				try { message.DeleteAsync(); } catch (Exception) { }
			}
		}
	}
}

namespace DidiFrame.UserCommands.Pipeline.Utils
{
	/// <summary>
	/// Dispatcher that based on simple discord messages
	/// </summary>
	public class MessageUserCommandDispatcher : IUserCommandPipelineDispatcher<IMessage>, IDisposable
	{
		private static readonly EventId EnableToDeleteMessageID = new(55, "EnableToDeleteMessage");


		private readonly IClient client;
		private readonly ILogger<MessageUserCommandDispatcher> logger;
		private DispatcherSyncCallback<IMessage>? callback = null;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.Utils.MessageUserCommandDispatcher
		/// </summary>
		/// <param name="client">Discord clint to access to discord</param>
		/// <param name="logger">Logger for dispatcher</param>
		public MessageUserCommandDispatcher(IClient client, ILogger<MessageUserCommandDispatcher> logger)
		{
			this.client = client;
			this.logger = logger;
			client.MessageSent += Client_MessageSent;
		}


		/// <inheritdoc/>
		public void Dispose()
		{
			client.MessageSent -= Client_MessageSent;
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public void FinalizePipeline(object stateObject)
		{
			var ss = (StateStruct)stateObject;

			Thread.Sleep(60000);

			try { ss.Message.DeleteAsync().Wait(); } catch(Exception ex)
			{ logger.Log(LogLevel.Warning, EnableToDeleteMessageID, ex, "Enable to delete call message with id {Id}", ss.Message.Id); }

			foreach (var msg in ss.Responses)
			{
				try
				{
					msg.DeleteAsync().Wait();
				}
				catch(Exception ex)
				{
					logger.Log(LogLevel.Warning, EnableToDeleteMessageID, ex, "Enable to delete respond message with id {Id}", msg.Id);
				}
			}
		}

		/// <inheritdoc/>
		public void Respond(object stateObject, UserCommandResult result)
		{
			var ss = (StateStruct)stateObject;

			IMessage? message = null;

			if (result.RespondMessage is not null)
				message = ss.Message.TextChannel.SendMessageAsync(result.RespondMessage).Result;
			else
			{
				if (result.Code != UserCommandCode.Sucssesful)
				{
					message = ss.Message.TextChannel.SendMessageAsync(new MessageSendModel("Error, command finished with code: " + result.Code)).Result;
				}
			}

			if (message is not null) ss.Responses.Add(message);
		}

		/// <inheritdoc/>
		public void SetSyncCallback(DispatcherSyncCallback<IMessage> callback)
		{
			this.callback = callback;
		}

		private void Client_MessageSent(IClient sender, IMessage message)
		{
			callback?.Invoke(this, message, new(message.Author, message.TextChannel), new StateStruct(sender, message));
		}


		private record StateStruct(IClient Sender, IMessage Message)
		{
			public List<IMessage> Responses { get; } = new();
		}
	}
}

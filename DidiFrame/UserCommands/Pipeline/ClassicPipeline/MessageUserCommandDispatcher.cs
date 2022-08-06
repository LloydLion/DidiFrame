using FluentValidation;

namespace DidiFrame.UserCommands.Pipeline.ClassicPipeline
{
	/// <summary>
	/// Dispatcher that based on simple discord messages
	/// </summary>
	public sealed class MessageUserCommandDispatcher : IUserCommandPipelineDispatcher<IMessage>, IDisposable
	{
		private static readonly EventId EnableToDeleteMessageID = new(55, "EnableToDeleteMessage");


		private readonly IClient client;
		private readonly ILogger<MessageUserCommandDispatcher> logger;
		private readonly IValidator<UserCommandResult> resultValidator;
		private DispatcherCallback<IMessage>? callback = null;


		/// <summary>
		/// Creates new instance of DidiFrame.UserCommands.Pipeline.Utils.MessageUserCommandDispatcher
		/// </summary>
		/// <param name="client">Discord clint to access to discord</param>
		/// <param name="logger">Logger for dispatcher</param>
		/// <param name="resultValidator">Validator for DidiFrame.UserCommands.Models.UserCommandResult</param>
		public MessageUserCommandDispatcher(IClient client, ILogger<MessageUserCommandDispatcher> logger, IValidator<UserCommandResult> resultValidator)
		{
			this.client = client;
			this.logger = logger;
			this.resultValidator = resultValidator;

			client.ServerCreated += Client_ServerCreated;
			foreach (var server in client.Servers) server.MessageSent += Client_MessageSent;
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			client.ServerCreated -= Client_ServerCreated;
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public void FinalizePipeline(object stateObject)
		{
			var ss = (StateStruct)stateObject;

			Thread.Sleep(60000);

			try { ss.Message.DeleteAsync().Wait(); }
			catch (Exception ex)
			{ logger.Log(LogLevel.Warning, EnableToDeleteMessageID, ex, "Enable to delete call message with id {Id}", ss.Message.Id); }

			foreach (var msg in ss.Responses)
			{
				try
				{
					msg.DeleteAsync().Wait();
				}
				catch (Exception ex)
				{
					logger.Log(LogLevel.Warning, EnableToDeleteMessageID, ex, "Enable to delete respond message with id {Id}", msg.Id);
				}
			}
		}

		/// <inheritdoc/>
		public async Task RespondAsync(object stateObject, UserCommandResult result)
		{
			resultValidator.ValidateAndThrow(result);

			var ss = (StateStruct)stateObject;

			IMessage? message = null;

			if (result.ResultType == UserCommandResult.Type.Message)
				message = await ss.Message.TextChannel.SendMessageAsync(result.GetRespondMessage());
			else if (result.ResultType == UserCommandResult.Type.None)
			{
				if (result.Code != UserCommandCode.Sucssesful)
				{
					message = ss.Message.TextChannel.SendMessageAsync(new MessageSendModel("Error, command finished with code: " + result.Code)).Result;
				}
			}
			else throw new NotSupportedException("Target type of user command result is not supported by dispatcher");

			if (message is not null) ss.Responses.Add(message);
		}

		/// <inheritdoc/>
		public void SetSyncCallback(DispatcherCallback<IMessage> callback)
		{
			this.callback = callback;
		}

		private void Client_MessageSent(IClient sender, IMessage message, bool isModified)
		{
			callback?.Invoke(this, message, new(message.Author, message.TextChannel), new StateStruct(sender, message));
		}

		private void Client_ServerCreated(IServer server)
		{
			server.MessageSent += Client_MessageSent;
		}


		private sealed record StateStruct(IClient Sender, IMessage Message)
		{
			public List<IMessage> Responses { get; } = new();
		}
	}
}

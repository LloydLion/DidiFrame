using DidiFrame.Clients.DSharp.Interactions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;

namespace DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories
{
	public class MessageRepository
	{
		private readonly Dictionary<ulong, MessageDynamicState> messageStates = new();
		private readonly List<ReferenceObserver> references = new();
		private readonly DSharpServer server;


		//TODO: imagine something with non-server context
		public MessageRepository(DSharpServer server)
		{
			this.server = server;
		}


		private DiscordClient DiscordClient => server.BaseClient.DiscordClient;


		public void Initialize()
		{
			DiscordClient.MessageDeleted += new AsyncEventHandler<DiscordClient, MessageDeleteEventArgs>(OnMessageDeleted).SyncIn(server.WorkQueue).FilterServer(server.Id);
		}

		public void PerformTerminate()
		{
			DiscordClient.MessageDeleted -= new AsyncEventHandler<DiscordClient, MessageDeleteEventArgs>(OnMessageDeleted).SyncIn(server.WorkQueue).FilterServer(server.Id);
		}

		public void RegistryReference(Reference reference)
		{
			CheckDynamicStates();

			references.Add(new ReferenceObserver(reference.MessageId, new WeakReference<object>(reference.ObservableObject)));
		}

		public MessageDynamicState EnsureDynamic(ulong messageId)
		{
			CheckDynamicStates();

			if (messageStates.TryGetValue(messageId, out var state))
				return state;
			else
			{
				var id = new MessageInteractionDispatcher(messageId, this);

				state = new MessageDynamicState(id);

				messageStates.Add(messageId, state);

				return state;
			}
		}

		public void DeleteDynamicState(ulong id)
		{
			CheckDynamicStates();

			references.RemoveAll(s => s.MessageId == id);
			messageStates.Remove(id);
		}

		public IUser ConvertAuthor(DiscordUser user)
		{
			return server.GetMember(user.Id);
		}

		private Task OnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
		{
			DeleteDynamicState(e.Message.Id);

			return Task.CompletedTask;
		}

		private void CheckDynamicStates()
		{
			var refsToDelete = new List<ReferenceObserver>();
			foreach (var item in references)
			{
				if (item.IsAlive == false)
					refsToDelete.Add(item);
			}

			references.RemoveAll((item) => refsToDelete.Contains(item));


			var msgToDelete = new List<ulong>();
			foreach (var message in messageStates)
			{
				if (message.Value.CanBeErased && GetReferenceFor(message.Key) is null)
				{
					msgToDelete.Add(message.Key);
				}
			}

			foreach (var message in msgToDelete)
				messageStates.Remove(message);
		}

		private ReferenceObserver? GetReferenceFor(ulong message)
		{
			foreach (var reference in references)
				if (reference.MessageId == message && reference.IsAlive)
					return reference;
			return null;
		}


		public sealed record MessageDynamicState(MessageInteractionDispatcher InteractionDispatcher)
		{
			public bool CanBeErased => InteractionDispatcher.HasSubscribers == false;
		}

		public readonly record struct Reference(ulong MessageId, object ObservableObject);

		private readonly record struct ReferenceObserver(ulong MessageId, WeakReference<object> ObservableObject)
		{
			public bool IsAlive => ObservableObject.TryGetTarget(out var _);
		}
	}
}

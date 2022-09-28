using DidiFrame.Entities.Message;
using DidiFrame.Exceptions;
using DidiFrame.Clients;

namespace DidiFrame.Clients.DSharp.Entities
{
	internal class NullChannel : ITextChannel, ITextThread, IVoiceChannel
	{
		public NullChannel(ulong id)
		{
			Id = id;
		}


		public string Name => throw new ObjectDoesNotExistException(nameof(Name));

		public ulong Id { get; }

		public IChannelCategory Category => throw new ObjectDoesNotExistException(nameof(Category));

		public bool IsExist => false;

		public IServer Server => throw new ObjectDoesNotExistException(nameof(Server));

		public IReadOnlyCollection<IMember> ConnectedMembers => throw new ObjectDoesNotExistException(nameof(ConnectedMembers));

		public ITextThreadContainerChannel Parent => throw new ObjectDoesNotExistException(nameof(Parent));


		public event MessageSentEventHandler? MessageSent
		{ add => throw new ObjectDoesNotExistException(nameof(MessageSent)); remove => throw new ObjectDoesNotExistException(nameof(MessageSent)); }

		public event MessageDeletedEventHandler? MessageDeleted
		{ add => throw new ObjectDoesNotExistException(nameof(MessageDeleted)); remove => throw new ObjectDoesNotExistException(nameof(MessageDeleted)); }


		public Task DeleteAsync()
		{
			throw new ObjectDoesNotExistException(nameof(DeleteAsync));
		}

		public bool Equals(IServerEntity? other)
		{
			return false;
		}

		public bool Equals(IChannel? other)
		{
			return false;
		}

		public IMessage GetMessage(ulong id)
		{
			throw new ObjectDoesNotExistException(nameof(GetMessage));
		}

		public IReadOnlyList<IMessage> GetMessages(int count = -1)
		{
			throw new ObjectDoesNotExistException(nameof(GetMessages));
		}

		public IReadOnlyCollection<ITextThread> GetThreads()
		{
			throw new ObjectDoesNotExistException(nameof(GetThreads));
		}

		public bool HasMessage(ulong id)
		{
			throw new ObjectDoesNotExistException(nameof(HasMessage));
		}

		public Task<IMessage> SendMessageAsync(MessageSendModel messageSendModel)
		{
			throw new ObjectDoesNotExistException(nameof(SendMessageAsync));
		}
	}
}
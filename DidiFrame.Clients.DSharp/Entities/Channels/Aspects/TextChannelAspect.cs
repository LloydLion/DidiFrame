using DidiFrame.Clients.DSharp.Operations;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Entities.Message;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities.Channels.Aspects
{
	public class TextChannelAspect : ChannelAspect<IDSharpTextChannelBase, TextChannelAspect.IProtectedContract>
	{
		public TextChannelAspect(IDSharpTextChannelBase inheritor, IProtectedContract contract, ChannelRepository repository) : base(inheritor, contract, repository) { }


		public async ValueTask<IServerMessage> GetMessageAsync(ulong id)
        {
            var rawMessage = await Contract.DoDiscordOperation(async () =>
            {
                return await Contract.AccessEntity().GetMessageAsync(id);
            });


            return new ServerMessage(Repository.MessageRepository, Inheritor, rawMessage);
        }

        public async ValueTask<IReadOnlyList<IServerMessage>> ListMessagesAsync(int limit = 25)
        {
            var list = await Contract.DoDiscordOperation(async () =>
            {
                return await Contract.AccessEntity().GetMessagesAsync(limit);
            });


            return list.Select(s => new ServerMessage(Repository.MessageRepository, Inheritor, s)).ToArray();
        }

        public async ValueTask<IServerMessage> SendMessageAsync(MessageSendModel message)
        {
            var messageBuilder = MessageConverter.ConvertUp(message);

            var newMessage = await Contract.DoDiscordOperation(async () =>
            {
                return await Contract.AccessEntity().SendMessageAsync(messageBuilder);
            });


            return new ServerMessage(Repository.MessageRepository, Inheritor, newMessage);
        }


        public interface IProtectedContract
        {
            public Task<TResult> DoDiscordOperation<TResult>(DiscordOperation<TResult> operation);

            public DiscordChannel AccessEntity();
		}
    }

    public interface IDSharpTextChannelBase : IDSharpMessageContainer, IDSharpChannel, ITextChannelBase
    {

    }
}

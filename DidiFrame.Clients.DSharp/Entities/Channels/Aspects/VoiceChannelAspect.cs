using DidiFrame.Clients.DSharp.Operations;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Clients.DSharp.Utils;
using DidiFrame.Utils.Collections;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities.Channels.Aspects
{
	public class VoiceChannelAspect : ChannelAspect<IDSharpVoiceChannelBase, VoiceChannelAspect.IProtectedContract>
	{
		public VoiceChannelAspect(IDSharpVoiceChannelBase inheritor, IProtectedContract contract, ChannelRepository repository) : base(inheritor, contract, repository) { }


		public IReadOnlyCollection<IMember> ListConnected()
		{
			return Contract.AccessState().ConnectedUsers;
		}

		public State BaseMutateWithNewObject(DiscordChannel discordObject)
		{
			return new State(new ReferenceEntityCollection<IMember>(discordObject.Users.Select(s => s.Id).ToArray(), Repository.MemberRepository));
		}


		public interface IProtectedContract
		{
			public State AccessState();
		}


		public record struct State(ReferenceEntityCollection<IMember> ConnectedUsers);
	}


	public interface IDSharpVoiceChannelBase : IDSharpChannel, IVoiceChannelBase
	{

	}
}

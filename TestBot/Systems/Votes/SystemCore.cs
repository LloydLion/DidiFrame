using DidiFrame.Lifetimes;

namespace TestBot.Systems.Votes
{
	internal class SystemCore
	{
		private readonly ILifetimesRegistry<VoteLifetime, VoteModel> repository;


		public SystemCore(ILifetimesRegistry<VoteLifetime, VoteModel> repository)
		{
			this.repository = repository;
		}


		public VoteLifetime CreateVote(IMember creator, ITextChannelBase channel, string title, IReadOnlyList<string> options)
		{
			var model = new VoteModel(creator, options, title, channel);
			return repository.RegistryLifetime(model);
		}
	}
}

using DidiFrame.Data.Lifetime;

namespace TestBot.Systems.Votes
{
	internal class SystemCore
	{
		private readonly IServersLifetimesRepository<VoteLifetime, VoteModel> repository;


		public SystemCore(IServersLifetimesRepository<VoteLifetime, VoteModel> repository)
		{
			this.repository = repository;
		}


		public VoteLifetime CreateVote(IMember creator, ITextChannelBase channel, string title, IReadOnlyList<string> options)
		{
			var model = new VoteModel(creator, options, title, channel);
			return repository.AddLifetime(model);
		}
	}
}

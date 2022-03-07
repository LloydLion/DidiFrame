using CGZBot3.Utils;

namespace CGZBot3.Systems.Reputation
{
	internal class MembersReputationRepository : IMembersReputationRepository
	{
		private readonly IServersStatesRepository<ICollection<MemberReputation>> repository;


		public MembersReputationRepository(IServersStatesRepositoryFactory repositoryFactory)
		{
			repository = repositoryFactory.Create<ICollection<MemberReputation>>(StatesKeys.ReputationSystem);
		}


		public ObjectHolder<MemberReputation> GetReputation(IMember member)
		{
			var state = repository.GetState(member.Server);

			var rp = state.Object.SingleOrDefault(s => s.Member.Equals((IUser)member));

			if (rp is null) rp = new MemberReputation(member);
			else state.Object.Remove(rp);

			return new ObjectHolder<MemberReputation>(rp, (holder) =>
			{
				state.Object.Add(holder.Object);
				state.Dispose();
			});
		}
	}
}

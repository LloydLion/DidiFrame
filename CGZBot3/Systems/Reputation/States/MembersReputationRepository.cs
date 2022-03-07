using CGZBot3.Utils;

namespace CGZBot3.Systems.Reputation.States
{
	internal class MembersReputationRepository : IMembersReputationRepository
	{
		private readonly IServersStatesRepository repository;
		private readonly ThreadLocker<IServer> locker = new();


		public MembersReputationRepository(IServersStatesRepository repository)
		{
			this.repository = repository;
		}


		public StateHandler<MemberReputation> GetReputation(IMember member)
		{
			var lockFree = locker.Lock(member.Server);

			var rpCol = repository.GetOrCreate<ICollection<MemberReputation>>(member.Server, StatesKeys.ReputationSystem);
			var rp = rpCol.SingleOrDefault(s => s.Member.Equals((IUser)member));

			if (rp is null) rp = new MemberReputation(member); //Not present
			else rpCol.Remove(rp);

			return new StateHandler<MemberReputation>(rp, (_) =>
			{
				rpCol.Add(rp);
				repository.Update(member.Server, rpCol, StatesKeys.ReputationSystem);
				lockFree.Dispose();
				return Task.CompletedTask;
			});
		}
	}
}

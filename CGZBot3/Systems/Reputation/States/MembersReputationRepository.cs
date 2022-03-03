using CGZBot3.Utils;

namespace CGZBot3.Systems.Reputation.States
{
	internal class MembersReputationRepository : IMembersReputationRepository
	{
		private readonly IServersStatesRepository repository;
		private readonly IModelConverter<MemberReputationPM, MemberReputation> converter;
		private readonly ThreadLocker<IServer> locker = new();


		public MembersReputationRepository(IServersStatesRepository repository, IModelConverter<MemberReputationPM, MemberReputation> converter)
		{
			this.repository = repository;
			this.converter = converter;
		}


		public async Task<StateHandler<MemberReputation>> GetReputationAsync(IMember member)
		{
			var lockFree = locker.Lock(member.Server);

			var rpCol = repository.GetOrCreate<ICollection<MemberReputationPM>>(member.Server, StatesKeys.ReputationSystem);
			var rp = rpCol.SingleOrDefault(s => s.Member == member.Id);

			if (rp is null) rp = new MemberReputationPM() { Member = member.Id }; //Not present
			else rpCol.Remove(rp);

			var ret = await converter.ConvertUpAsync(member.Server, rp);

			return new StateHandler<MemberReputation>(ret, async (_) =>
			{
				var pm = await converter.ConvertDownAsync(member.Server, ret);
				rpCol.Add(pm);
				repository.Update(member.Server, rpCol, StatesKeys.ReputationSystem);
				lockFree.Dispose();
			});
		}
	}
}

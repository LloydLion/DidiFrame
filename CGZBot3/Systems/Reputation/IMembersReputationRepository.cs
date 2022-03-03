using CGZBot3.Utils;

namespace CGZBot3.Systems.Reputation
{
	public interface IMembersReputationRepository
	{
		public Task<StateHandler<MemberReputation>> GetReputationAsync(IMember member);
	}
}

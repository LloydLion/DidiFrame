using CGZBot3.Utils;

namespace CGZBot3.Systems.Reputation
{
	public interface IMembersReputationRepository
	{
		public ObjectHolder<MemberReputation> GetReputation(IMember member);
	}
}

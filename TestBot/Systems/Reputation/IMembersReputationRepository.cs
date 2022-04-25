using DidiFrame.Utils;

namespace TestBot.Systems.Reputation
{
	public interface IMembersReputationRepository
	{
		public ObjectHolder<MemberReputation> GetReputation(IMember member);
	}
}

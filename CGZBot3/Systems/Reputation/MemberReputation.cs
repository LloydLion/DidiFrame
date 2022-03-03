namespace CGZBot3.Systems.Reputation
{
	public class MemberReputation
	{
		public IDictionary<ReputationType, int> Reputation { get; } = new Dictionary<ReputationType, int>();

		public IMember Member { get; }


		public MemberReputation(IMember member)
		{
			Member = member;
		}
	}
}

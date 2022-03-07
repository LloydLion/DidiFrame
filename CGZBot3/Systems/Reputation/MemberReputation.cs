using CGZBot3.Data.Model;

namespace CGZBot3.Systems.Reputation
{
	public class MemberReputation
	{
		private readonly Dictionary<ReputationType, int> rp = new();


		[ConstructorAssignableProperty(1, "initialReputation")]
		public IReadOnlyDictionary<ReputationType, int> Reputation => rp;

		[ConstructorAssignableProperty(0, "member")]
		public IMember Member { get; }


		public MemberReputation(IMember member, IReadOnlyDictionary<ReputationType, int> initialReputation) : this(member)
		{
			foreach (var item in initialReputation) rp[item.Key] = item.Value;
		}

		public MemberReputation(IMember member)
		{
			Member = member;

			foreach (var item in Enum.GetValues(typeof(ReputationType)).Cast<ReputationType>())
				rp.Add(item, 0);
		}


		public void Increase(ReputationType type, int amount)
		{
			rp[type] += amount;
		}

		public void Decrease(ReputationType type, int amount)
		{
			rp[type] -= amount;
		}
	}
}

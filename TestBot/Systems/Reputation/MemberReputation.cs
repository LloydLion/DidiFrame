using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Reputation
{
	[DataKey(StatesKeys.ReputationSystem)]
	public class MemberReputation : AbstractModel
	{
		[ModelProperty(PropertyType.Collection)]
		private ModelPrimitivesList<KeyValuePair<ReputationType, int>> ReputationInternal { get => new(WritableReputation); set => WritableReputation = value.ToDictionary(s => s.Key, s => s.Value); }

		public IReadOnlyDictionary<ReputationType, int> Reputation => WritableReputation;

		private Dictionary<ReputationType, int> WritableReputation { get => GetDataFromStore<Dictionary<ReputationType, int>>(nameof(Reputation)); set => SetDataToStore(value, nameof(Reputation)); }

		[ModelProperty(PropertyType.Primitive)]
		public IMember Member { get => GetDataFromStore<IMember>(); set => SetDataToStore(value); }

		public override IServer Server => Member.Server;


		public MemberReputation(IMember member, IReadOnlyDictionary<ReputationType, int> initialReputation) : this(member)
		{
			foreach (var item in initialReputation) WritableReputation[item.Key] = item.Value;
		}

		public MemberReputation(IMember member)
		{
			Member = member;

			foreach (var item in Enum.GetValues(typeof(ReputationType)).Cast<ReputationType>())
				WritableReputation.Add(item, 0);
		}

#nullable disable
		public MemberReputation(ISerializationModel model) : base(model) { }
#nullable restore


		public void Increase(ReputationType type, int amount)
		{
			WritableReputation[type] += amount;
		}

		public void Decrease(ReputationType type, int amount)
		{
			WritableReputation[type] -= amount;
		}
	}
}

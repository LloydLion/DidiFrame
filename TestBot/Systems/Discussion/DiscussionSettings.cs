using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Discussion
{
	[DataKey(SettingsKeys.DiscussionSystem)]
	internal class DiscussionSettings : IDataModel
	{
		[SerializationConstructor]
		public DiscussionSettings(IReadOnlyCollection<IChannelCategory> disscussionCategories)
		{
			DisscussionCategories = disscussionCategories;
		}


		[ConstructorAssignableProperty(0, "disscussionCategories")]
		public IReadOnlyCollection<IChannelCategory> DisscussionCategories { get; }

		public Guid Id { get; } = Guid.NewGuid();


		public bool Equals(IDataModel? other) => Equals(other, this);
	}
}

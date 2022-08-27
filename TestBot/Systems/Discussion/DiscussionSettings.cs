using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Discussion
{
	[DataKey(SettingsKeys.DiscussionSystem)]
	internal class DiscussionSettings : AbstractModel
	{
		public DiscussionSettings(IServer server, IReadOnlyCollection<IChannelCategory> disscussionCategories)
		{
			DisscussionCategoriesInternal = new(disscussionCategories);
			SetDataToStore(server, nameof(Server));
		}

#nullable disable
		public DiscussionSettings(ISerializationModel model) : base(model)
		{
			SetDataToStore(model.ReadPrimitive<IServer>(nameof(Server)), nameof(Server));
		}
#nullable restore


		[ModelProperty(PropertyType.Collection)]
		private ModelPrimitivesList<IChannelCategory> DisscussionCategoriesInternal
		{ get => GetDataFromStore<ModelPrimitivesList<IChannelCategory>>(nameof(DisscussionCategories)); set => SetDataToStore(value, nameof(DisscussionCategories)); }

		public IReadOnlyCollection<IChannelCategory> DisscussionCategories => (IReadOnlyCollection<IChannelCategory>)DisscussionCategoriesInternal;

		public override IServer Server => GetDataFromStore<IServer>();


		protected override void AdditionalSerializeTo(ISerializationModelBuilder builder)
		{
			builder.WritePrimitive(nameof(Server), Server);
		}
	}
}

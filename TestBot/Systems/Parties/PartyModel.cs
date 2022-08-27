using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Parties
{
	[DataKey(StatesKeys.PartiesSystem)]
	public class PartyModel : AbstractModel
	{
		public PartyModel(string name, IMember creator, ICollection<IMember> members)
		{
			Name = name;
			Creator = creator;
			MembersInternal = new(members);
		}

#nullable disable
		public PartyModel(ISerializationModel model) : base(model) { }
#nullable restore


		public string Name { get => GetDataFromStore<string>(); set => SetDataToStore(value); }

		public ModelPrimitivesList<IMember> MembersInternal { get => GetDataFromStore<ModelPrimitivesList<IMember>>(nameof(Members)); private set => SetDataToStore(value, nameof(Members)); }

		public ICollection<IMember> Members => MembersInternal;

		public IMember Creator { get => GetDataFromStore<IMember>(); private set => SetDataToStore(value); }

		public override IServer Server => Creator.Server;
	}
}

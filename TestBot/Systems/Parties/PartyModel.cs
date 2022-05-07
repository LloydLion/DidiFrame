using DidiFrame.Data.AutoKeys;
using DidiFrame.Data.Model;

namespace TestBot.Systems.Parties
{
	[DataKey(StatesKeys.PartiesSystem)]
	public class PartyModel
	{
		private static int nextId = 0;


		/// <summary>
		///	Serialization ctor
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="creator"></param>
		/// <param name="members"></param>
		public PartyModel(int id, string name, IMember creator, ICollection<IMember> members)
		{
			Id = id;
			Name = name;
			Creator = creator;
			Members = members;
			nextId = Math.Max(nextId, id);
		}

		public PartyModel(string name, IMember creator, IReadOnlyCollection<IMember> members) : this(++nextId, name, creator, members.ToList()) { }


		[ConstructorAssignableProperty(1, "name")]
		public string Name { get; set; }

		/// <summary>
		///	Exclude creator
		/// </summary>
		[ConstructorAssignableProperty(3, "members")]
		public ICollection<IMember> Members { get; }

		[ConstructorAssignableProperty(2, "creator")]
		public IMember Creator { get; }

		[ConstructorAssignableProperty(0, "id")]
		public int Id { get; }


		public override bool Equals(object? obj)
		{
			return obj is PartyModel model &&
				   Id == model.Id;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Id);
		}
	}
}

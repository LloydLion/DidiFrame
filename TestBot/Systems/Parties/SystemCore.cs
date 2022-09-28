using DidiFrame.Data.AutoKeys;
using DidiFrame.Utils;

namespace TestBot.Systems.Parties
{
	public class SystemCore : ISystemCore, ISystemNotifier
	{
		private readonly IServersStatesRepository<ICollection<PartyModel>> states;


		public SystemCore(IServersStatesRepository<ICollection<PartyModel>> states)
		{
			this.states = states;
		}


		public void CreateParty(string name, IMember creator, IReadOnlyCollection<IMember> members)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name mustn't be white space", nameof(name));
			if (members.Any(s => s == creator)) throw new ArgumentException("Members mustn't include creator", nameof(members));
			if (members.Any(s => s.IsBot)) throw new ArgumentException("No anyone member mustn't be bot", nameof(members));

			//ORDER is important!!
			if (HasParty(creator.Server, name)) throw new ArgumentException($"Party with name {name} already exits on this server", nameof(name));
			using var state = states.GetState(creator.Server).Open();

			var party = new PartyModel(name, creator, members);
			state.Object.Add(party);
		}

		public IObjectController<IReadOnlyCollection<PartyModel>> GetPartiesWith(IMember member)
		{
			var state = states.GetState(member.Server);
			return new SelectObjectContoller<ICollection<PartyModel>, IReadOnlyCollection<PartyModel>>(state, baseCollection => baseCollection.Where(s => s.Members.Contains(member) || s.Creator == member).ToArray());
		}

		public IObjectController<PartyModel> GetParty(IServer server, string name)
		{
			var state = states.GetState(server);
			return new SelectObjectContoller<ICollection<PartyModel>, PartyModel>(state, baseCollection => baseCollection.Single(s => s.Name == name));
		}

		public bool HasParty(IServer server, string name)
		{
			using var state = states.GetState(server).Open();
			return state.Object.Any(s => s.Name == name);
		}
	}
}

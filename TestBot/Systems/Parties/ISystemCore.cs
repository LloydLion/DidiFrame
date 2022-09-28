using DidiFrame.Utils;

namespace TestBot.Systems.Parties
{
	public interface ISystemCore
	{
		public void CreateParty(string name, IMember creator, IReadOnlyCollection<IMember> members);

		public IObjectController<PartyModel> GetParty(IServer server, string name);

		public bool HasParty(IServer server, string name);

		public IObjectController<IReadOnlyCollection<PartyModel>> GetPartiesWith(IMember member);
	}
}

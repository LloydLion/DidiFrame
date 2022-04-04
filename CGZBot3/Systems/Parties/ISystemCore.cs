using CGZBot3.Utils;

namespace CGZBot3.Systems.Parties
{
	public interface ISystemCore
	{
		public void CreateParty(string name, IMember creator, IReadOnlyCollection<IMember> members);

		public ObjectHolder<PartyModel> GetParty(IServer server, string name);

		public bool HasParty(IServer server, string name);

		public ObjectHolder<IReadOnlyCollection<PartyModel>> GetPartiesWith(IMember member);
	}
}

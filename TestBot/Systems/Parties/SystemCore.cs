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

		public ObjectHolder<IReadOnlyCollection<PartyModel>> GetPartiesWith(IMember member)
		{
			var state = states.GetState(member.Server).Open();
			var collection = state.Object.Where(s => s.Members.Contains(member) || s.Creator == member).ToArray();
			return new ObjectHolder<IReadOnlyCollection<PartyModel>>(collection, (_) =>
			{
				var exceptions = new List<Exception>();
				foreach (var party in collection)
				{
					var ex = ValidateParty((IReadOnlyCollection<PartyModel>)state.Object, party, () => state.Object.Remove(party));
					if (ex is not null) exceptions.Add(ex);
				}

				state.Dispose();

				if(exceptions.Count != 0) throw new AggregateException(exceptions);
			});
		}

		public ObjectHolder<PartyModel> GetParty(IServer server, string name)
		{
			bool ok = false;
			ObjectHolder<ICollection<PartyModel>>? state = null;

			try
			{
				state = states.GetState(server).Open();
				var party = state.Object.Single(s => s.Name == name);
				ok = true;
				return new ObjectHolder<PartyModel>(party, (_) => { var ex = ValidateParty((IReadOnlyCollection<PartyModel>)state.Object, party,
					() => state.Object.Remove(party)); state.Dispose(); if (ex is not null) throw ex; });
			}
			finally
			{
				if (!ok) state?.Dispose();
			}
		}

		public bool HasParty(IServer server, string name)
		{
			using var state = states.GetState(server).Open();
			return state.Object.Any(s => s.Name == name);
		}

		private static Exception? ValidateParty(IReadOnlyCollection<PartyModel> allParties, PartyModel party, Action callback)
		{
			if (party.Members.Any(s => s == party.Creator))
			{
				callback();
				return new InvalidOperationException("Members mustn't include creator");
			}

			if (party.Members.Any(s => s.IsBot))
			{
				callback();
				return new InvalidOperationException("No anyone member mustn't be bot");
			}

			if (allParties.Any(s => !s.Equals(party) && s.Name == party.Name))
			{
				callback();
				return new InvalidOperationException($"Party with same name ({party.Name}) already exits on this server");
			}

			if (string.IsNullOrWhiteSpace(party.Name))
			{
				callback();
				throw new InvalidOperationException("Name mustn't be white space");
			}

			return null;
		}
	}
}

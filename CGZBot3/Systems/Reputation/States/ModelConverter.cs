namespace CGZBot3.Systems.Reputation.States
{
	internal class ModelConverter : IModelConverter<MemberReputationPM, MemberReputation>
	{
		public Task<MemberReputationPM> ConvertDownAsync(IServer server, MemberReputation origin)
		{
			return Task.FromResult(new MemberReputationPM() { Member = origin.Member.Id, Reputation = origin.Reputation });
		}

		public async Task<MemberReputation> ConvertUpAsync(IServer server, MemberReputationPM pm)
		{
			var mr = new MemberReputation(await server.GetMemberAsync(pm.Member));

			foreach (var rptype in Enum.GetValues(typeof(ReputationType)))
			{
				var key = (ReputationType)rptype;
				if (pm.Reputation.TryGetValue(key, out var s))
					mr.Reputation.Add(key, s);
				else mr.Reputation.Add(key, 0);
			}

			return mr;
		}
	}
}

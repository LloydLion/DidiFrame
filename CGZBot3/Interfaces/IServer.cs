namespace CGZBot3.Interfaces
{
	internal interface IServer
	{
		public Task<IReadOnlyCollection<IMember>> GetMembersAsync();

		public Task<IMember> GetMemberAsync(string id);

		public string Name { get; }
	}
}

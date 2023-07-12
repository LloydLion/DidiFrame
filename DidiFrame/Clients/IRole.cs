namespace DidiFrame.Clients
{
	public interface IRole : IPermissionSubject
	{
		public Color Color { get; }

		public int Position { get; }

		public string? IconUrl { get; }

		public bool IsMentionable { get; }

		public bool IsHoisted { get; }
	}
}

using DidiFrame.Entities;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities
{
	public class Role : ServerObject, IRole
	{
		private DiscordRole? role;
		private Color color;
		private int position;
		private string? iconUrl;
		private Permissions permissions;
		private bool isMentionable;
		private bool isHoisted;

		public Role(Server baseServer, DiscordRole role) : base(baseServer, role)
		{
			this.role = role;

			WrapName = role.Name;
			color = role.Color.GetAbstract();
			position = role.Position;
			iconUrl = role.IconUrl;
			permissions = role.Permissions.GetAbstract();
			isMentionable = role.IsMentionable;
			isHoisted = role.IsHoisted;
		}

		public Role(Server baseServer, ulong id) : base(baseServer, id)
		{
			role = null;

			WrapName = string.Empty;
			color = default;
			position = 0;
			iconUrl = null;
			permissions = Permissions.None;
			isMentionable = false;
			isHoisted = false;
		}


		public Color Color { get => CheckAccess(color); private set => color = value; }

		public int Position { get => CheckAccess(position); private set => position = value; }

		public string? IconUrl { get => CheckAccess<string?>(iconUrl); private set => iconUrl = value; }

		public Permissions Permissions { get => CheckAccess(permissions); private set => permissions = value; }

		public bool IsMentionable { get => CheckAccess(isMentionable); private set => isMentionable = value; }

		public bool IsHoisted { get => CheckAccess(isHoisted); private set => isHoisted = value; }

		protected override string WrapName { get; set; }


		public override string? ToString()
		{
			return $"[Discord role ({Id})] {{Name={Name}; Position={Position}; Color={Color}; Permissions={(long)Permissions}; CreationTimeStamp={CreationTimeStamp}; IsMentionable={IsMentionable}; IsHoisted={IsHoisted}; IconUrl={IconUrl ?? "null"}}}";
		}

		internal Task MutateAsync(DiscordRole discordRole)
		{
			CheckAccess();

			role = discordRole;

			WrapName = role.Name;
			Color = role.Color.GetAbstract();
			Position = role.Position;
			IconUrl = role.IconUrl;
			Permissions = role.Permissions.GetAbstract();
			IsMentionable = role.IsMentionable;
			IsHoisted = role.IsHoisted;

			return NotifyModified();
		}


		protected override async ValueTask CallDeleteOperationAsync()
		{
			await AccessObject(role).DeleteAsync();
		}

		protected override async ValueTask CallRenameOperationAsync(string newName)
		{
			await AccessObject(role).ModifyAsync(name: newName);
		}
	}
}

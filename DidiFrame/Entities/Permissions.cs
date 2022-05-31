namespace DidiFrame.Entities
{
	/// <summary>
	/// Repesents discord permissions
	/// </summary>
	[Flags]
	public enum Permissions : long
	{
		/// <summary>
		/// No permissions for member
		/// </summary>
		None = 0x0L,
		/// <summary>
		/// All permissions for member
		/// </summary>
		All = 0x3FFFFFFFFFL,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		CreateInstantInvite = 0x1L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		KickMembers = 0x2L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		BanMembers = 0x4L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		Administrator = 0x8L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ManageChannels = 0x10L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ManageGuild = 0x20L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		AddReactions = 0x40L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ViewAuditLog = 0x80L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		PrioritySpeaker = 0x100L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		AccessChannels = 0x400L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		SendMessages = 0x800L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		SendTtsMessages = 0x1000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ManageMessages = 0x2000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		EmbedLinks = 0x4000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		AttachFiles = 0x8000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ReadMessageHistory = 0x10000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		MentionEveryone = 0x20000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		UseExternalEmojis = 0x40000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		UseVoice = 0x100000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		Speak = 0x200000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		MuteMembers = 0x400000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		DeafenMembers = 0x800000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		MoveMembers = 0x1000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		UseVoiceDetection = 0x2000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ChangeNickname = 0x4000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ManageNicknames = 0x8000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ManageRoles = 0x10000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ManageWebhooks = 0x20000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ManageEmojis = 0x40000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		Stream = 0x200L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		UseSlashCommands = 0x80000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		RequestToSpeak = 0x100000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		ManageThreads = 0x400000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		UsePublicThreads = 0x800000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		UsePrivateThreads = 0x1000000000L,
		/// <summary>
		/// See discord's documentation
		/// </summary>
		UseExternalStickers = 0x2000000000L
	}
}

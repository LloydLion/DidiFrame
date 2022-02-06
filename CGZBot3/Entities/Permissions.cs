using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Entities
{
	[Flags]
	public enum Permissions : long
	{
		None = 0x0L,
		All = 0x3FFFFFFFFFL,
		CreateInstantInvite = 0x1L,
		KickMembers = 0x2L,
		BanMembers = 0x4L,
		Administrator = 0x8L,
		ManageChannels = 0x10L,
		ManageGuild = 0x20L,
		AddReactions = 0x40L,
		ViewAuditLog = 0x80L,
		PrioritySpeaker = 0x100L,
		AccessChannels = 0x400L,
		SendMessages = 0x800L,
		SendTtsMessages = 0x1000L,
		ManageMessages = 0x2000L,
		EmbedLinks = 0x4000L,
		AttachFiles = 0x8000L,
		ReadMessageHistory = 0x10000L,
		MentionEveryone = 0x20000L,
		UseExternalEmojis = 0x40000L,
		UseVoice = 0x100000L,
		Speak = 0x200000L,
		MuteMembers = 0x400000L,
		DeafenMembers = 0x800000L,
		MoveMembers = 0x1000000L,
		UseVoiceDetection = 0x2000000L,
		ChangeNickname = 0x4000000L,
		ManageNicknames = 0x8000000L,
		ManageRoles = 0x10000000L,
		ManageWebhooks = 0x20000000L,
		ManageEmojis = 0x40000000L,
		Stream = 0x200L,
		UseSlashCommands = 0x80000000L,
		RequestToSpeak = 0x100000000L,
		ManageThreads = 0x400000000L,
		UsePublicThreads = 0x800000000L,
		UsePrivateThreads = 0x1000000000L,
		UseExternalStickers = 0x2000000000L
	}
}

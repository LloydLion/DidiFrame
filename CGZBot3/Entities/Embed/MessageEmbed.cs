﻿namespace CGZBot3.Entities.Embed
{
	public record MessageEmbed(string Title, string Description, Color Color, IReadOnlyCollection<EmbedField> Fields, EmbedMeta Metadata);
}

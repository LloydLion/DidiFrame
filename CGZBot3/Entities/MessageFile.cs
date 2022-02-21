namespace CGZBot3.Entities
{
	public record MessageFile(string FileName, TextReader reader, string FileType, string ContentType);
}

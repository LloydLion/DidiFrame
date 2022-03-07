namespace CGZBot3
{
	internal static class Extensions
	{
		public static string JoinWords(this IEnumerable<string> strs)
		{
			return string.Join(" ", strs);
		}
	}
}

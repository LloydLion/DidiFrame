namespace DidiFrame.Utils
{
	public class LocaleMap
	{
		private readonly IReadOnlyDictionary<string, string> dictionary;


		public LocaleMap(IReadOnlyDictionary<string, string> dictionary)
		{
			this.dictionary = dictionary;
		}


		public string TranscriptCode(string code) => dictionary[code];

		public bool CanTranscriptCode(string code) => dictionary.ContainsKey(code);
	}
}

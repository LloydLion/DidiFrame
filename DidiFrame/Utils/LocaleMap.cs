namespace DidiFrame.Utils
{
	/// <summary>
	/// Localization map to transcript some code to absolute locale keys
	/// </summary>
	public class LocaleMap
	{
		private readonly IReadOnlyDictionary<string, string> dictionary;


		/// <summary>
		/// Creates new instance on DidiFrame.Utils.LocaleMap based on dictionary
		/// </summary>
		/// <param name="dictionary">Base dictionary</param>
		public LocaleMap(IReadOnlyDictionary<string, string> dictionary)
		{
			this.dictionary = dictionary;
		}


		/// <summary>
		/// Transcripts code to absolute locale key
		/// </summary>
		/// <param name="code">Some code</param>
		/// <returns>Locale key</returns>
		public string TranscriptCode(string code) => dictionary[code];

		/// <summary>
		/// Checks can transcript code to absolute locale key
		/// </summary>
		/// <param name="code">Some code</param>
		/// <returns>If can transcript</returns>
		public bool CanTranscriptCode(string code) => dictionary.ContainsKey(code);
	}
}

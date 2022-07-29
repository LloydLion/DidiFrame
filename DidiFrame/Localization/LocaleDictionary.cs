using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Localization
{
	/// <summary>
	/// DidiFrame localization part. Don't use this class anywhere
	/// </summary>
	public class LocaleDictionary : IReadOnlyDictionary<string, string>
	{
		private readonly IReadOnlyDictionary<string, string> baseDic;


		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public LocaleDictionary(IReadOnlyDictionary<string, string> baseDic)
		{
			this.baseDic = baseDic;
		}


		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public string this[string key] => baseDic[key];


		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public IEnumerable<string> Keys => baseDic.Keys;

		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public IEnumerable<string> Values => baseDic.Values;

		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public int Count => baseDic.Count;


		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public bool ContainsKey(string key)
		{
			return baseDic.ContainsKey(key);
		}

		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return baseDic.GetEnumerator();
		}

		/// <summary>
		/// DidiFrame localization part. Don't use this member anywhere
		/// </summary>
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
		{
			return baseDic.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)baseDic).GetEnumerator();
		}
	}
}

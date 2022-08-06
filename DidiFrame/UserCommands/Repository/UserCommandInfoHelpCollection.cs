using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.UserCommands.Repository
{
	internal class UserCommandInfoHelpCollection : IList<UserCommandInfo>, IReadOnlyDictionary<string, UserCommandInfo>
	{
		private readonly List<UserCommandInfo> orderedMessages = new();
		private readonly Dictionary<string, UserCommandInfo> keyPairs = new();


		public UserCommandInfo this[int index]
		{
			get => orderedMessages[index];
			set
			{
				var res = orderedMessages[index];
				keyPairs[res.Name] = value;
				orderedMessages[index] = value;
			}
		}

		public UserCommandInfo this[string key] => keyPairs[key];


		public int Count => orderedMessages.Count;

		public bool IsReadOnly => false;

		public IEnumerable<string> Keys => keyPairs.Keys;

		public IEnumerable<UserCommandInfo> Values => keyPairs.Values;


		public void Add(UserCommandInfo item)
		{
			orderedMessages.Add(item);
			keyPairs.Add(item.Name, item);
		}

		public void Clear()
		{
			keyPairs.Clear();
			orderedMessages.Clear();
		}

		public bool Contains(UserCommandInfo item)
		{
			return orderedMessages.Contains(item);
		}

		public bool ContainsKey(string key)
		{
			return keyPairs.ContainsKey(key);
		}

		public void CopyTo(UserCommandInfo[] array, int arrayIndex)
		{
			orderedMessages.CopyTo(array, arrayIndex);
		}

		public IEnumerator<UserCommandInfo> GetEnumerator()
		{
			return orderedMessages.GetEnumerator();
		}

		public int IndexOf(UserCommandInfo item)
		{
			return orderedMessages.IndexOf(item);
		}

		public void Insert(int index, UserCommandInfo item)
		{
			keyPairs.Add(item.Name, item);
			orderedMessages.Insert(index, item);
		}

		public bool Remove(UserCommandInfo item)
		{
			keyPairs.Remove(item.Name);
			return orderedMessages.Remove(item);
		}

		public void RemoveAt(int index)
		{
			keyPairs.Remove(orderedMessages[index].Name);
			orderedMessages.RemoveAt(index);
		}

		public bool TryGetValue(string key, [MaybeNullWhen(false)] out UserCommandInfo value)
		{
			return keyPairs.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<KeyValuePair<string, UserCommandInfo>> IEnumerable<KeyValuePair<string, UserCommandInfo>>.GetEnumerator()
		{
			return keyPairs.GetEnumerator();
		}

		public IEnumerable<UserCommandInfo> AsEnumerable() => this;
	}
}

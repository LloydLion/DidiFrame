using System.Collections;

namespace DidiFrame.Utils.Collections
{
	/// <summary>
	/// Collection that contains only one element
	/// </summary>
	/// <typeparam name="T">Collection type</typeparam>
	public class SingleElementCollection<T> : IReadOnlyList<T>
	{
		private readonly T element;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.Collections.SingleElementCollection`1 using single element
		/// </summary>
		/// <param name="element">Element to store</param>
		public SingleElementCollection(T element)
		{
			this.element = element;
		}


		/// <inheritdoc/>
		public T this[int index]
		{
			get
			{
				if (index != 0)
					throw new IndexOutOfRangeException();
				else return element;
			}
		}


		/// <inheritdoc/>
		public int Count => 1;


		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			yield return element;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	/// <summary>
	/// Extensions from DidiFrame.Utils.Collections.SingleElementCollection`1
	/// </summary>
	public static class SingleElementCollectionExtensions
	{
		/// <summary>
		/// Create new instance of DidiFrame.Utils.Collections.SingleElementCollection`1
		/// </summary>
		/// <typeparam name="T">Type of collection</typeparam>
		/// <param name="obj">Object to store</param>
		/// <returns>New single element collection</returns>
		public static IReadOnlyList<T> StoreSingle<T>(this T obj)
		{
			return new SingleElementCollection<T>(obj);
		}
	}
}

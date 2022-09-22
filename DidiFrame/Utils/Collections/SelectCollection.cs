using System.Collections;

namespace DidiFrame.Utils.Collections
{
	/// <summary>
	/// Collection utility that casts one collection to other, don't use with value types
	/// </summary>
	/// <typeparam name="T">Target collection type</typeparam>
	public class SelectCollection<T> : IReadOnlyCollection<T>
	{
		private readonly IReadOnlyCollection<object> originalCollection;
		private readonly Func<object, T> selector;


		/// <summary>
		/// Creates new DidiFrame.Utils.Collections.SelectCollection`1. Don't use constructor, use Create method
		/// </summary>
		/// <param name="originalCollection">Original collection of objects</param>
		/// <param name="selector">Selector from collection to T object</param>
		public SelectCollection(IReadOnlyCollection<object> originalCollection, Func<object, T> selector)
		{
			this.originalCollection = originalCollection;
			this.selector = selector;
		}


		/// <summary>
		/// Creates new DidiFrame.Utils.Collections.SelectCollection`1
		/// </summary>
		/// <typeparam name="TOriginal">Type of original collection</typeparam>
		/// <param name="originalCollection">Original collection of objects</param>
		/// <param name="selector">Selector from collection to T object</param>
		/// <returns>New instance of SelectCollection</returns>
		public static SelectCollection<T> Create<TOriginal>(IReadOnlyCollection<TOriginal> originalCollection, Func<TOriginal, T> selector) where TOriginal : class
		{
			return new SelectCollection<T>(originalCollection, s => selector((TOriginal)s));
		}


		/// <inheritdoc/>
		public int Count => originalCollection.Count;


		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator()
		{
			foreach (var item in originalCollection)
			{
				yield return selector(item);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}

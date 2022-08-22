using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Utils.Collections
{
	public class SelectCollection<T> : IReadOnlyCollection<T>
	{
		private readonly IReadOnlyCollection<object> originalCollection;
		private readonly Func<object, T> selector;


		public SelectCollection(IReadOnlyCollection<object> originalCollection, Func<object, T> selector)
		{
			this.originalCollection = originalCollection;
			this.selector = selector;
		}


		public static SelectCollection<T> Create<TOriginal>(IReadOnlyCollection<TOriginal> originalCollection, Func<TOriginal, T> selector)
		{
			return new SelectCollection<T>((IReadOnlyCollection<object>)originalCollection, s => selector((TOriginal)s));
		}


		public int Count => originalCollection.Count;


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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Data.Model
{
	public class ModelPrimitivesList<TElement> : IList<TElement>, IDataCollection<TElement>
	{
		private readonly ObservableCollection<TElement> innerCollection;
		private int writingAbility = 0;


		public event NotifyCollectionChangedEventHandler? CollectionChanged
		{ add => innerCollection.CollectionChanged += value; remove => innerCollection.CollectionChanged -= value; }


		public int Count => ((ICollection<TElement>)innerCollection).Count;

		public bool IsReadOnly => ((ICollection<TElement>)innerCollection).IsReadOnly;

		public Type ElementType => typeof(TElement);


		public TElement this[int index] { get => ((IList<TElement>)innerCollection)[index]; set => ((IList<TElement>)innerCollection)[index] = value; }


		public ModelPrimitivesList(IEnumerable<TElement> elements)
		{
			innerCollection = new(elements);
		}

		public ModelPrimitivesList()
		{
			innerCollection = new();
		}


		public IDisposable PreventWritings()
		{
			return new WritingsPreventer(this);
		}

		public int IndexOf(TElement item)
		{
			return ((IList<TElement>)innerCollection).IndexOf(item);
		}

		public void Insert(int index, TElement item)
		{
			ThrowIfCannotWrite();
			((IList<TElement>)innerCollection).Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			ThrowIfCannotWrite();
			((IList<TElement>)innerCollection).RemoveAt(index);
		}

		public void Add(TElement item)
		{
			ThrowIfCannotWrite();
			((ICollection<TElement>)innerCollection).Add(item);
		}

		public void Clear()
		{
			ThrowIfCannotWrite();
			((ICollection<TElement>)innerCollection).Clear();
		}

		public bool Contains(TElement item)
		{
			return ((ICollection<TElement>)innerCollection).Contains(item);
		}

		public void CopyTo(TElement[] array, int arrayIndex)
		{
			((ICollection<TElement>)innerCollection).CopyTo(array, arrayIndex);
		}

		public bool Remove(TElement item)
		{
			ThrowIfCannotWrite();
			return ((ICollection<TElement>)innerCollection).Remove(item);
		}

		public IEnumerator<TElement> GetEnumerator()
		{
			return ((IEnumerable<TElement>)innerCollection).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)innerCollection).GetEnumerator();
		}

		private void ThrowIfCannotWrite(int targetIndex = -1)
		{
			if (writingAbility != 0)
			{
				if (targetIndex == -1) throw new ModelBlockedToWriteException();
				else throw new ModelBlockedToWriteException(targetIndex);
			}
		}


		private sealed class WritingsPreventer : IDisposable
		{
			private readonly ModelPrimitivesList<TElement> owner;


			public WritingsPreventer(ModelPrimitivesList<TElement> owner)
			{
				owner.writingAbility += 1;
				this.owner = owner;
			}


			public void Dispose()
			{
				owner.writingAbility -= 1;
			}
		}
	}
}

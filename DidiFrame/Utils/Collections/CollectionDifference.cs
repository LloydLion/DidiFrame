using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Utils.Collections
{
	public class CollectionDifference<TInput, TTarget, TKey> where TInput : notnull where TTarget : notnull where TKey : notnull
	{
		private readonly IReadOnlyCollection<TInput> from;
		private readonly IReadOnlyCollection<TTarget> to;
		private readonly Func<TInput, TKey> fromKeySelector;
		private readonly Func<TTarget, TKey> toKeySelector;


		public CollectionDifference(IReadOnlyCollection<TInput> from, IReadOnlyCollection<TTarget> to, Func<TInput, TKey> fromKeySelector, Func<TTarget, TKey> toKeySelector)
		{
			this.from = from;
			this.to = to;
			this.fromKeySelector = fromKeySelector;
			this.toKeySelector = toKeySelector;
		}


		public IReadOnlyCollection<Difference> CalculateDifference()
		{
			var difference = new List<Difference>();

			var dynamicCol = new List<TTarget>();
			var clone = to.ToDictionary(s => toKeySelector(s));

			foreach (var item in from)
			{
				var key = fromKeySelector(item);
				if (clone.ContainsKey(key))
				{
					dynamicCol.Add(clone[key]);
					clone.Remove(key);
				}
				else
				{
					difference.Add(new(key, OperationType.Add));
				}
			}

			foreach (var item in clone) difference.Add(new(item.Key, OperationType.Remove));

			return difference;
		}


		public struct Difference
		{
			public Difference(TKey key, OperationType type)
			{
				Key = key;
				Type = type;
			}


			public TKey Key { get; }

			public OperationType Type { get; }
		}

		public enum OperationType
		{
			Add,
			Remove
		}
	}
}

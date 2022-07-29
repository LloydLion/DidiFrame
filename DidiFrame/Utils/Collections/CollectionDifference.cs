namespace DidiFrame.Utils.Collections
{
	/// <summary>
	/// Helper to calculate collections difference
	/// </summary>
	/// <typeparam name="TInput">Type of main collection</typeparam>
	/// <typeparam name="TTarget">Type of target collection</typeparam>
	/// <typeparam name="TKey">Type of key that will be used to join collections</typeparam>
	public class CollectionDifference<TInput, TTarget, TKey> where TInput : notnull where TTarget : notnull where TKey : notnull
	{
		private readonly IReadOnlyCollection<TInput> from;
		private readonly IReadOnlyCollection<TTarget> to;
		private readonly Func<TInput, TKey> fromKeySelector;
		private readonly Func<TTarget, TKey> toKeySelector;


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.Collections.CollectionDifference`3
		/// </summary>
		/// <param name="from">Collection that will be used as a reference</param>
		/// <param name="to">Collection to get difference with reference</param>
		/// <param name="fromKeySelector">Key selector for FROM colleciton</param>
		/// <param name="toKeySelector">Key selector for TO colelction</param>
		public CollectionDifference(IReadOnlyCollection<TInput> from, IReadOnlyCollection<TTarget> to, Func<TInput, TKey> fromKeySelector, Func<TTarget, TKey> toKeySelector)
		{
			this.from = from;
			this.to = to;
			this.fromKeySelector = fromKeySelector;
			this.toKeySelector = toKeySelector;
		}


		/// <summary>
		/// Calculates operation set that make from target collection (TO) reference collection (FROM)
		/// </summary>
		/// <returns>Operation set</returns>
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
					difference.Add(new(key, CollectionDifference.OperationType.Add));
				}
			}

			foreach (var item in clone) difference.Add(new(item.Key, CollectionDifference.OperationType.Remove));

			return difference;
		}


		/// <summary>
		/// Represents difference between two collections
		/// </summary>
		public struct Difference
		{
			/// <summary>
			/// Creates new instance of DidiFrame.Utils.Collections.CollectionDifference`3.Difference
			/// </summary>
			/// <param name="key">Key of target element</param>
			/// <param name="type">Type of operation</param>
			public Difference(TKey key, CollectionDifference.OperationType type)
			{
				Key = key;
				Type = type;
			}


			/// <summary>
			/// Key of target element
			/// </summary>
			public TKey Key { get; }

			/// <summary>
			/// Type of operation
			/// </summary>
			public CollectionDifference.OperationType Type { get; }
		}

	}

	/// <summary>
	/// Static subclass for DidiFrame.Utils.Collections.CollectionDifference`3
	/// </summary>
	public static class CollectionDifference
	{
		/// <summary>
		/// Type of operation between two collection
		/// </summary>
		public enum OperationType
		{
			/// <summary>
			/// Add element from reference to target
			/// </summary>
			Add,
			/// <summary>
			/// Remove element from target
			/// </summary>
			Remove
		}
	}
}

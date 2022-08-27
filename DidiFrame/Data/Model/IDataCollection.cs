using System.Collections;
using System.Collections.Specialized;

namespace DidiFrame.Data.Model
{
	/*
	 * Implementator must has public constuctor with single paramter of IEnumerable<TElement> type
	 */
	public interface IDataCollection<out TElement> : IDataCollection, IEnumerable<TElement>
	{
		
	}

	public interface IDataCollection : IDataEntity, INotifyCollectionChanged, IEnumerable
	{
		public Type ElementType { get; }
	}
}

﻿namespace DidiFrame.Clients
{
	/// <summary>
	/// Represents discord category
	/// </summary>
	public interface ICategory : IServerObject
	{
		/// <summary>
		/// Lists all items in this category
		/// </summary>
		/// <returns>Collection of items</returns>
		public IReadOnlyCollection<ICategoryItem> ListItems();

		/// <summary>
		/// Gets category item by id
		/// </summary>
		/// <typeparam name="TItem">Target item type</typeparam>
		/// <param name="id">Id of item</param>
		/// <returns>Found item</returns>
		public TItem GetItem<TItem>(ulong id) where TItem : class, ICategoryItem;

		public IChannelPermissions ManagePermissions();


		/// <summary>
		/// If category represents global server's items space
		/// </summary>
		public bool IsGlobal { get; }
	}
}

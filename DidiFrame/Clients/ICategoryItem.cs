namespace DidiFrame.Clients
{
	/// <summary>
	/// Represents category item
	/// </summary>
	public interface ICategoryItem : IServerObject
	{
		/// <summary>
		/// Parent category
		/// </summary>
		public ICategory Category { get; }


		/// <summary>
		/// Moves item from one category to another
		/// </summary>
		/// <param name="category">New category</param>
		/// <returns>Wait task</returns>
		public ValueTask ChangeCategoryAsync(ICategory category);
	}
}
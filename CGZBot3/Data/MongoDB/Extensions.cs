using MongoDB.Driver;

namespace CGZBot3.Data.MongoDB
{
	internal static class Extensions
	{
		public static async IAsyncEnumerator<IEnumerable<TModel>> GetAsyncEnumerator<TModel>(this IAsyncCursor<TModel> cursor)
		{
			while(await cursor.MoveNextAsync()) yield return cursor.Current;
			cursor.Dispose();
		}
	}
}

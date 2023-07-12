using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using System.Collections;

namespace DidiFrame.Clients.DSharp.Utils
{
	public class ReferenceEntityCollection<TDiscordEntity> : IReadOnlyList<TDiscordEntity>
	{
		private readonly IReadOnlyList<ulong> baseList;
		private readonly IEntityRepository<TDiscordEntity> repository;


		public ReferenceEntityCollection(IReadOnlyList<ulong> baseList, IEntityRepository<TDiscordEntity> repository)
		{
			this.baseList = baseList;
			this.repository = repository;
		}


		public TDiscordEntity this[int index] => repository.GetById(baseList[index]);

		public int Count => baseList.Count;


		public IEnumerator<TDiscordEntity> GetEnumerator()
		{
			foreach (var item in baseList)
				yield return repository.GetById(item);
		}

		public IReadOnlyList<ulong> GetBaseList() => baseList;

		public override string ToString()
		{
			return $"[{string.Join(", ", baseList)}]";
		}

		public override bool Equals(object? obj) => obj is ReferenceEntityCollection<TDiscordEntity> rec && rec.baseList.SequenceEqual(baseList);

		public override int GetHashCode() => baseList.GetHashCode();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	}
}

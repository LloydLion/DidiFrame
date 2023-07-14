using DidiFrame.Clients.DSharp.Utils;
using DidiFrame.Utils;
using AEHAdd = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.ChannelCreateEventArgs>;
using AEHUpdate = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.ChannelUpdateEventArgs>;
using AEHRemove = Emzi0767.Utilities.AsyncEventHandler<DSharpPlus.DiscordClient, DSharpPlus.EventArgs.ChannelDeleteEventArgs>;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using System.Collections;
using Microsoft.VisualBasic;

namespace DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories
{
	public class CategoryRepository : IEntityRepository<IDSharpCategory>, IEntityRepository<ICategory>
	{
		private readonly GlobalCategory globalCategory;
		private readonly Dictionary<ulong, Category> categories = new();
		private readonly DSharpServer server;
		private readonly List<IEntityRepository<ICategoryItem>> categoryItemRepositories = new();
		private readonly EventBuffer eventBuffer;
		private readonly CategoryCollection categoryCollection;


		public CategoryRepository(DSharpServer server, EventBuffer eventBuffer)
		{
			this.server = server;
			this.eventBuffer = eventBuffer;

			globalCategory = new GlobalCategory(server, this);

			categoryCollection = new CategoryCollection(categories.Values, globalCategory);
		}


		private DiscordClient DiscordClient => server.BaseClient.DiscordClient;


		public IReadOnlyCollection<IDSharpCategory> GetAll() => categoryCollection;

		public IDSharpCategory GetById(ulong id)
		{
			if (id == server.Id)
				return globalCategory;
			else return categories[id];
		}

		IReadOnlyCollection<ICategory> IEntityRepository<ICategory>.GetAll() => GetAll();

		ICategory IEntityRepository<ICategory>.GetById(ulong id) => GetById(id);

		public async Task InitializeAsync(CompositeAsyncDisposable postInitializationContainer, IEnumerable<IEntityRepository<ICategoryItem>> categoryItemRepositories)
		{
			this.categoryItemRepositories.AddRange(categoryItemRepositories);

			postInitializationContainer.PushDisposable(globalCategory.Initialize());

			foreach (var category in (await server.BaseGuild.GetChannelsAsync()).Where(s => s.IsCategory))
			{
				var scategory = new Category(server, category.Id, this);
				categories.Add(scategory.Id, scategory);

				var disposable = scategory.Initialize(category);

				postInitializationContainer.PushDisposable(disposable);
			}

			DiscordClient.ChannelCreated += new AEHAdd(OnCategoryCreated).SyncIn(server.WorkQueue).Filter(IsCategory).FilterServer(server.Id);
			DiscordClient.ChannelUpdated += new AEHUpdate(OnCategoryUpdated).SyncIn(server.WorkQueue).Filter(IsCategory).FilterServer(server.Id);
			DiscordClient.ChannelDeleted += new AEHRemove(OnCategoryDeleted).SyncIn(server.WorkQueue).Filter(IsCategory).FilterServer(server.Id);
		}

		public void PerformTerminate()
		{
			DiscordClient.ChannelCreated -= new AEHAdd(OnCategoryCreated).SyncIn(server.WorkQueue).Filter(IsCategory).FilterServer(server.Id);
			DiscordClient.ChannelUpdated -= new AEHUpdate(OnCategoryUpdated).SyncIn(server.WorkQueue).Filter(IsCategory).FilterServer(server.Id);
			DiscordClient.ChannelDeleted -= new AEHRemove(OnCategoryDeleted).SyncIn(server.WorkQueue).Filter(IsCategory).FilterServer(server.Id);
		}

		public Task TerminateAsync()
		{
			return Task.CompletedTask;
		}

		public Task DeleteAsync(Category category)
		{
			categories.Remove(category.Id);
			return category.Finalize().DisposeAsync().AsTask();
		}

		public IReadOnlyCollection<ICategoryItem> GetItemsFor(IDSharpCategory targetCategory)
		{
			var result = new List<ICategoryItem>(capacity: 10);

			foreach (var repository in categoryItemRepositories)
				result.AddRange(repository.GetAll().Where(s => Equals(s.Category, targetCategory)));

			return result;
		}

		public TItem GetItemIn<TItem>(ulong id, IDSharpCategory targetCategory) where TItem : ICategoryItem
		{
			foreach (var repository in categoryItemRepositories)
			{
				var searchResult = repository.GetAll().OfType<TItem>().FirstOrDefault(s => Equals(s.Category, targetCategory) && s.Id == id);
				if (searchResult is not null)
					return searchResult;
			}

			throw new KeyNotFoundException($"No such {typeof(TItem).Name} with id {id} in {targetCategory.Id} category");
		}

		private Task OnCategoryCreated(DiscordClient sender, ChannelCreateEventArgs e) => CreateOrUpdateAsync(e.Channel);

		private Task OnCategoryUpdated(DiscordClient sender, ChannelUpdateEventArgs e) => CreateOrUpdateAsync(e.ChannelAfter);

		private Task OnCategoryDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
		{
			if (categories.TryGetValue(e.Channel.Id, out var member))
			{
				categories.Remove(member.Id);

				var disposable = member.Finalize();

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}

			return Task.CompletedTask;
		}

		private Task CreateOrUpdateAsync(DiscordChannel category)
		{
			if (categories.TryGetValue(category.Id, out var scategory))
			{
				var disposable = scategory.Mutate(category);

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}
			else
			{
				scategory = new Category(server, category.Id, this);
				categories.Add(scategory.Id, scategory);

				var disposable = scategory.Initialize(category);

				eventBuffer.Dispatch(async () => await disposable.DisposeAsync());
			}

			return Task.CompletedTask;
		}

		private bool IsCategory(ChannelCreateEventArgs args) => args.Channel.IsCategory;

		private bool IsCategory(ChannelUpdateEventArgs args) => args.ChannelAfter.IsCategory;

		private bool IsCategory(ChannelDeleteEventArgs args) => args.Channel.IsCategory;


		private sealed class CategoryCollection : IReadOnlyCollection<IDSharpCategory>
		{
			private readonly IReadOnlyCollection<Category> categories;
			private readonly GlobalCategory globalCategory;


			public CategoryCollection(IReadOnlyCollection<Category> categories, GlobalCategory globalCategory)
			{
				this.categories = categories;
				this.globalCategory = globalCategory;
			}


			public int Count => categories.Count + 1;


			public IEnumerator<IDSharpCategory> GetEnumerator()
			{
				yield return globalCategory;
				foreach (var item in categories)
					yield return item;
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}

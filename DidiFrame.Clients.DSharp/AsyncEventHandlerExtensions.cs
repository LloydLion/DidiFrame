using DidiFrame.Threading;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Clients.DSharp
{
	public static class AsyncEventHandlerExtensions
	{
		public static AsyncEventHandler<TSender, TArgs> FilterServer<TSender, TArgs>(this AsyncEventHandler<TSender, TArgs> handler, ulong serverId) where TArgs : DiscordEventArgs
		{
			return new FilterServerDecorator<TSender, TArgs>(handler, serverId).WrappedHandler;
		}

		public static AsyncEventHandler<TSender, TArgs> Filter<TSender, TArgs>(this AsyncEventHandler<TSender, TArgs> handler, Predicate<TArgs> criterion) where TArgs : DiscordEventArgs
		{
			return new FilterDecorator<TSender, TArgs>(handler, criterion).WrappedHandler;
		}

		public static AsyncEventHandler<TSender, TArgs> SyncIn<TSender, TArgs>(this AsyncEventHandler<TSender, TArgs> handler, IManagedThreadExecutionQueue queue) where TArgs : DiscordEventArgs
		{
			return new SyncInDecorator<TSender, TArgs>(handler, queue).WrappedHandler;
		}


		private sealed class SyncInDecorator<TSender, TArgs> : AsyncEventHandlerDecorator<TSender, TArgs> where TArgs : DiscordEventArgs
		{
			private readonly IManagedThreadExecutionQueue queue;


			public SyncInDecorator(AsyncEventHandler<TSender, TArgs> handler, IManagedThreadExecutionQueue queue) : base(handler)
			{
				this.queue = queue;
			}


			public override Task WrappedHandler(TSender sender, TArgs args)
			{
				return queue.AwaitDispatchAsyncIgnoreEx(() =>
				{
					return new(Handler(sender, args));
				});
			}


			public override bool Equals(object? obj) => EqualsInternal<SyncInDecorator<TSender, TArgs>>(obj, s => Equals(s.queue, queue));

			public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), queue);
		}


		private sealed class FilterServerDecorator<TSender, TArgs> : FilterDecorator<TSender, TArgs> where TArgs : DiscordEventArgs
		{
			public FilterServerDecorator(AsyncEventHandler<TSender, TArgs> handler, ulong serverId) : base(handler, new Criterion(serverId).Match) { }


			private sealed record Criterion(ulong ServerId)
			{
				public bool Match(TArgs args)
				{
					dynamic dargs = args;
					DiscordGuild guild = dargs.Guild;

					return guild.Id == ServerId;
				}
			}
		}

		private class FilterDecorator<TSender, TArgs> : AsyncEventHandlerDecorator<TSender, TArgs> where TArgs : DiscordEventArgs
		{
			private readonly Predicate<TArgs> criterion;


			public FilterDecorator(AsyncEventHandler<TSender, TArgs> handler, Predicate<TArgs> criterion) : base(handler)
			{
				this.criterion = criterion;
			}


			public sealed override Task WrappedHandler(TSender sender, TArgs args)
			{
				if (criterion(args))
					return Handler(sender, args);
				else return Task.CompletedTask;
			}

			public sealed override bool Equals(object? obj) => EqualsInternal<FilterDecorator<TSender, TArgs>>(obj, s => s.criterion == criterion);

			public sealed override int GetHashCode() => HashCode.Combine(base.GetHashCode(), criterion);
		}

		private abstract class AsyncEventHandlerDecorator<TSender, TArgs> where TArgs : DiscordEventArgs
		{
			private readonly AsyncEventHandler<TSender, TArgs> handler;


			protected AsyncEventHandlerDecorator(AsyncEventHandler<TSender, TArgs> handler)
			{
				this.handler = handler;
			}


			protected AsyncEventHandler<TSender, TArgs> Handler => handler;


			[SuppressMessage("CodeQuality", "IDE0079")]
			[SuppressMessage("Major Code Smell", "S1144")]
			public abstract Task WrappedHandler(TSender sender, TArgs args);

			public abstract override bool Equals(object? obj);

			protected virtual bool EqualsInternal<T>(object? obj, Predicate<T> additionalEquals) where T : AsyncEventHandlerDecorator<TSender, TArgs>
				=> obj is T decorator && Equals(decorator.handler, handler) && additionalEquals(decorator);

			public override int GetHashCode() => handler.GetHashCode();
		}
	}
}

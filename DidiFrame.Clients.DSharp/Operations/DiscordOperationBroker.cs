using DidiFrame.Exceptions;
using DSharpPlus.Exceptions;

namespace DidiFrame.Clients.DSharp.Operations
{
	public class DiscordOperationBroker
	{
		private readonly DSharpClient client;


		public DiscordOperationBroker(DSharpClient client)
		{
			this.client = client;
		}


		public async Task<TResult> OperateAsync<TResult>(DiscordOperation<TResult> operation, DiscordOperationEffector<TResult> effector, ServerObject serverObject, DiscordOperationConfiguration configuration)
		{
			try
			{
				if (serverObject.BaseServer.Status.IsAfter(ServerStatus.PerformTermination))
					throw new InvalidServerStatusException(
						message: "Discord operations are disallowed",
						condition: new(ServerStatus.PerformTermination, InvalidServerStatusException.Direction.After, Inclusive: true),
						server: serverObject.BaseServer
					);

				if (serverObject.IsExists == false)
					throw new DiscordObjectNotFoundException(serverObject.GetType().Name, serverObject.Id, serverObject.Name);

				int retryCount = 0;
			retry:
				await client.AwaitConnection();

				TResult result;

				try
				{
					result = await operation();
				}
				catch (Exception? ex)
				{
					ex = ex is AggregateException aex ? aex.InnerException : ex;

					if (ex is NotFoundException notFoundEx)
					{
						if (configuration.EntityNotFoundCode == notFoundEx.EjectEntityCode())
							await serverObject.NotifyRepositoryThatDeletedAsync();
						
						throw new DiscordObjectNotFoundException(serverObject.GetType().Name, serverObject.Id, serverObject.Name);
					}

					if (ex is UnauthorizedException unauthorizedException)
					{
						throw new NotEnoughPermissionsException("Bot don't have permission to do this discord operation", unauthorizedException);
					}

					if (retryCount == 5)
						throw new InternalDiscordException("Discord operation failed", ex);

					retryCount++;

#pragma warning disable S907 // "goto" statement should not be used
					goto retry;
#pragma warning restore S907
				}

				await effector(result);

				return result;
			}
			catch (Exception ex)
			{
				throw new DiscordOperationException("Discord operation failed!", ex);
			}
		}
	}
}

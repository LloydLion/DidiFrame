using DidiFrame.Clients.DSharp;
using DidiFrame.Exceptions;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace DidiFrame.Clients.DSharp.ClientUtils
{
	/// <summary>
	/// Extensions for safe client operations
	/// </summary>
	public static class SafeOperationsExtensions
	{
		private const int RatelimitTimeoutInMilliseconds = 250;
		private const int SafeExceptionTimeoutInMilliseconds = 250;
		private readonly static EventId SafeOperationErrorID = new(23, "SafeOperationError");
		private readonly static EventId SafeOperationCriticalErrorID = new(22, "SafeOperationCriticalError");


#pragma warning disable S907 //goto statement use
		/// <summary>
		/// Do safe opration under discord client
		/// </summary>
		/// <param name="client">Target client</param>
		/// <param name="operation">Operation delegate</param>
		/// <param name="nfi">Not found info for object not found case</param>
		public static void DoSafeOperation(this DSharpClient client, Action operation, NotFoundInfo? nfi = null)
		{
		reset:
			client.CheckAndAwaitConnectionAsync().Wait();

			try
			{
				operation();
			}
			catch (Exception ex)
			{
				client.ProcessOperationException(ex, nfi);
				goto reset;
			}
		}

		/// <summary>
		/// Do safe opration under discord client with result
		/// </summary>
		/// <typeparam name="TReturn">Type of result</typeparam>
		/// <param name="client">Target client</param>
		/// <param name="operation">Operation delegate</param>
		/// <param name="nfi">Not found info for object not found case</param>
		/// <returns>Operation result</returns>
		public static TReturn DoSafeOperation<TReturn>(this DSharpClient client, Func<TReturn> operation, NotFoundInfo? nfi = null)
		{
		reset:
			client.CheckAndAwaitConnectionAsync().Wait();

			try
			{
				return operation();
			}
			catch (Exception ex)
			{
				client.ProcessOperationException(ex, nfi);
				goto reset;
			}
		}

		/// <summary>
		/// Do safe async opration under discord client
		/// </summary>
		/// <param name="client">Target client</param>
		/// <param name="operation">Async operation delegate</param>
		/// <param name="nfi">Not found info for object not found case</param>
		/// <returns>Wait task</returns>
		public static async Task DoSafeOperationAsync(this DSharpClient client, Func<Task> operation, NotFoundInfo? nfi = null)
		{
		reset:
			await client.CheckAndAwaitConnectionAsync();

			try
			{
				await operation();
			}
			catch (Exception ex)
			{
				client.ProcessOperationException(ex, nfi);
				goto reset;
			}
		}

		/// <summary>
		/// Do safe async opration under discord client with result
		/// </summary>
		/// <typeparam name="TReturn">Type of result</typeparam>
		/// <param name="client">Target client</param>
		/// <param name="operation">Async operation delegate</param>
		/// <param name="nfi">Not found info for object not found case</param>
		/// <returns>Async operation result</returns>
		public static async Task<TReturn> DoSafeOperationAsync<TReturn>(this DSharpClient client, Func<Task<TReturn>> operation, NotFoundInfo? nfi = null)
		{
		reset:
			await client.CheckAndAwaitConnectionAsync();

			try
			{
				return await operation();
			}
			catch (Exception ex)
			{
				client.ProcessOperationException(ex, nfi);
				goto reset;
			}
		}
#pragma warning restore S907 //goto statement use

		/* 
		 * Contract:
		 * AggregateException in @ex will be unpacked automaticlly
		 * 
		 * If @ex is safe exception - method will do nothing, you should restart operation
		 * If @ex is critical exception - method will throw wrapped exception (using @nfi if NotFoundException)
		 * | If @nfi is null: NotFoundException will be processed as any other exception
		 */
		private static void ProcessOperationException(this DSharpClient client, Exception ex, NotFoundInfo? nfi = null)
		{
			var pex = ex;
			if (ex is AggregateException ar && ar.InnerException is not null) pex = ar.InnerException;

			if (pex is ServerErrorException || pex is RateLimitException) //Safe exception: restart operation
			{
				client.Logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
				if (pex is RateLimitException) Thread.Sleep(RatelimitTimeoutInMilliseconds);
				else Thread.Sleep(SafeExceptionTimeoutInMilliseconds);
			}
			else //Critical exception: drop operation and throw error
			{
				client.Logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");

				if (nfi.HasValue && pex is NotFoundException)
					throw new DiscordObjectNotFoundException(nfi.Value.ObjectType.ToString(), nfi.Value.ObjectId, nfi.Value.ObjectName);
				else if (pex is UnauthorizedException) throw new NotEnoughPermissionsException(pex.Message);
				else throw new InternalDiscordException(pex.Message, pex);
			}
		}


		/// <summary>
		/// Error info if some object is not found
		/// </summary>
		public struct NotFoundInfo
		{
			/// <summary>
			/// Creates new instance of NFI
			/// </summary>
			/// <param name="objectType">Type of finding object</param>
			/// <param name="objectId">Id of object</param>
			/// <param name="objectName">Optional object name</param>
			public NotFoundInfo(Type objectType, ulong objectId, string? objectName = null)
			{
				ObjectType = objectType;
				ObjectId = objectId;
				ObjectName = objectName;
			}


			/// <summary>
			/// Type of finding object
			/// </summary>
			public Type ObjectType { get; }

			/// <summary>
			/// Optional object name
			/// </summary>
			public string? ObjectName { get; }

			/// <summary>
			/// Id of object
			/// </summary>
			public ulong ObjectId { get; }


			/// <summary>
			/// Type of finding object
			/// </summary>
			public enum Type
			{
				/// <summary>
				/// Channel type
				/// </summary>
				Channel,
				/// <summary>
				/// Member type
				/// </summary>
				Member,
				/// <summary>
				/// Message type
				/// </summary>
				Message,
				/// <summary>
				/// User type
				/// </summary>
				User,
				/// <summary>
				/// Server type
				/// </summary>
				Server,
				/// <summary>
				/// Role type
				/// </summary>
				Role
			}
		}
	}
}

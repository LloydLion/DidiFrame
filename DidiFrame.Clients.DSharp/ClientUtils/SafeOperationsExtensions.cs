using DidiFrame.Client.DSharp;
using DidiFrame.Exceptions;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Clients.DSharp.ClientUtils
{
	public static class SafeOperationsExtensions
	{
		private readonly static EventId SafeOperationErrorID = new(23, "SafeOperationError");
		private readonly static EventId SafeOperationCriticalErrorID = new(22, "SafeOperationCriticalError");


#pragma warning disable S907 //goto statement use
		///// <summary>
		///// Do safe opration under discord client
		///// </summary>
		///// <param name="operation">Operation delegate</param>
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
				var pex = ex;
				if (ex is AggregateException ar && ar.InnerException is not null) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					client.Logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					else Thread.Sleep(1000);
					goto reset;
				}
				else
				{
					client.Logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");

					if (nfi.HasValue && pex is NotFoundException)
						throw new DiscordObjectNotFoundException(nfi.Value.ObjectType.ToString(), nfi.Value.ObjectId, nfi.Value.ObjectName);
					else if (pex is UnauthorizedException) throw new NotEnoughPermissionsException(pex.Message);
					else throw new InternalDiscordException(pex.Message, pex);
				}
			}
		}

		///// <summary>
		///// Do safe opration under discord client with result
		///// </summary>
		///// <typeparam name="TReturn">Type of result</typeparam>
		///// <param name="operation">Operation delegate</param>
		///// <returns>Operation result</returns>
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
				var pex = ex;
				if (ex is AggregateException ar && ar.InnerException is not null) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					client.Logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					else Thread.Sleep(1000);
					goto reset;
				}
				else
				{
					client.Logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");

					if (nfi.HasValue && pex is NotFoundException)
						throw new DiscordObjectNotFoundException(nfi.Value.ObjectType.ToString(), nfi.Value.ObjectId, nfi.Value.ObjectName);
					else if (pex is UnauthorizedException) throw new NotEnoughPermissionsException(pex.Message);
					else throw new InternalDiscordException(pex.Message, pex);
				}
			}
		}

		///// <summary>
		///// Do safe async opration under discord client
		///// </summary>
		///// <param name="operation">Async operation delegate</param>
		///// <returns>Wait task</returns>
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
				var pex = ex;
				if (ex is AggregateException ar && ar.InnerException is not null) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					client.Logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					else Thread.Sleep(1000);
					goto reset;
				}
				else
				{
					client.Logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");

					if (nfi.HasValue && pex is NotFoundException)
						throw new DiscordObjectNotFoundException(nfi.Value.ObjectType.ToString(), nfi.Value.ObjectId, nfi.Value.ObjectName);
					else if (pex is UnauthorizedException) throw new NotEnoughPermissionsException(pex.Message);
					else throw new InternalDiscordException(pex.Message, pex);
				}
			}
		}

		///// <summary>
		///// Do safe async opration under discord client with result
		///// </summary>
		///// <typeparam name="TReturn">Type of result</typeparam>
		///// <param name="operation">Async operation delegate</param>
		///// <returns>Async operation result</returns>
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
				var pex = ex;
				if (ex is AggregateException ar && ar.InnerException is not null) pex = ar.InnerException;

				if (pex is ServerErrorException || pex is RateLimitException)
				{
					client.Logger.Log(LogLevel.Warning, SafeOperationErrorID, ex, "Safe exception in safe operation. All OK!");
					if (pex is RateLimitException) Thread.Sleep(250);
					else Thread.Sleep(1000);
					goto reset;
				}
				else
				{
					client.Logger.Log(LogLevel.Error, SafeOperationCriticalErrorID, ex, "Critical exception in safe operation");

					if (nfi.HasValue && pex is NotFoundException)
						throw new DiscordObjectNotFoundException(nfi.Value.ObjectType.ToString(), nfi.Value.ObjectId, nfi.Value.ObjectName);
					else if (pex is UnauthorizedException) throw new NotEnoughPermissionsException(pex.Message);
					else throw new InternalDiscordException(pex.Message, pex);
				}
			}
		}
#pragma warning restore S907 //goto statement use


		public struct NotFoundInfo
		{
			public NotFoundInfo(Type objectType, ulong objectId, string? objectName = null)
			{
				ObjectType = objectType;
				ObjectId = objectId;
				ObjectName = objectName;
			}


			public Type ObjectType { get; }

			public string? ObjectName { get; }

			public ulong ObjectId { get; }


			public enum Type
			{
				Channel,
				Member,
				Message,
				User,
				Server,
				Role
			}
		}
	}
}

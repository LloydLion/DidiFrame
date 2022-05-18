using DidiFrame.Culture;
using DidiFrame.UserCommands.Pipeline;
using DidiFrame.Utils;
using System.Collections.Concurrent;

namespace DidiFrame.UserCommands.Executing
{
	public class DefaultUserCommandsExecutor : AbstractUserCommandPipelineMiddleware<ValidatedUserCommandContext, UserCommandResult>, IDisposable
	{
		private static readonly EventId CommandStartID = new (32, "CommandStart");
		private static readonly EventId CommandCompliteID = new (33, "CommandComplite");
		private static readonly EventId InternalErrorID = new(36, "InternalError");


		private readonly Options options;
		private readonly ILogger<DefaultUserCommandsExecutor> logger;
		private readonly IServerCultureProvider cultureProvider;
		private readonly ThreadLocker<IMember> memberLocker = new();
		private readonly Dictionary<IServer, ThreadExecutionUnit> executionThreads = new();
		private bool abortThreads = false;


		public DefaultUserCommandsExecutor(IOptions<Options> options, ILogger<DefaultUserCommandsExecutor> logger, IServerCultureProvider cultureProvider)
		{
			this.options = options.Value;
			this.logger = logger;
			this.cultureProvider = cultureProvider;
		}


		public void Dispose()
		{
			GC.SuppressFinalize(this);
			abortThreads = true;
			foreach (var thread in executionThreads.Values) thread.Thread.Join();
		}

		public override UserCommandResult? Process(ValidatedUserCommandContext ctx, UserCommandPipelineContext pipelineContext)
		{
			try
			{
				using (memberLocker.Lock(ctx.Invoker))
				{
					UserCommandResult result;

					using (logger.BeginScope("Command: {CommandName}", ctx.Command.Name))
					{
						cultureProvider.SetupCulture(ctx.Invoker.Server);

						logger.Log(LogLevel.Debug, CommandStartID, "Command executing started");

						try
						{
							var synCtx = new CommandSynchronizationContext(GetExecutionUnitFor(ctx.Channel.Server));

							Task<UserCommandResult>? task = null;

							synCtx.Send((o) =>
							{
								task = ctx.Command.Handler(ctx);
							}, null);

							if (task is null) throw new ImpossibleVariantException();

							result = task.Result;
						}
						catch (Exception ex)
						{
							result = new UserCommandResult(UserCommandCode.UnspecifiedError)
							{
								RespondMessage = createExcetionMessage(ex)
							};
						}

						logger.Log(LogLevel.Debug, CommandCompliteID, "Command executed with code {ResultCode}", result.Code);

						return result;
					}


					MessageSendModel? createExcetionMessage(Exception ex)
					{
						string text;
						switch (options.UnspecifiedErrorMessage)
						{
							case Options.UnspecifiedErrorMessageBehavior.Disable:
								return null;
							case Options.UnspecifiedErrorMessageBehavior.EnableWithoutDebugInfo:
								text = "Command excecution finished with error\nCode: " + nameof(UserCommandCode.UnspecifiedError);
								break;
							case Options.UnspecifiedErrorMessageBehavior.EnableWithExceptionsTypeAndMessage:
								text = "Command excecution finished with error\n" +
										$"Error: {ex}\n" +
										"Code: " + nameof(UserCommandCode.UnspecifiedError);
								break;
							case Options.UnspecifiedErrorMessageBehavior.EnableWithFullExceptionInfo:
								text = "Command excecution finished with error\n" +
										$"Error: {ex}\n" +
										$"Stack: {ex.StackTrace}" +
										$"InnerException: {ex.InnerException?.ToString() ?? "No inner exception"}\n" +
										$"InnerExceptionStack: {ex.InnerException?.StackTrace ?? "No inner exception"}\n" +
										"Code: " + nameof(UserCommandCode.UnspecifiedError);
								break;
							default: throw new Exception(); //Never be
						}

						return new MessageSendModel(text);
					}
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, InternalErrorID, ex, "Exception while handling the command (Internal)");
				pipelineContext.DropPipeline();
				return null;
			}
		}

		private ThreadExecutionUnit GetExecutionUnitFor(IServer server)
		{
			if (!executionThreads.ContainsKey(server))
			{
				var queue = new ConcurrentQueue<ExecutionUnitTask>();
				var thread = new Thread(() =>
				{
					while (!abortThreads)
					{
						while (queue.TryDequeue(out var task))
						{
							SynchronizationContext.SetSynchronizationContext(task.Caller);
							task.Action(task.State);
							task.OnCompletedCallback?.Invoke();
						}

						Thread.Sleep(50);
					}
				});

				executionThreads.Add(server, new ThreadExecutionUnit(thread, queue));
			}

			return executionThreads[server];
		}


		public class Options
		{
			public UnspecifiedErrorMessageBehavior UnspecifiedErrorMessage { get; set; } = UnspecifiedErrorMessageBehavior.EnableWithExceptionsTypeAndMessage;


			public enum UnspecifiedErrorMessageBehavior
			{
				Disable,
				EnableWithoutDebugInfo,
				EnableWithExceptionsTypeAndMessage,
				EnableWithFullExceptionInfo
			}
		}
	}
}

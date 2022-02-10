using CGZBot3.UserCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.UserCommands
{
	internal class UserCommandsHandler : IUserCommandsHandler
	{
		private readonly Options options;
		private readonly IValidator<UserCommandContext> ctxVal;


		public UserCommandsHandler(IOptions<Options> options, IValidator<UserCommandContext> ctxVal)
		{
			this.options = options.Value;
			this.ctxVal = ctxVal;
		}


		public async Task HandleAsync(UserCommandContext ctx)
		{
			UserCommandResult result;

			ctxVal.ValidateAndThrow(ctx);

			try
			{
				result = await ctx.Command.Handler.Invoke(ctx);
			}
			catch(Exception ex)
			{
				result = new UserCommandResult(UserCommandCode.UnspecifiedError)
				{
					RespondMessage = createExcetionMessage(ex)
				};
			}

			if(result.RespondMessage != null)
			{
				await ctx.Channel.SendMessageAsync(result.RespondMessage);
			}

			//TEMP!!! Change to logger!!!
			Console.WriteLine($"Command {ctx.Command.Name} executed with code {result.Code}");

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
						text =	"Command excecution finished with error\n" +
								$"Error: {ex}\n" +
								"Code: " + nameof(UserCommandCode.UnspecifiedError);
						break;
					case Options.UnspecifiedErrorMessageBehavior.EnableWithFullExceptionInfo:
						text =	"Command excecution finished with error\n" +
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


		public class Options
		{
			public UnspecifiedErrorMessageBehavior UnspecifiedErrorMessage { get; } = UnspecifiedErrorMessageBehavior.EnableWithExceptionsTypeAndMessage;


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

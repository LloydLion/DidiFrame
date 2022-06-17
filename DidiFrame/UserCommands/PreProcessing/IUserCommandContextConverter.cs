using DidiFrame.UserCommands.Pipeline;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// Middleware that converts raw context with primitive arguments to ready-to-use context
	/// </summary>
	public interface IUserCommandContextConverter : IUserCommandPipelineMiddleware<UserCommandPreContext, UserCommandContext>
	{
		public IUserCommandContextSubConverter GetSubConverter(Type type);

		public IUserCommandContextSubConverter GetSubConverter(IReadOnlyList<UserCommandArgument.Type> inputs);
	}
}
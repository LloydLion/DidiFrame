using DidiFrame.UserCommands.Pipeline;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.UserCommands.PreProcessing
{
	/// <summary>
	/// Middleware that converts raw context with primitive arguments to ready-to-use context
	/// </summary>
	public interface IUserCommandContextConverter : IUserCommandPipelineMiddleware<UserCommandPreContext, UserCommandContext>
	{
		/// <summary>
		/// Gets subconverter by target type
		/// </summary>
		/// <param name="type">Target type of subconverter</param>
		/// <returns>Instance of subconverter</returns>
		public IUserCommandContextSubConverter GetSubConverter(Type type);

		/// <summary>
		/// Gets subconverter by target preobjects types
		/// </summary>
		/// <param name="inputs">Target preobjects types</param>
		/// <returns>Instance of subconverter</returns>
		public IUserCommandContextSubConverter GetSubConverter(IReadOnlyList<UserCommandArgument.Type> inputs);
	}
}
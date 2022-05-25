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
        /// Returns true and value only if complex type has only one set of pretypes
        /// </summary>
        /// <param name="complexType">Complex type for search</param>
        /// <param name="possiblePreObjectTypes">Pretypes of complex type</param>
        /// <returns>If has only one set</returns>
        public bool TryGetPreObjectTypes(Type complexType, [NotNullWhen(true)] out IReadOnlyList<UserCommandArgument.Type>? possiblePreObjectTypes);
    }
}
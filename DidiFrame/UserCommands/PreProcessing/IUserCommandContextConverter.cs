using DidiFrame.UserCommands.Pipeline;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.UserCommands.PreProcessing
{
    public interface IUserCommandContextConverter : IUserCommandPipelineMiddleware<UserCommandPreContext, UserCommandContext>
    {
        /// <summary>
        /// Return true and value only if complex type has single pretype
        /// </summary>
        /// <param name="complexType"></param>
        /// <param name="possiblePreObjectType"></param>
        /// <returns></returns>
        public bool TryGetPreObjectTypes(Type complexType, [NotNullWhen(true)] out IReadOnlyList<UserCommandArgument.Type>? possiblePreObjectTypes);
    }
}
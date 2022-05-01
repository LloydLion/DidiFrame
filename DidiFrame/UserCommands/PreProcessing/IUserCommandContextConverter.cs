using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.UserCommands.PreProcessing
{
    public interface IUserCommandContextConverter
    {
        public UserCommandContext Convert(UserCommandPreContext preCtx);

        /// <summary>
        /// Return true and value only if complex type has single pretype
        /// </summary>
        /// <param name="complexType"></param>
        /// <param name="possiblePreObjectType"></param>
        /// <returns></returns>
        public bool TryGetPreObjectTypes(Type complexType, [NotNullWhen(true)] out IReadOnlyList<UserCommandArgument.Type>? possiblePreObjectTypes);
    }
}
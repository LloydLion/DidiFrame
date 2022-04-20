using System.Diagnostics.CodeAnalysis;

namespace CGZBot3.UserCommands
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
        public bool TryGetPreObjectTypes(Type complexType, [NotNullWhen(true)] out IReadOnlyList<UserCommandInfo.Argument.Type>? possiblePreObjectTypes);
    }
}
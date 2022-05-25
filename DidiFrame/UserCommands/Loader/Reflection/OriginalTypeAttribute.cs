namespace DidiFrame.UserCommands.Loader.Reflection
{
    /// <summary>
    /// Attribute for complex-type arguments that determines from which types need to convert it
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class OriginalTypeAttribute : Attribute
    {
        /// <summary>
        /// Original types as tuple type
        /// </summary>
        /// <param name="originalType">Tuple type</param>
        public OriginalTypeAttribute(Type originalType)
        {
            OriginalType = originalType;
        }


        /// <summary>
        /// Given tuple type
        /// </summary>
        public Type OriginalType { get; }
    }
}

namespace DidiFrame.UserCommands.Loader.Reflection
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class OriginalTypeAttribute : Attribute
    {
        public OriginalTypeAttribute(Type originalType)
        {
            OriginalType = originalType;
        }


        public Type OriginalType { get; }
    }
}

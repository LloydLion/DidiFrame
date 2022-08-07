namespace DidiFrame.Dependencies
{
	/// <summary>
	/// Marks constructor parameter as dependency
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class DependencyAttribute : Attribute
	{

	}
}

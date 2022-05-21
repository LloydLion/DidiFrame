namespace DidiFrame.Exceptions
{
	/// <summary>
	/// Plug exception that indicates impossible situation. It is useful in nullable context
	/// </summary>
	public class ImpossibleVariantException : Exception
	{
		public ImpossibleVariantException() : base("It is exception can't be thrown. If you see it please contact with developers") { }
	}
}

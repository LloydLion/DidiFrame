using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Exceptions
{
	/// <summary>
	/// Plug exception that indicates impossible situation. It is useful in nullable context
	/// </summary>
	public sealed class ImpossibleVariantException : Exception
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Exceptions.ImpossibleVariantException
		/// </summary>
		public ImpossibleVariantException() : base("It is exception can't be thrown. If you see it please contact with developers") { }
	}
}

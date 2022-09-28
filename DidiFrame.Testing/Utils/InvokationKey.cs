namespace DidiFrame.Testing.Utils
{
	/// <summary>
	/// Objects that count of invokes its set method
	/// </summary>
	public class InvokationKey
	{
		/// <summary>
		/// Count of invokes its set method
		/// </summary>
		public int SetCount { get; private set; }


		/// <summary>
		/// Calls set method
		/// </summary>
		public void Set()
		{
			SetCount++;
		}
	}
}

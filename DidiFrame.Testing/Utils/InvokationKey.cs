namespace DidiFrame.Testing.Utils
{
	public class InvokationKey
	{
		public int SetCount { get; private set; }


		public void Set()
		{
			SetCount++;
		}
	}
}

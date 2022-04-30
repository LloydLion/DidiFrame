namespace DidiFrame.Utils.Dialogs
{
	public class InterMessageLink
	{
		private readonly Dictionary<string, object> globalVars = new();
		private Dictionary<string, object> newChainedVars = new();
		private Dictionary<string, object> oldChainedVars = new();


		public void SetGlobalVariable(string key, object value)
		{
			//Every global var is readonly
			if (globalVars.ContainsKey(key))
				throw new InvalidOperationException("Any global variable is readonly");

			globalVars.Add(key, value);
		}

		public TValue GetGlobalVariable<TValue>(string key)
		{
			return (TValue)globalVars[key];
		}

		public TValue GetChainedVariable<TValue>(string key)
		{
			return (TValue)oldChainedVars[key];
		}

		public void SetChainedVariable(string key, object value)
		{
			//Every global var is readonly
			if (newChainedVars.ContainsKey(key))
				throw new InvalidOperationException("Any chained variable is readonly");

			newChainedVars.Add(key, value);
		}

		internal void SwapBuffers()
		{
			oldChainedVars = newChainedVars;
			newChainedVars = new();
		}
	}
}

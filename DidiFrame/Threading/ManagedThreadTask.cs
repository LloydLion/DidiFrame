namespace DidiFrame.Threading
{
	public delegate void ManagedThreadTask();


	public static class ManagedThreadTaskExtensions
	{
		public static string AsLogString(this ManagedThreadTask task)
		{
			return $"[{task.Method}] on {{{task.Target}}} (hash {task.Target?.GetHashCode()})";
		}
	}
}

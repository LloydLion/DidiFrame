using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Contains text threads for other channels
	/// </summary>
	public class ThreadsContainer
	{
		private readonly List<TextThread> threads = new();


		/// <summary>
		/// Provides full list of threads
		/// </summary>
		/// <returns>Collection of stored threads</returns>
		public IReadOnlyCollection<ITextThread> GetThreads() => threads;

		internal void AddThreadInternal(TextThread thread) => threads.Add(thread);

		internal void DeleteThreadInternal(TextThread thread) => threads.Remove(thread);
	}
}

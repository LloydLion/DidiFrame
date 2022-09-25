using DidiFrame.Clients;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test ITextChannel implementation
	/// </summary>
	public class TextChannel : TextChannelBase, ITextChannel
	{
		private readonly List<TextThread> threads = new();


		internal TextChannel(string name, ChannelCategory category) : base(name, category)
		{

		}


		/// <inheritdoc/>
		public IReadOnlyCollection<ITextThread> GetThreads() => GetIfExist(threads);

		internal void AddThreadInternal(TextThread thread) => GetIfExist(threads).Add(thread);

		internal void DeleteThreadInternal(TextThread thread) => GetIfExist(threads).Remove(thread);
	}
}

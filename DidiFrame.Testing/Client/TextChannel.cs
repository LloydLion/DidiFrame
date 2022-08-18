using DidiFrame.Client;

namespace DidiFrame.Testing.Client
{
	public class TextChannel : TextChannelBase, ITextChannel
	{
		private readonly List<TextThread> threads = new();


		public TextChannel(string name, ChannelCategory category) : base(name, category)
		{

		}


		public IReadOnlyCollection<ITextThread> GetThreads() => GetIfExist(threads);

		internal void AddThreadInternal(TextThread thread) => GetIfExist(threads).Add(thread);

		internal void DeleteThreadInternal(TextThread thread) => GetIfExist(threads).Remove(thread);
	}
}

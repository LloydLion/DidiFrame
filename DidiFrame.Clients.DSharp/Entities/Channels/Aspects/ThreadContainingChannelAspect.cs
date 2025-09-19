using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Utils.RoutedEvents;

namespace DidiFrame.Clients.DSharp.Entities.Channels.Aspects
{
	public class ThreadContainingChannelAspect<TThread> : ChannelAspect<IDSharpThreadContainingChannel<TThread>, ThreadContainingChannelAspect<TThread>.IProtectedContract> where TThread : IThreadChannel
	{
		public ThreadContainingChannelAspect(
			IDSharpThreadContainingChannel<TThread> inheritor,
			ThreadContainingChannelAspect<TThread>.IProtectedContract contract,
			ChannelRepository repository) : base(inheritor, contract, repository)
		{ }


		public IReadOnlyCollection<TThread> ListThreads()
		{
			return Repository.GetThreadsFor<TThread>(Inheritor);
		}


		public interface IProtectedContract
		{
			
		}
	}


	public interface IDSharpThreadContainingChannel<out TThread> : IDSharpChannel, IThreadContainingChannel<TThread> where TThread : IThreadChannel
	{
		internal RoutedEventTreeNode CreateEventTreeNode(IServerObject serverObject);
	}
}

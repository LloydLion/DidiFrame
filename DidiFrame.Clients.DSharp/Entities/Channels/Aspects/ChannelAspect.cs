using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;

namespace DidiFrame.Clients.DSharp.Entities.Channels.Aspects
{
	public class ChannelAspect<TInheritor, TContract> where TInheritor : class, IDSharpChannel where TContract : class
	{
		private readonly TInheritor inheritor;
		private readonly TContract contract;
		private readonly ChannelRepository repository;


		public ChannelAspect(TInheritor inheritor, TContract contract, ChannelRepository repository)
		{
			this.inheritor = inheritor;
			this.contract = contract;
			this.repository = repository;
		}


		protected TInheritor Inheritor => inheritor;

		protected TContract Contract => contract;

		protected ChannelRepository Repository => repository;
	}
}

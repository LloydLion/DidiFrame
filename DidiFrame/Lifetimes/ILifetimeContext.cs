using DidiFrame.Utils;

namespace DidiFrame.Lifetimes
{
	public interface ILifetimeContext<TBase> where TBase : class, ILifetimeBase
	{
		public bool IsNewlyCreated { get; }

		public IObjectController<TBase> AccessBase();

		public void FinalizeLifetime(Exception? ifFailed = null);
	}
}

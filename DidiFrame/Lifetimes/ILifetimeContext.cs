namespace DidiFrame.Lifetimes
{
	public interface ILifetimeContext<TBase> where TBase : ILifetimeBase
	{
		public bool IsNewlyCreated { get; }

		public IDisposable AccessBase(out TBase baseObject);

		public void FinalizeLifetime(Exception? ifFailed = null);
	}
}

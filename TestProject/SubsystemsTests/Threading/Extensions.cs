using DidiFrame.Threading;
using System;

namespace TestProject.SubsystemsTests.Threading
{
	public static class Extensions
	{
		public static IDisposable CreateTestDisposable(this IManagedThread thread, out IManagedThread outThread)
		{
			outThread = thread;
			return new TestDisposable(thread);
		}


		private class TestDisposable : IDisposable
		{
			private readonly IManagedThread thread;


			public TestDisposable(IManagedThread thread)
			{
				this.thread = thread;
			}


			public void Dispose()
			{
				thread.GetActiveQueue().Dispatch(thread.Stop);
			}
		}
	}
}

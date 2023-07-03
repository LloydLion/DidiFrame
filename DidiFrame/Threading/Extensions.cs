namespace DidiFrame.Threading
{
	public static class Extensions
	{
		public static ManagedQueueBasedSynchronizationContext CreateSynchronizationContext(this IManagedThreadExecutionQueue queue)
		{
			return new ManagedQueueBasedSynchronizationContext(queue);
		}

		public static void SetAsSynchronizationContext(this IManagedThreadExecutionQueue queue)
		{
			SynchronizationContext.SetSynchronizationContext(queue.CreateSynchronizationContext());
		}

		public static Task DispatchAsync(this IManagedThreadExecutionQueue queue, Action action)
		{
			var tcs = new TaskCompletionSource();

			queue.Dispatch(() =>
			{
				try
				{
					action();
					tcs.SetResult();
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					throw;
				}
			});

			return tcs.Task;
		}

		public static Task DispatchAsync(this IManagedThreadExecutionQueue queue, Func<ValueTask> asyncAction)
		{
			var tcs = new TaskCompletionSource();

			queue.Dispatch(async () =>
			{
				try
				{
					await asyncAction();
					tcs.SetResult();
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					throw;
				}
			});

			return tcs.Task;
		}

		public static Task<TResult> DispatchAsync<TResult>(this IManagedThreadExecutionQueue queue, Func<TResult> action)
		{
			var tcs = new TaskCompletionSource<TResult>();

			queue.Dispatch(() =>
			{
				try
				{
					var result = action();
					tcs.SetResult(result);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					throw;
				}
			});

			return tcs.Task;
		}

		public static Task<TResult> DispatchAsync<TResult>(this IManagedThreadExecutionQueue queue, Func<ValueTask<TResult>> asyncAction)
		{
			var tcs = new TaskCompletionSource<TResult>();

			queue.Dispatch(async () =>
			{
				try
				{
					var result = await asyncAction();
					tcs.SetResult(result);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
					throw;
				}
			});

			return tcs.Task;
		}

		public static Task DispatchAsyncIgnore(this IManagedThreadExecutionQueue queue, Action action)
		{
			var tcs = new TaskCompletionSource();

			queue.Dispatch(() =>
			{
				try
				{
					action();
				}
				finally
				{
					tcs.SetResult();
				}
			});

			return tcs.Task;
		}

		public static Task DispatchAsyncIgnore(this IManagedThreadExecutionQueue queue, Func<ValueTask> asyncAction)
		{
			var tcs = new TaskCompletionSource();

			queue.Dispatch(async () =>
			{
				try
				{
					await asyncAction();
				}
				finally
				{
					tcs.SetResult();
				}
			});

			return tcs.Task;
		}
	}
}

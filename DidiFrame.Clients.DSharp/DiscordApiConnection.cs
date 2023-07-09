using DSharpPlus;

namespace DidiFrame.Clients.DSharp
{
	public class DiscordApiConnection
	{
		private readonly DiscordClient client;


		public DiscordApiConnection(DiscordClient client)
		{
			this.client = client;
		}


		public ConnectStatus CurrentConnectStatus { get; private set; }


		public async ValueTask ConnectAsync()
		{
			if (CurrentConnectStatus != ConnectStatus.NoConnection)
				throw new InvalidOperationException("Enable to connect twice");

			await client.ConnectAsync();

			await Task.Delay(5000);

			CurrentConnectStatus = ConnectStatus.Connected;
		}

		public async Task AwaitForExit()
		{
			ThrowUnlessConnected();

			int ticks = 0;
			while (ticks < 5)
			{
				await Task.Delay(new TimeSpan(0, 1, 0));

				try
				{
					//Demo operation
					await client.GetUserAsync(client.CurrentUser.Id, updateCache: true);
					ticks = 0;
				}
				catch (Exception)
				{
					ticks++;
				}
			}

			CurrentConnectStatus = ConnectStatus.Disconnected;
		}

		public async Task AwaitConnection()
		{
			while (true)
			{
				try
				{
					await client.GetUserAsync(client.CurrentUser.Id, updateCache: true);
					return;
				}
				catch (Exception)
				{
					if (CurrentConnectStatus == ConnectStatus.Disconnected)
						throw new Exception("Client disconnected, connection will not be fixed, dropping connection await operation.");


					await Task.Delay(new TimeSpan(0, 0, 30));
				}
			}
		}

		public void ThrowUnlessConnected()
		{
			if (CurrentConnectStatus != ConnectStatus.Connected)
				throw new InvalidOperationException("Enable to do this operation if client hasn't connected");
		}


		/// <summary>
		/// Represents client state
		/// </summary>
		public enum ConnectStatus
		{
			/// <summary>
			/// New client, no connection to server
			/// </summary>
			NoConnection,
			/// <summary>
			/// Connected to discord server
			/// </summary>
			Connected,
			/// <summary>
			/// Session closed, client closed
			/// </summary>
			Disconnected,
		}
	}
}

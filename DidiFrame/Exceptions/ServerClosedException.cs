using DidiFrame.Clients;
using System;

namespace DidiFrame.Exceptions
{
	public class ServerClosedException : Exception
	{
		public ServerClosedException(string message, IServer server) : base($"Target server ({server.Id}) is closed." + message) { }
	}
}

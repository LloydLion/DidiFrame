namespace DidiFrame.Exceptions
{
	public class InvalidServerStatusException : Exception
	{
		public InvalidServerStatusException(string message, StatusCondition condition, IServer server)
			: base($"Target server ({server.Id}) has invalid status ({server.Status}), valid is {condition}. " + message)
		{ }


		public record struct StatusCondition(ServerStatus Status, Direction Direction, bool Inclusive)
		{
			public override string ToString()
			{
				return $"{Direction} {Status} [inclusive: {Inclusive}]";
			}
		}

		public enum Direction
		{
			Before,
			After
		}
	}
}

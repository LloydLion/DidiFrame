﻿namespace DidiFrame.Interfaces
{
	public interface IRole : IServerEntity
	{
		public Permissions Permissions { get; }

		public string Name { get; }

		public ulong Id { get; }
	}
}
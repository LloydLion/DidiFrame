﻿using CGZBot3.Interfaces;

namespace TestProject.TestAdapter
{
	internal class User : IUser
	{
		public string UserName { get; }

		public ulong Id { get; protected set; }

		public IClient Client { get; }


		public User(Client client, string userName)
		{
			Client = client;
			UserName = userName;
			Id = client.GenerateId();
		}


		public bool Equals(IUser? other) => other is User user && user.Id == Id;
	}
}
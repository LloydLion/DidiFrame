using CGZBot3.GlobalEvents;
using CGZBot3.Systems.Voice;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestProject.Environment;

namespace TestProject.SystemsTests.Voice
{
	public class Tests
	{
		[Fact]
		public void SimpleCreate()
		{
			var client = new Client();

			var server = new Server(client, "Some server");
			client.BaseServers.Add(server);

			var people = server.AddMember(new User(client, "The people"), Permissions.All);

			var gcat = server.Categories.First();
			var channel = new TextChannel("the-channel", gcat);
			gcat.BaseChannels.Add(channel);


			var repository = new CreatedVoiceChannelLifetimeRepository(channel);
			var setting = new SettingsRepository(gcat, channel);
			var culture = new CultureProvider();
			var startupEvent = new StartupEvent(client, culture);
			var systemCore = new SystemCore(repository, setting, new ChannelCreationArgsValidator(), startupEvent);

			startupEvent.InvokeStartup();

			//-------------------------------------------------------------------------
			//Act
			var lt = systemCore.CreateAsync(new VoiceChannelCreationArgs("Correct name", people)).Result;
			var voice = (VoiceChannel)lt.BaseObject.BaseChannel;

			//-------------------------------------------------------------------------
			//Assert
			Assert.Equal("Correct name", voice.Name);

			//-------------------------------------------------------------------------
			//Act
			Thread.Sleep(6000);
			voice.Connect(people);

			//-------------------------------------------------------------------------
			//Assert
			Assert.Contains(voice, gcat.Channels);

			//-------------------------------------------------------------------------
			//Act
			Thread.Sleep(11000);

			//-------------------------------------------------------------------------
			//Assert
			Assert.Contains(voice, gcat.Channels);

			//-------------------------------------------------------------------------
			//Act
			voice.Disconnect(people);
			Thread.Sleep(100);

			//-------------------------------------------------------------------------
			//Assert
			Assert.DoesNotContain(voice, gcat.Channels);
		}

		[Fact]
		public void CreateWithInvalidName()
		{
			var client = new Client();

			var server = new Server(client, "Some server");
			client.BaseServers.Add(server);

			var people = server.AddMember(new User(client, "The people"), Permissions.All);

			var gcat = server.Categories.First();
			var channel = new TextChannel("the-channel", gcat);
			gcat.BaseChannels.Add(channel);


			var repository = new CreatedVoiceChannelLifetimeRepository(channel);
			var setting = new SettingsRepository(gcat, channel);
			var culture = new CultureProvider();
			var startupEvent = new StartupEvent(client, culture);
			var systemCore = new SystemCore(repository, setting, new ChannelCreationArgsValidator(), startupEvent);

			startupEvent.InvokeStartup();

			//-------------------------------------------------------------------------
			//Act and Assert
			Assert.Throws<AggregateException>(() => systemCore.CreateAsync(new VoiceChannelCreationArgs("   ", people)).Wait());
		}

		[Fact]
		public void EnableDeleteVoiceTest()
		{
			var client = new Client();

			var server = new Server(client, "Some server");
			client.BaseServers.Add(server);

			var people = server.AddMember(new User(client, "The people"), Permissions.All);

			var gcat = server.Categories.First();
			var channel = new TextChannel("the-channel", gcat);
			gcat.BaseChannels.Add(channel);


			var repository = new CreatedVoiceChannelLifetimeRepository(channel);
			var setting = new SettingsRepository(gcat, channel);
			var culture = new CultureProvider();
			var startupEvent = new StartupEvent(client, culture);
			var systemCore = new SystemCore(repository, setting, new ChannelCreationArgsValidator(), startupEvent);

			startupEvent.InvokeStartup();

			//-------------------------------------------------------------------------
			//Act
			var lt = systemCore.CreateAsync(new VoiceChannelCreationArgs("Correct name", people)).Result;
			var voice = (VoiceChannel)lt.BaseObject.BaseChannel;
			voice.DeleteAsync().Wait();
			Thread.Sleep(12000);

			//-------------------------------------------------------------------------
			//Assert
			Assert.Empty(repository.Lifetimes);
			Assert.Empty(channel.Messages); //check report
		}
	}
}

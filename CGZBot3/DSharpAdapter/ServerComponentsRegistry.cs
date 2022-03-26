using CGZBot3.Entities.Message.Components;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class ServerComponentsRegistry
	{
		private readonly Dictionary<IComponent, DiscordComponent> components = new();
		private readonly Dictionary<string, object> menuOptions = new();
		private int nextCompId = 0;
		private int nextMenuOptId = 0;
			

		public DiscordComponent GetComponent(IComponent component)
		{
			return components[component];
		}

		public DiscordComponent AddComponent(IComponent component, Func<string, DiscordComponent> discordComponentFromId)
		{
			var id = "component-" + ++nextCompId;
			var discord = discordComponentFromId(id);
			components.Add(component, discord);
			if (discord is not DiscordLinkButtonComponent && discord.CustomId != id) throw new ArgumentException("Id of new discord component must be equals generated id which given in delegate");
			return discord;
		}

		public void DeleteComponent(IComponent component)
		{
			components.Remove(component);
		}

		public string AddMenuOptionValue(object value)
		{
			var id = "option-" + ++nextMenuOptId;
			menuOptions.Add(id, value);
			return id;
		}

		public T GetMenuOption<T>(string id)
		{
			return (T)menuOptions[id];
		}

		public void DeleteMenuOption(object value)
		{
			menuOptions.Remove(menuOptions.Single(s => s.Value == value).Key);
		}

		public void DeleteMenuOption(string id)
		{
			menuOptions.Remove(id);
		}
	}
}

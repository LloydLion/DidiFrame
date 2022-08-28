using DidiFrame.Utils.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DidiFrame.Utils.Json
{
	/// <summary>
	/// Tool to create configurated json serializers from discord servers
	/// </summary>
	public static class JsonSerializerFactory
	{
		private static readonly EventId CollectionElementParseErrorID = new(41, "CollectionElementParseError");


		/// <summary>
		/// Creates configurated json serializer for given server and callback
		/// </summary>
		/// <param name="server">Server for that need to create serializer</param>
		/// <param name="invalidCollectionElementCallback">Error callback for safe collection convertation</param>
		/// <returns>New serializer</returns>
		public static JsonSerializer CreateWithConverters(IServer server, Action<JContainer, string, Exception>? invalidCollectionElementCallback = null)
		{
			var ret = new JsonSerializer()
			{
				Formatting = Formatting.Indented,
			};

			ret.Converters.Add(new AbstractConveter());

			ret.Converters.Add(new ServerEntityConverter<IMember>(server, s => s.Id, (id, server) => server.GetMember(id)));
			ret.Converters.Add(new ServerEntityConverter<IRole>(server, s => s.Id, (id, server) => server.GetRole(id)));
			ret.Converters.Add(new ServerEntityConverter<IChannel>(server, s => s.Id, (id, server) => server.GetChannel(id)));
			ret.Converters.Add(new ServerEntityConverter<IChannelCategory>(server, s => s.Id, (id, server) => server.GetCategory(id)));
			ret.Converters.Add(new MessageConverter());
			ret.Converters.Add(new ServerConveter(server));

			ret.Converters.Add(new StringEnumConverter());

			ret.Converters.Add(new SafeCollectionConveter(invalidCollectionElementCallback));

			return ret;
		}

		/// <summary>
		/// Creates configurated json serializer for given server, callback and logger
		/// </summary>
		/// <param name="server">Server for that need to create serializer</param>
		/// <param name="invalidCollectionElementLogger">Logger for errors while collections convertations</param>
		/// <param name="invalidCollectionElementCallback">Error callback for safe collection convertation</param>
		/// <returns>New serializer</returns>
		public static JsonSerializer CreateWithConverters(IServer server, ILogger invalidCollectionElementLogger, Action<JContainer, string, Exception>? invalidCollectionElementCallback = null)
		{
			var callback = defCall;
			if (invalidCollectionElementCallback is not null)
				callback += invalidCollectionElementCallback;


			var ret = new JsonSerializer()
			{
				Formatting = Formatting.Indented,
			};

			ret.Converters.Add(new AbstractConveter());

			ret.Converters.Add(new ServerEntityConverter<IMember>(server, s => s.Id, (id, server) => server.GetMember(id)));
			ret.Converters.Add(new ServerEntityConverter<IRole>(server, s => s.Id, (id, server) => server.GetRole(id)));
			ret.Converters.Add(new ServerEntityConverter<IChannel>(server, s => s.Id, (id, server) => server.GetChannel(id)));
			ret.Converters.Add(new ServerEntityConverter<IChannelCategory>(server, s => s.Id, (id, server) => server.GetCategory(id)));
			ret.Converters.Add(new MessageConverter());
			ret.Converters.Add(new ServerConveter(server));

			ret.Converters.Add(new StringEnumConverter());

			ret.Converters.Add(new SafeCollectionConveter(callback));

			return ret;


			void defCall(JContainer rootEl, string jpath, Exception ex)
			{
				invalidCollectionElementLogger.Log(LogLevel.Warning, CollectionElementParseErrorID, ex, "Enable to convert object:\n{Json}", rootEl.SelectToken(jpath));
			}
		}
	}
}

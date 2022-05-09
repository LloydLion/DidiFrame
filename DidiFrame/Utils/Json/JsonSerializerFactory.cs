using DidiFrame.Utils.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DidiFrame.Utils.Json
{
	public static class JsonSerializerFactory
	{
		private static readonly EventId CollectionElementParseErrorID = new(41, "CollectionElementParseError");


		public static JsonSerializer CreateWithConverters(IServer server, Action<JContainer, string, Exception>? invalidCollectionElementCallback = null)
		{
			var ret = new JsonSerializer()
			{
				Formatting = Formatting.Indented,
			};

			ret.Converters.Add(new AbstractConveter());

			ret.Converters.Add(new CategoryConveter(server));
			ret.Converters.Add(new ChannelConverter(server));
			ret.Converters.Add(new MemberConveter(server));
			ret.Converters.Add(new RoleConverter(server));
			ret.Converters.Add(new MessageConverter(server));
			ret.Converters.Add(new ServerConveter(server.Client));
			ret.Converters.Add(new StringEnumConverter());

			ret.Converters.Add(new SafeCollectionConveter(invalidCollectionElementCallback ?? noCall));

			return ret;


			static void noCall(JContainer _1, string _2, Exception _3) { };
		}

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

			ret.Converters.Add(new CategoryConveter(server));
			ret.Converters.Add(new ChannelConverter(server));
			ret.Converters.Add(new MemberConveter(server));
			ret.Converters.Add(new RoleConverter(server));
			ret.Converters.Add(new MessageConverter(server));
			ret.Converters.Add(new ServerConveter(server.Client));
			ret.Converters.Add(new StringEnumConverter());

			ret.Converters.Add(new SafeCollectionConveter(callback));

			return ret;


			void defCall(JContainer rootEl, string jpath, Exception ex)
			{
				invalidCollectionElementLogger.Log(LogLevel.Warning, CollectionElementParseErrorID, ex, "Enable to convert object:\n{Json}", rootEl.SelectToken(jpath));
			};
		}
	}
}

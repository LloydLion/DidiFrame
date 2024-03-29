﻿using DidiFrame.Exceptions;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace DidiFrame.Data.Mongo
{
	internal static class Extensions
	{
		public static async IAsyncEnumerator<IEnumerable<TModel>> GetAsyncEnumerator<TModel>(this IAsyncCursor<TModel> cursor)
		{
			while(await cursor.MoveNextAsync()) yield return cursor.Current;
			cursor.Dispose();
		}

		public static string Serialize<T>(this JsonSerializer serializer, T? obj)
		{
			var strWriter = new StringWriter();
			using var json = new JsonTextWriter(strWriter);

			serializer.Serialize(json, obj);

			return strWriter.GetStringBuilder().ToString();
		}

		public static T Deserialize<T>(this JsonSerializer serializer, string str)
		{
			if (string.IsNullOrWhiteSpace(str)) str = "{}";
			var stringReader = new StringReader(str);
			using var json = new JsonTextReader(stringReader);

			return serializer.Deserialize<T>(json) ?? throw new ImpossibleVariantException();
		}

		public static JContainer GetAbsoluteRoot(this JContainer token, out string jpath)
		{
			var result = new StringBuilder();
			result.Append('.');

			while (token.Parent is not null)
			{
				result.Append(token.Path);

				token = token.Parent;
				if (token is JObject) //if jarray do nothing
					result.Append('.');
			}

			jpath = result.ToString();
			return token;
		}
	}
}

using System.Text;

namespace CGZBot3.Utils
{
	internal class FileCache
	{
		private readonly Dictionary<string, byte[]> baseCache = new();
		private readonly string basePath;
		private readonly byte[] defaultValue;
		private static readonly ThreadLocker<FileCache> globalLocker = new();


		public FileCache(string basePath, ReadOnlySpan<byte> defaultValue)
		{
			this.basePath = basePath;
			this.defaultValue = defaultValue.ToArray();
		}

		public FileCache(string basePath, string defaultValue)
		{
			this.basePath = basePath;
			this.defaultValue = Encoding.Default.GetBytes(defaultValue);
		}


		public async Task LoadAllAsync()
		{
			var files = Directory.GetFiles(basePath);
			var tasks = new List<Task<(string, byte[])>>();
			
			foreach (var file in files)
				tasks.Add(load(file));

			var results = await Task.WhenAll(tasks);

			foreach (var result in results) baseCache.Add(result.Item1, result.Item2);

			static async Task<(string, byte[])> load(string file) =>
				(file, await File.ReadAllBytesAsync(file));
		}

		public void Put(string path, ReadOnlySpan<byte> data)
		{
			using (globalLocker.Lock(this))
			{
				var copy = data.ToArray();
				if (baseCache.ContainsKey(path)) baseCache[path] = copy;
				else baseCache.Add(path, copy);
			}
		}

		public void Put(string path, string content) => Put(path, Encoding.Default.GetBytes(content));

		public void PutDefault(string path) => Put(path, defaultValue);

		public string GetString(string path) => Encoding.Default.GetString(Get(path));

		public ReadOnlySpan<byte> Get(string path)
		{
			using (globalLocker.Lock(this))
			{
				if (!baseCache.ContainsKey(path))
					baseCache.Add(path, defaultValue);
				return baseCache[path];
			}
		}

		public async Task SaveAsync()
		{
			using (globalLocker.Lock(this))
			{
				var tasks = new List<Task>();

				foreach (var file in baseCache)
				{

					tasks.Add(push(stream, file.Value));
				}

				await Task.WhenAll(tasks);
			}


			async Task push(string path, byte[] data)
			{
				FileStream? fs = null;

				try
				{
					fs = File.Create(Path.Combine(basePath, path));
					await fs.WriteAsync(data);
					await fs.FlushAsync();
				}
				catch (Exception ex)
				{
					fs?.Dispose();
					logger.Log(LogLevel.Warning);
				}
			}
		}
	}
}

using System.Text;

namespace DidiFrame.Utils
{
	/// <summary>
	/// Tool for synhonized file working
	/// </summary>
	public class FileCache
	{
		private readonly Dictionary<string, byte[]> baseCache = new();
		private readonly string basePath;
		private readonly byte[] defaultValue;
		private static readonly ThreadLocker<FileCache> globalLocker = new();


		/// <summary>
		/// Creates new instance of DidiFrame.Utils.FileCache
		/// </summary>
		/// <param name="basePath">Path to file directory</param>
		/// <param name="defaultValue">Default content for file if that doesn't exist</param>
		public FileCache(string basePath, ReadOnlySpan<byte> defaultValue)
		{
			this.basePath = basePath;
			this.defaultValue = defaultValue.ToArray();
		}

		/// <summary>
		/// Creates new instance of DidiFrame.Utils.FileCache
		/// </summary>
		/// <param name="basePath">Path to file directory</param>
		/// <param name="defaultValue">Default content for file if that doesn't exist</param>
		public FileCache(string basePath, string defaultValue)
		{
			this.basePath = basePath;
			this.defaultValue = Encoding.Default.GetBytes(defaultValue);
		}


		/// <summary>
		/// Loads and caches all files from directory
		/// </summary>
		/// <returns>Wait task</returns>
		public async Task LoadAllAsync()
		{
			var files = Directory.GetFiles(basePath);
			var tasks = new List<Task<(string, byte[])>>();
			
			foreach (var file in files)
				tasks.Add(load(file));

			var results = await Task.WhenAll(tasks);
			
			foreach (var result in results) baseCache.Add(result.Item1, result.Item2);

			async Task<(string, byte[])> load(string file) =>
				(Path.GetRelativePath(basePath, file), await File.ReadAllBytesAsync(file));
		}

		/// <summary>
		/// Put new data to cached file
		/// </summary>
		/// <param name="path">Path to a file</param>
		/// <param name="data">New data</param>
		public void Put(string path, ReadOnlySpan<byte> data)
		{
			using (globalLocker.Lock(this))
			{
				var copy = data.ToArray();
				if (baseCache.ContainsKey(path)) baseCache[path] = copy;
				else baseCache.Add(path, copy);
			}
		}

		/// <summary>
		/// Put new data to cached file
		/// </summary>
		/// <param name="path">Path to a file</param>
		/// <param name="content">New data</param>
		public void Put(string path, string content) => Put(path, Encoding.Default.GetBytes(content));

		/// <summary>
		/// Put default data to cached file
		/// </summary>
		/// <param name="path">Path to a file</param>
		public void PutDefault(string path) => Put(path, defaultValue);

		/// <summary>
		/// Gets file content from cache as string
		/// </summary>
		/// <param name="path">Path to a file</param>
		/// <returns>File content</returns>
		public string GetString(string path) => Encoding.Default.GetString(Get(path));

		/// <summary>
		/// Gets file content from cache as byte span
		/// </summary>
		/// <param name="path">Path to a file</param>
		/// <returns>File content</returns>
		public ReadOnlySpan<byte> Get(string path)
		{
			using (globalLocker.Lock(this))
			{
				if (!baseCache.ContainsKey(path))
					baseCache.Add(path, defaultValue);
				var val = baseCache[path];
				if (val.Length == 0) val = defaultValue;
				return val;
			}
		}

		/// <summary>
		/// Pushes all changes from cache to real drive
		/// </summary>
		/// <returns>Wait task</returns>
		public async Task SaveAsync()
		{
			using (globalLocker.Lock(this))
			{
				var tasks = new List<Task>();

				foreach (var file in baseCache)
				{

					tasks.Add(push(file.Key, file.Value));
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
				catch (Exception)
				{
					//TODO: make logging
					fs?.Dispose();
				}
			}
		}
	}
}

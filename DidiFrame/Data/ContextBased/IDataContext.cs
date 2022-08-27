using DidiFrame.Data.Model;

namespace DidiFrame.Data.ContextBased
{
    /// <summary>
    /// Represents data context for context-based approach
    /// </summary>
	public interface IDataContext
    {
        /// <summary>
        /// Loads a data model using given data key for given server. If these isn't uses the factory
        /// </summary>
        /// <typeparam name="TModel">Work model type</typeparam>
        /// <param name="server">Target server</param>
        /// <param name="key">Data key for model loading</param>
        /// <param name="factory">Factory to create models, if null and these isn't object exeption will be thrown</param>
        /// <returns>Loaded model</returns>
        public TModel Load<TModel>(IServer server, string key, IModelFactory<TModel>? factory = null) where TModel : class, IDataEntity;

        /// <summary>
        /// Puts the data model using given data key for given server
        /// </summary>
        /// <typeparam name="TModel">Work model type</typeparam>
        /// <param name="server">Target server</param>
        /// <param name="key">Data key for model putting</param>
        /// <param name="model">Model to be put</param>
        public void Put<TModel>(IServer server, string key, TModel model) where TModel : class, IDataEntity;

        /// <summary>
        /// Prepares context data. Must be called once on start
        /// </summary>
        /// <returns>Wait task</returns>
        public Task PreloadDataAsync();
    }
}

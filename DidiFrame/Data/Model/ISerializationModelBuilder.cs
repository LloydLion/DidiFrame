namespace DidiFrame.Data.Model
{
	public interface ISerializationModelBuilder
	{
		public void WritePrimitive(string propertyName, object? value);

		public void WriteModel(string propertyName, IDataModel? model);

		public void WriteCollection(string propertyName, IDataCollection? values);
	}
}

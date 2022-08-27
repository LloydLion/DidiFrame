using DidiFrame.Data.Model;
using System.ComponentModel;
using System.Globalization;

namespace DidiFrame.Culture
{
	/// <summary>
	/// Settings that contain culture information of server
	/// </summary>
	public class CultureSettings : AbstractModel
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Culture.CultureSettings
		/// </summary>
		/// <param name="cultureInfo">Culture info of server to be writen into model</param>
		public CultureSettings(IServer server, CultureInfo cultureInfo)
		{
			Server = server;
			CultureInfo = cultureInfo;
		}

#nullable disable
		public CultureSettings(ISerializationModel model) : base(model)
		{
			Server = model.ReadPrimitive<IServer>(nameof(Server));
		}
#nullable restore


		/// <summary>
		/// Server's culture info
		/// </summary>
		public CultureInfo CultureInfo { get => GetDataFromStore<CultureInfo>(); private set => SetDataToStore(value); }

		[ModelProperty(PropertyType.Primitive)]
		public string CultureInfoKey { get => CultureInfo.Name; set => CultureInfo = new(value); }

		public override IServer Server { get; }


		protected override void AdditionalSerializeTo(ISerializationModelBuilder builder)
		{
			builder.WritePrimitive(nameof(Server), Server);
		}
	}
}

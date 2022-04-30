namespace DidiFrame.Utils.Dialogs
{
	public class CtorMessageFactory<TMessage> : IDialogMessageFactory where TMessage : IDialogMessage
	{
		public IDialogMessage Create(DialogContext ctx, IReadOnlyDictionary<string, object?> dynamicParameters)
		{
			var type = typeof(TMessage);
			var ctor = type.GetConstructors().Single();
			var parameters = ctor.GetParameters();

			var args = new List<object?>();
			foreach (var param in parameters)
			{
				args.Add(dynamicParameters[param.Name ?? throw new ImpossibleVariantException()]);
			}

			return (IDialogMessage)(ctor.Invoke(args.Prepend(ctx).ToArray()) ?? throw new ImpossibleVariantException());
		}
	}
}

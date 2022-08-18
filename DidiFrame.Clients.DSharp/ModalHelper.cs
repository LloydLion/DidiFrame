using DidiFrame.UserCommands.Modals;
using DidiFrame.UserCommands.Modals.Components;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace DidiFrame.Client.DSharp
{
	internal sealed class ModalHelper : IDisposable
	{
		private const int ModalTimeoutInMinutes = 2; 


		private readonly Client client;
		private readonly IStringLocalizer localizer;
		private readonly IValidator<ModalModel> modalValidator;
		private readonly Dictionary<string, RunningModal> runningModals = new();


		public ModalHelper(Client client, IStringLocalizer localizer, IValidator<ModalModel> modalValidator)
		{
			client.BaseClient.ModalSubmitted += OnModalSubmitted;
			client.BaseClient.ComponentInteractionCreated += OnComponentInteractionCreated;

			this.client = client;
			this.localizer = localizer;
			this.modalValidator = modalValidator;
		}


		private Task OnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
		{
			var runningModal = runningModals.Values.Where(s => s.HasRetryData).SingleOrDefault(s => s.RetryData.Message.Id == e.Message.Id);

			if (runningModal is null) return Task.CompletedTask;
			else
			{
				var oldValues = runningModal.RetryData.OldValues;

				var components = runningModal.Model.Components.Cast<ModalTextBox>().Select(s =>
				{
					var initialValue = oldValues.ContainsKey(s) ? (string)oldValues[s] : s.InitialValue;
					return new TextInputComponent(s.Label, s.Id, s.PlaceHolder, initialValue,
						s.RequiredToFill, s.Style == TextBoxStyle.Short ? TextInputStyle.Short : TextInputStyle.Paragraph,
						s.MinInputLength, s.MaxInputLength);
				}).ToArray();

				return e.Interaction.CreateResponseAsync(InteractionResponseType.Modal,
					new DiscordInteractionResponseBuilder().WithCustomId(runningModal.Id.ToString()).WithTitle(runningModal.Model.Title).AddComponents(components));
			}
		}

		private async Task OnModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
		{
			ClearClosed();

			var targetId = e.Interaction.Data.CustomId;

			if (runningModals.ContainsKey(targetId))
			{
				var runningModal = runningModals[targetId];
				runningModal.ResetRetryData();

				var components = runningModal.Model.Components;
				var fillData = components.Select(s => new { Value = e.Values[s.Id], Component = s }).ToDictionary(s => s.Component, s => (object)s.Value);

				var scope = new ModalArgumentsScope(fillData);
				var context = new ModalSubmitContext(scope);

				var result = await runningModal.Modal.SubmitModalAsync(context);

				if (result.ResultType == ModalSubmitResult.Type.Successful)
				{
					var msg = MessageConverter.ConvertUp(result.GetMessage());
					runningModals.Remove(targetId);
					await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder(msg).AsEphemeral());
				}
				else if (result.ResultType == ModalSubmitResult.Type.ValidationError)
				{
					var error = result.GetValidationErrorModel();

					var responceBuilder = new DiscordInteractionResponseBuilder()
						.WithContent(localizer["ValidationErrorMessageContent", error.Message, error.Component])
						.AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "retry", localizer["TryAgainLabel"]))
						.AsEphemeral();

					await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responceBuilder);
					var responceMessage = await e.Interaction.GetOriginalResponseAsync();

					runningModal.RetryData = new ModalRetryData(responceMessage, fillData);
				}
				else throw new NotSupportedException();
			}
		}

		public async Task CreateModalAsync(DiscordInteraction interaction, IModalForm form)
		{
			var pair = await CreateModalAsync(interaction, form, Guid.NewGuid());
			runningModals.Add(pair.Key, pair.Value);
		}

		private async Task<KeyValuePair<string, RunningModal>> CreateModalAsync(DiscordInteraction interaction, IModalForm form, Guid guid)
		{
			ClearClosed();

			var id = guid.ToString();

			var builder = new ModalBuilder(modalValidator);

			form.Build(builder);

			var modal = builder.Build();

			var components = modal.Components.Cast<ModalTextBox>().Select(s =>
			{
				return new TextInputComponent(s.Label, s.Id, s.PlaceHolder, s.InitialValue,
					s.RequiredToFill, s.Style == TextBoxStyle.Short ? TextInputStyle.Short : TextInputStyle.Paragraph,
					s.MinInputLength, s.MaxInputLength);
			}).ToArray();

			await interaction.CreateResponseAsync(InteractionResponseType.Modal,
				new DiscordInteractionResponseBuilder().WithCustomId(id).WithTitle(modal.Title).AddComponents(components));

			return new KeyValuePair<string, RunningModal>(id, new RunningModal(DateTime.UtcNow, modal, form, guid));
		}

		public void Dispose()
		{
			client.BaseClient.ModalSubmitted -= OnModalSubmitted;
			client.BaseClient.ComponentInteractionCreated -= OnComponentInteractionCreated;
		}

		private void ClearClosed()
		{
			var toDelete = new List<string>();

			foreach (var item in runningModals)
			{
				if (DateTime.UtcNow - item.Value.CreationDate > TimeSpan.FromMinutes(ModalTimeoutInMinutes))
					toDelete.Add(item.Key);
			}

			foreach (var item in toDelete) runningModals.Remove(item);
		}


		private sealed record RunningModal(DateTime CreationDate, ModalModel Model, IModalForm Modal, Guid Id)
		{
			private ModalRetryData? retryData;


			public ModalRetryData RetryData { get => retryData ?? throw new InvalidOperationException("Enable to get retry data if it isn't generated"); set => retryData = value; }

			public bool HasRetryData => retryData is not null;

			public void ResetRetryData() => retryData = null;
		}

		private sealed record ModalRetryData(DiscordMessage Message, IReadOnlyDictionary<IModalComponent, object> OldValues);
	}
}

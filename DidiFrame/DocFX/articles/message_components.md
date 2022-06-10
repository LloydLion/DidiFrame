# Message components

## Description

Discord provides functions to create and handle buttons, select lists in messages, it called as Discord components and you can read about it in discord's documentation.

DidiFrame contains classes from each discord component: [MessageButton](../api/DidiFrame.Entities.Message.Components.MessageButton.html), [MessageLinkButton](../api/DidiFrame.Entities.Message.Components.MessageLinkButton.html), [MessageSelectMenu](../api/DidiFrame.Entities.Message.Components.MessageSelectMenu.html) (Component row is not visual component, discord uses it only as container).

Each of them you can add into message using ComponentsRows property in [MessageSendModel](http://localhost:8080/api/DidiFrame.Entities.Message.MessageSendModel.html).

Note: MessageComponentRow is container for other components from discord, it represent one row of components, them will be rendered in row.

DidiFrame provides [IInteractionDispatcher](../api/DidiFrame.Interfaces.IInteractionDispatcher.html) interface implementation for each message that bot sends (If you try attach to non bot message nothing will happen).
To access it call `GetInteractionDispatcher()` on message.

It has two methods: `Attach<TComponent>(string, AsyncInteractionCallback<TComponent>)` and `Detach<TComponent>(string, AsyncInteractionCallback<TComponent> callback)` where TComponent is IInteractionComponent.
The first adds handler, the second removes. Handler is special delegates, but it is simple handlers, nothing unusual, but as result every handler must return interaction result that will be displayed to user.

As first parameter both methods accepts string of interaction component to handle.

## Component interface

DidiFrame provides two interfaces: [IComponent]() and [IInteractionComponent]().

* The first is simple component (like [MessageLinkButton](../api/DidiFrame.Entities.Message.Components.MessageLinkButton.html)), doesn't create any interactions. This components can be used only as visual componets.
* The second inherits the first, but supports interactions, it can be used in [IInteractionDispatcher](../api/DidiFrame.Interfaces.IInteractionDispatcher.html).

## Using example

```cs
var id = message.GetInteractionDispatcher();

id.Attach<MessageButton>("demo_bnt", ctx =>
{
	//Do something
	return new(new MessageSendModel("Hello from demo button"));
})

//To detach use functions

```
# User commands and pipelines

Commands is main methods for user to interact with bot. We should know how it works in DidiFrame.

## What are user commands

User command is [model](../api/DidiFrame.UserCommands.Models.UserCommandInfo.html) that contains info about executor, arguments and its name.
Commands can be realized using different paths, this behavior is determined by User command pipeline.
Command can has many forms like simple text command or discord application's command.

## Where commands are stored

In [User commands repository](../api/DidiFrame.UserCommands.Repository.IUserCommandsRepository.html). It is a collection for commands in each server. You can add commands to repository (Only in bot's Main) using its methods.

If need to get commands for server you can use `GetCommandsFor(IServer)` that returns a new [collection](../api/DidiFrame.UserCommands.Repository.IUserCommandsCollection.html) of commands.

DidiFrame provides simple implementation of repository interface - [SimpleUserCommandsRepository](../api/DidiFrame.UserCommands.Repository.SimpleUserCommandsRepository.html), but it doesn't support "per server commands".

## How commands are loaded

Commands are loaded by [User commands loader](../api/DidiFrame.UserCommands.Loader.IUserCommandsLoader.html). This classes contains only one method - `LoadTo(IUserCommandsRepository)` that loads commands from some source to repository.
Application builder gets loaders from service collection, it supports many loaders.

DidiFrame provides simple implementations of interface: [ReflectionUserCommandsLoader](../api/DidiFrame.UserCommands.Loader.Reflection.ReflectionUserCommandsLoader.html) and [HelpCommandLoader](../api/DidiFrame.UserCommands.Loader.EmbededCommands.Help.HelpCommandLoader.html)  
* ReflectionUserCommandsLoader is loader that loads from commands modules.
* HelpCommandLoader simple loads 2 commands: `help` and `cmd` to help users get information about bot.

## Reflection user commands loader

ReflectionUserCommandsLoader gets implementations of [ICommandsModule](../api/DidiFrame.UserCommands.Loader.Reflection.ICommandsModule.html) from service collection and analyzes them.
It gets all method that marked as command by [CommandAttribute](../api/DidiFrame.UserCommands.Loader.Reflection.CommandAttribute.html). Attribute requires set command name.  
Then the loader reads method signature:
* return type must be [UserCommandResult](../api/DidiFrame.UserCommands.Models.UserCommandResult.html) or Task\<[UserCommandResult](../api/DidiFrame.UserCommands.Models.UserCommandResult.html)\>
* first argument must be [UserCommandContext](../api/DidiFrame.UserCommands.Models.UserCommandContext.html)
* other arguments will converted to command's arguments

When user calls command method will be called with trasmited arguments and handles command.

## Commands extending

Commands can contain additional components to extend them functional.
Commands do it using [IModelAdditionalInfoProvider](../api/DidiFrame.Utils.ExtendableModels.IModelAdditionalInfoProvider.html). This class like to IServiceProvider, but for models and using to extend models.

Same mechanism also used for each command's argument.

DidiFrame has simple implementation for [IModelAdditionalInfoProvider](../api/DidiFrame.Utils.ExtendableModels.IModelAdditionalInfoProvider.html) interface - [SimpleModelAdditionalInfoProvider](../DidiFrame.Utils.ExtendableModels.SimpleModelAdditionalInfoProvider.html)

## Argument validation

Commands' arguments can accept any values, but sometimes it is bad and we need to constrain values.
Framework has validation mechanism.

To validate argument's value you need to implementate [IUserCommandArgumentValidator](../api/DidiFrame.UserCommands.ContextValidation.Arguments.IUserCommandArgumentValidator.html) in some class or use [ready validators](../api/DidiFrame.UserCommands.ContextValidation.Arguments.Validators.html).  
Interfacte has one method - `ValidationFailResult? Validate(IServiceProvider, UserCommandContext, UserCommandArgument, UserCommandContext.ArgumentValue)` that will be called before handler and can drop command pipline if returns not null value  
About return: if `Validate` methods returns null it means that all ok else result must have locale key to print error and final command execution code. Locale key will be transcripted in default localizer for command (From additional info) by pattern: $(command_name).$(arg_name):$(key).

To apply validators add collection with them into additional info of argument.  
To apply it in reflection loader use [ValidatorAttribute](../api/DidiFrame.UserCommands.Loader.Reflection.ValidatorAttribute.html) on argument.

Tip 1: you can see error locale key of default validators in them description.

Tip 2: abstract class [AbstractArgumentValidator](../api/DidiFrame.UserCommands.ContextValidation.Arguments.AbstractArgumentValidator-1.html) can help in validators createtion.

## Invoker filtration

Filters for invoker are useful if need to check permission level or some timeouts.

Filter is class that implements [IUserCommandInvokerFilter](../api/DidiFrame.UserCommands.ContextValidation.Invoker.IUserCommandInvokerFilter.html) interface.  
DidiFrame provides some filters in [DidiFrame.UserCommands.ContextValidation.Invoker.Filters](../DidiFrame.UserCommands.ContextValidation.Invoker.Filters.html)

Filter is very simillar to validators: has simillar interface with one method, is called before handler and can drop pipeline.  
Method in [IUserCommandInvokerFilter](../api/DidiFrame.UserCommands.ContextValidation.Invoker.IUserCommandInvokerFilter.html) has same result type and it works same, but pattern is other: $(command_name):$(key).

To apply filters add collection with them into additional info of command.  
To apply it in reflection loader use [InvokerFilter](../api/DidiFrame.UserCommands.Loader.Reflection.InvokerFilter.html) attribute on method.

Tip: you can see error locale key of default filters in them description.

## Custom arguments types

As command's argumetns you can use constrained set of types, each of them listed in [UserCommandArgument.Type](../api/DidiFrame.UserCommands.Models.UserCommandArgument.Type.html) enum, but what if you want use more.

Command arguments before they are intercepted by the executor, passed to the command, go through a conversion process.

To create custom type implementate [IDefaultContextConveterSubConverter](api/DidiFrame.UserCommands.PreProcessing.IDefaultContextConveterSubConverter.html) interface and add it into di container.

It works like it:

\cmd <ins>"text"</ins> <ins>42 \@Nobody</ins> <ins>02.12\|12:23:22</ins>

```raw
|Type  |Raw            |Convertation|Result        |  
|------|---------------|------------|--------------|  
|string|"text"         | ---->      |"text"        |  
|int   |42             | --|        |              |  
|member|@Nobody        | --} --->   |Custom model  |  
|date  |02.12|12:23:22 | ---->      |02.12|12:23:22|
```

## User command pipeline

Pipeline is main component of all user commands subsystem, it processes validators and filters, pareses and sends messages and also does all other work.

A pipeline works simple: central component is dispatcher ([IUserCommandPipelineDispatcher\`1](../api/DidiFrame.UserCommands.Pipeline.IUserCommandPipelineDispatcher-1.html)).  
It starts and ends pipeline. It contains single method that sets callback. Dispatcher must trigger callback at some event and transmit call info with special dispatcher's object, it initiate pipeline start.  
Call info includes [meta info](../api/DidiFrame.UserCommands.Models.UserCommandSendData.html) and second callback that will be called at pipeline end.

Dispatcher triggers the chain of middlewares.  
[Middleware](../api/DidiFrame.UserCommands.Pipeline.IUserCommandPipelineMiddleware.html) is middle component and element of chain. Middleware has single method - `TOut? Process(TIn, PipelineContext)` that processes some object into other.

Finnally, we have that execution order:
Dispatcher \-\> Middleware 1 \-\> Middleware 2 \-\> Middleware 3 \-\> ... \-\> Dispatcher's callback

Each link has custom and determined by generic types transit object's type. Dispatcher has special object that will be accepted by first middleware as input, than the middleware processes output object that trasmites to next middleware as input.
The last middleware must return [UserCommandResult](../api/DidiFrame.UserCommands.Models.UserCommandResult.html) that object dispatcher's callback will accept.

About support classes:
* [UserCommandPipeline](../api/DidiFrame.UserCommands.Pipeline.UserCommandPipeline.html) - Model of allmost pipeline with each middleware and dispatcher
* [IUserCommandPipelineExecutor](../api/DidiFrame.UserCommands.Pipeline.IUserCommandPipelineExecutor.html) - Executor of pipeline that accepts pipeline and input object from dispatcher, returns task with [UserCommandResult](../api/DidiFrame.UserCommands.Models.UserCommandResult.html) for dispatcher's callback.
* [UserCommandPipelineContext](../api/DidiFrame.UserCommands.Pipeline.UserCommandPipelineContext.html) - Help context for pipeline's middlewares

Note:
* Executor don't work with dispatcher, callback that calls executor must be assignated outside.
* Executor is async.
* Pipeline finalization stops pipeline executing and returns result to dispatcher's callback, but dropping is stops pipeline executing and don't calls dispatcher's callback.
* To drop or finalize pipeline return null from middleware and call special context's method.
* [IDiscordApplication](../api/DidiFrame.Application.IDiscordApplication.html) automaticlly registers all need for pipeline stuff using special builder.
* If you want create your middleware use [AbstractUserCommandPipelineMiddleware\`2](../api/DidiFrame.UserCommands.Pipeline.AbstractUserCommandPipelineMiddleware-2.html) support class .

## User command pipeline building

Adding pipeline to di container or direct constructing means using builder to create pipeline.

Why we should do it, we can directly create instance of [UserCommandPipeline](../api/DidiFrame.UserCommands.Pipeline.UserCommandPipeline.html) and use it in executor? Answer - builder is more convenient, it: checks type using generic types, support di container to integrate each pipeline element, has extensions methods do something more and builder is required if you use [IDiscordApplication](../api/DidiFrame.Application.IDiscordApplication.html).

Builder is class that implements [IUserCommandPipelineBuilder](../api/DidiFrame.UserCommands.Pipeline.Building.IUserCommandPipelineBuilder.html) interface. Default implementation of it interface is internal class of DidiFrame and you can't access it outside.
But you has `AddUserCommandPipeline(Action\<IUserCommandPipelineBuilder\>)` extension method for service collection. Builder is standard fluent builder, simple read methods description and don't forget call `Build()` at end.
Also builder has extension methods that can simplify process.

Note:  
Framework has ready pipeline that can be added using `ClassicMessageUserCommandPipeline(IConfiguration textCommandParserConfig, IConfiguration defaultCommandsExecutorConfig)` extension method on service collection.  
That contains:  
[MessageUserCommandDispatcher](../api/DidiFrame.UserCommands.Pipeline.Utils.MessageUserCommandDispatcher.html) -> [DelegateMiddleware\<IMessage, string\>](../api/DidiFrame.UserCommands.Pipeline.Utils.DelegateMiddleware-2.html) -> [TextCommandParser](../api/DidiFrame.UserCommands.Pipeline.Utils.TextCommandParser.html) -> [DefaultUserCommandContextConverter](../api/DidiFrame.UserCommands.PreProcessing.DefaultUserCommandContextConverter.html) -> [ContextValidator](../api/DidiFrame.UserCommands.ContextValidation.ContextValidator.html) -> [DefaultUserCommandsExecutor](../api/DidiFrame.UserCommands.Executing.DefaultUserCommandsExecutor.html).  
Each of they integrated in di container ([DefaultUserCommandContextConverter](../api/DidiFrame.UserCommands.PreProcessing.DefaultUserCommandContextConverter.html) registrated as [IUserCommandContextConverter](../api/DidiFrame.UserCommands.PreProcessing.IUserCommandContextConverter.html)).
DelegateMiddleware selects content from message.
# Auto injecting

## Description

Service collection is super tool, but if you create very big project main file with collection inizialization becomes huge.
To solve this problem DidiFrame has auto injecting mechanism.

To inject services call `InjectAutoDependencies(IAutoInjector)` and transmit some [IAutoInjector](../api/DidiFrame.AutoInjecting.IAutoInjector.html) inteface implementation.
DidiFrame privides reflection based aut injector - [ReflectionAutoInjector](../api/DidiFrame.AutoInjecting.ReflectionAutoInjector.html) that gets services from given to ctor assembly using type that implements [IAutoSubInjector](../api/DidiFrame.AutoInjecting.IAutoSubInjector.html)

## Using example

**Program.cs**
```cs
public static void Main(string[] args)
{
	...

	builder.AddServices(services =>
		...
		.InjectAutoDependencies(new ReflectionAutoInjector(Assembly.GetExecutingAssembly()))
		//Provide assembly is optional, injector has default ctor that gets calling assembly
	)
	...
}
```

**Other file**
```cs
internal class SomeSystemAutoInjector : IAutoSubInjector
{
	void InjectDependencies(IServiceCollection services)
	{
		//Add something here
	}
}
```
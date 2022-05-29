# Deserialize with dependency injection

This sample deserializes JSON using dependency injection.

<!-- snippet: DeserializeWithDependencyInjectionTypes -->
<a id='snippet-deserializewithdependencyinjectiontypes'></a>
```cs
public class AutofacContractResolver : DefaultContractResolver
{
    readonly IContainer container;

    public AutofacContractResolver(IContainer container)
    {
        this.container = container;
    }

    protected override JsonObjectContract CreateObjectContract(Type type)
    {
        // use Autofac to create types that have been registered with it
        if (container.IsRegistered(type))
        {
            var contract = ResolveContact(type);
            contract.DefaultCreator = () => container.Resolve(type);

            return contract;
        }

        return base.CreateObjectContract(type);
    }

    JsonObjectContract ResolveContact(Type type)
    {
        // attempt to create the contact from the resolved type
        if (container.ComponentRegistry.TryGetRegistration(new TypedService(type), out var registration))
        {
            var viewType = (registration.Activator as ReflectionActivator)?.LimitType;
            if (viewType != null)
            {
                return base.CreateObjectContract(viewType);
            }
        }

        // fall back to using the registered type
        return base.CreateObjectContract(type);
    }
}

public class TaskController
{
    public TaskController(ITaskRepository repository, ILogger logger)
    {
        Repository = repository;
        Logger = logger;
    }

    public ITaskRepository Repository { get; }

    public ILogger Logger { get; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeWithDependencyInjection.cs#L11-L66' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializewithdependencyinjectiontypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeWithDependencyInjectionUsage -->
<a id='snippet-deserializewithdependencyinjectionusage'></a>
```cs
var builder = new ContainerBuilder();
builder.RegisterType<TaskRepository>().As<ITaskRepository>();
builder.RegisterType<TaskController>();
builder.Register(_ => new LogManager(new(2000, 12, 12))).As<ILogger>();

var container = builder.Build();

var contractResolver = new AutofacContractResolver(container);

var json = @"{
      'Logger': {
        'Level':'Debug'
      }
    }";

// ITaskRespository and ILogger constructor parameters are injected by Autofac
var controller = JsonConvert.DeserializeObject<TaskController>(json, new JsonSerializerSettings
{
    ContractResolver = contractResolver
});

Console.WriteLine(controller.Repository.GetType().Name);
// TaskRepository
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeWithDependencyInjection.cs#L71-L97' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializewithdependencyinjectionusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

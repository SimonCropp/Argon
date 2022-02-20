# Deserialize with dependency injection

This sample deserializes JSON using dependency injection.

<!-- snippet: DeserializeWithDependencyInjectionTypes -->
<a id='snippet-deserializewithdependencyinjectiontypes'></a>
```cs
public class AutofacContractResolver : DefaultContractResolver
{
    readonly IContainer _container;

    public AutofacContractResolver(IContainer container)
    {
        _container = container;
    }

    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        // use Autofac to create types that have been registered with it
        if (_container.IsRegistered(objectType))
        {
            var contract = ResolveContact(objectType);
            contract.DefaultCreator = () => _container.Resolve(objectType);

            return contract;
        }

        return base.CreateObjectContract(objectType);
    }

    JsonObjectContract ResolveContact(Type objectType)
    {
        // attempt to create the contact from the resolved type
        if (_container.ComponentRegistry.TryGetRegistration(new TypedService(objectType), out var registration))
        {
            var viewType = (registration.Activator as ReflectionActivator)?.LimitType;
            if (viewType != null)
            {
                return base.CreateObjectContract(viewType);
            }
        }

        // fall back to using the registered type
        return base.CreateObjectContract(objectType);
    }
}

public class TaskController
{
    readonly ITaskRepository _repository;
    readonly ILogger _logger;

    public TaskController(ITaskRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public ITaskRepository Repository => _repository;

    public ILogger Logger => _logger;
}
```
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/DeserializeWithDependencyInjection.cs#L37-L93' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializewithdependencyinjectiontypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeWithDependencyInjectionUsage -->
<a id='snippet-deserializewithdependencyinjectionusage'></a>
```cs
var builder = new ContainerBuilder();
builder.RegisterType<TaskRepository>().As<ITaskRepository>();
builder.RegisterType<TaskController>();
builder.Register(_ => new LogService(new DateTime(2000, 12, 12))).As<ILogger>();

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
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/DeserializeWithDependencyInjection.cs#L98-L122' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializewithdependencyinjectionusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

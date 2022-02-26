// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;

public class DeserializeWithDependencyInjection : TestFixtureBase
{
    #region DeserializeWithDependencyInjectionTypes
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
            this.Repository = repository;
            this.Logger = logger;
        }

        public ITaskRepository Repository { get; }

        public ILogger Logger { get; }
    }
    #endregion

    [Fact]
    public void Example()
    {
        #region DeserializeWithDependencyInjectionUsage
        var builder = new ContainerBuilder();
        builder.RegisterType<TaskRepository>().As<ITaskRepository>();
        builder.RegisterType<TaskController>();
        builder.Register(_ => new LogManager(new DateTime(2000, 12, 12))).As<ILogger>();

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
        #endregion

        Assert.NotNull(controller);
        Assert.NotNull(controller.Logger);

        Assert.Equal(new DateTime(2000, 12, 12), controller.Logger.DateTime);
        Assert.Equal("Debug", controller.Logger.Level);
    }

    public interface IBase
    {
        DateTime CreatedOn { get; set; }
    }

    public interface ITaskRepository : IBase
    {
        string ConnectionString { get; set; }
    }

    public interface ILogger
    {
        DateTime DateTime { get; }
        string Level { get; set; }
    }

    public class Base : IBase
    {
        public DateTime CreatedOn { get; set; }
    }

    public class TaskRepository : Base, ITaskRepository
    {
        public string ConnectionString { get; set; }
    }

    public class LogManager : ILogger
    {
        public LogManager(DateTime dt)
        {
            DateTime = dt;
        }

        public DateTime DateTime { get; }

        public string Level { get; set; }
    }

    [DataContract]
    public class User
    {
        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [DataMember(Name = "company")]
        public ICompany Company { get; set; }
    }

    public interface ICompany
    {
        string CompanyName { get; set; }
    }

    [DataContract]
    public class Company : ICompany
    {
        [DataMember(Name = "company_name")]
        public string CompanyName { get; set; }
    }
}
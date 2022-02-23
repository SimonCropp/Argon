#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

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
        readonly DateTime _dt;

        public LogManager(DateTime dt)
        {
            _dt = dt;
        }

        public DateTime DateTime => _dt;

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
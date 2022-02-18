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

using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Argon.Tests.Serialization;
using LogService = Argon.Tests.Serialization.LogManager;

namespace Argon.Tests.Documentation.Samples.Serializer;

public class DeserializeWithDependencyInjection : TestFixtureBase
{
    #region Types
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
    #endregion

    [Fact]
    public void Example()
    {
        #region Usage
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
        #endregion

        Xunit.Assert.NotNull(controller);
        Xunit.Assert.NotNull(controller.Logger);

        Assert.AreEqual(new DateTime(2000, 12, 12), controller.Logger.DateTime);
        Assert.AreEqual("Debug", controller.Logger.Level);
    }
}
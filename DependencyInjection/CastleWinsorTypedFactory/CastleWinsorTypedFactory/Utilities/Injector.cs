using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueryProcessor;
using APIProcessor;
using Connector;

namespace CastleWinsorTypedFactory.Utilities
{
    public static class Injector
    {
        private static readonly object InstanceLock = new object();

        private static IWindsorContainer instance;

        public static IWindsorContainer Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    return instance ?? (instance = GetInjector());
                }
            }
        }

        private static IWindsorContainer GetInjector()
        {
            var container = new WindsorContainer();

            container.Install(FromAssembly.This());

            RegisterInjector(container);

            return container;
        }

        private static void RegisterInjector(WindsorContainer container)
        {

            // To use the TypedFactoryFacility must add it
            container.AddFacility<TypedFactoryFacility>();

            // Any selectors must be registered 
            container.Register(
                Component.For<ProcessorFactorySelector>()
                );


            // Factories must be registered as a Factory
            container.Register(
                Component
                    .For<IConfigurableObjectFactory>()
                    .AsFactory()
                    );

            // Factories must be registered as a Factory
            container
                .Register(Component
                    .For<IProcessorFactory>()
                    .AsFactory(x => x.SelectedWith<ProcessorFactorySelector>())
                    );

            // To be able to selectively resolve a specific
            // class for an interface we need to Name them
            container.Register(
                Component
                    .For<IProcessor>()
                    .ImplementedBy<QueryProcessorClass>()
                    .Named("Query")
                    );

            container.Register(
                Component
                    .For<IProcessor>()
                    .ImplementedBy<APIProcessorClass>()
                    .Named("API")
                    );

            container.Register(
                Component
                    .For<IConfigurableObject>()
                    .ImplementedBy<ConnectorClass>()
                    );

            container.Register(
                Component
                    .For<CastleWinsorTypedFactoryObject>()
                );

        }
            
    }
}

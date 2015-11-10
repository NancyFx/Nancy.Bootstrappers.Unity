namespace Nancy.Bootstrappers.Unity
{
    using System;
    using System.Collections.Generic;
    using Diagnostics;
    using Microsoft.Practices.Unity;
    using Nancy.Configuration;
    using Bootstrapper;
    using ViewEngines;

    /// <summary>
    /// Nancy bootstrapper for the Unity container.
    /// </summary>
    public abstract class UnityNancyBootstrapper : NancyBootstrapperWithRequestContainerBase<IUnityContainer>
    {
        /// <summary>
        /// Gets the diagnostics for intialisation
        /// </summary>
        /// <returns>An <see cref="IDiagnostics"/> implementation</returns>
        protected override IDiagnostics GetDiagnostics()
        {
            return this.ApplicationContainer.Resolve<IDiagnostics>();
        }

        /// <summary>
        /// Gets all registered startup tasks
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="IApplicationStartup"/> instances. </returns>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return this.ApplicationContainer.ResolveAll<IApplicationStartup>();
        }

        /// <summary>
        /// Registers and resolves all request startup tasks
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="requestStartupTypes">Types to register</param>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> instance containing <see cref="IRequestStartup"/> instances.</returns>
        protected override IEnumerable<IRequestStartup> RegisterAndGetRequestStartupTasks(IUnityContainer container, Type[] requestStartupTypes)
        {
            foreach (var requestStartupType in requestStartupTypes)
            {
                container.RegisterType(
                    typeof(IRequestStartup),
                    requestStartupType,
                    requestStartupType.ToString(),
                    new ContainerControlledLifetimeManager());
            }

            return container.ResolveAll<IRequestStartup>();
        }

        /// <summary>
        /// Gets all registered application registration tasks
        /// </summary>
        /// <returns>An <see cref="System.Collections.Generic.IEnumerable{T}"/> instance containing <see cref="IRegistrations"/> instances.</returns>
        protected override IEnumerable<IRegistrations> GetRegistrationTasks()
        {
            return this.ApplicationContainer.ResolveAll<IRegistrations>();
        }

        /// <summary>
        /// Resolve <see cref="INancyEngine"/>
        /// </summary>
        /// <returns>An <see cref="INancyEngine"/> implementation</returns>
        protected override INancyEngine GetEngineInternal()
        {
            return this.ApplicationContainer.Resolve<INancyEngine>();
        }

        /// <summary>
        /// Gets the <see cref="INancyEnvironmentConfigurator"/> used by th.
        /// </summary>
        /// <returns>An <see cref="INancyEnvironmentConfigurator"/> instance.</returns>
        protected override INancyEnvironmentConfigurator GetEnvironmentConfigurator()
        {
            return this.ApplicationContainer.Resolve<INancyEnvironmentConfigurator>();
        }

        /// <summary>
        /// Registers an <see cref="INancyEnvironment"/> instance in the container.
        /// </summary>
        /// <param name="container">The container to register into.</param>
        /// <param name="environment">The <see cref="INancyEnvironment"/> instance to register.</param>
        protected override void RegisterNancyEnvironment(IUnityContainer container, INancyEnvironment environment)
        {
            container.RegisterInstance(environment);
        }

        /// <summary>
        /// Gets the application level container
        /// </summary>
        /// <returns>Container instance</returns>
        protected override IUnityContainer GetApplicationContainer()
        {
            return new UnityContainer();
        }

        /// <summary>
        /// Register the bootstrapper's implemented types into the container.
        /// This is necessary so a user can pass in a populated container but not have
        /// to take the responsibility of registering things like <see cref="INancyModuleCatalog"/> manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override void RegisterBootstrapperTypes(IUnityContainer applicationContainer)
        {
            // This is here to add the EnumerableExtension, even though someone
            // used an external IUnityContainer (not created by this bootstrapper).
            // It's probably not the best place for it, but it's called right after
            // GetApplicationContainer in NancyBootstrapperBase.
            applicationContainer.AddNewExtension<EnumerableExtension>();

            applicationContainer.RegisterInstance<INancyModuleCatalog>(this, new ContainerControlledLifetimeManager());
        }

        /// <summary>
        /// Register the default implementations of internally used types into the container as singletons
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="typeRegistrations">Type registrations to register</param>
        protected override void RegisterTypes(IUnityContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            foreach (var typeRegistration in typeRegistrations)
            {
                switch (typeRegistration.Lifetime)
                {
                    case Lifetime.Transient:
                        container.RegisterType(
                            typeRegistration.RegistrationType,
                            typeRegistration.ImplementationType,
                            new TransientLifetimeManager());
                        break;
                    case Lifetime.Singleton:
                        container.RegisterType(
                            typeRegistration.RegistrationType,
                            typeRegistration.ImplementationType,
                            new ContainerControlledLifetimeManager());
                        break;
                    case Lifetime.PerRequest:
                        throw new InvalidOperationException("Unable to directly register a per request lifetime.");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Added this in here because Unity doesn't seem to support
            // resolving using the greediest resolvable constructor
            container.RegisterType<IFileSystemReader, DefaultFileSystemReader>(new ContainerControlledLifetimeManager());
        }

        /// <summary>
        /// Register the various collections into the container as singletons to later be resolved
        /// by IEnumerable{Type} constructor dependencies.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="collectionTypeRegistrations">Collection type registrations to register</param>
        protected override void RegisterCollectionTypes(IUnityContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            foreach (var collectionTypeRegistration in collectionTypeRegistrations)
            {
                foreach (var implementationType in collectionTypeRegistration.ImplementationTypes)
                {
                    switch (collectionTypeRegistration.Lifetime)
                    {
                        case Lifetime.Transient:
                            container.RegisterType(
                                collectionTypeRegistration.RegistrationType,
                                implementationType,
                                implementationType.ToString(),
                                new TransientLifetimeManager());
                            break;
                        case Lifetime.Singleton:
                            container.RegisterType(
                                collectionTypeRegistration.RegistrationType,
                                implementationType,
                                implementationType.ToString(),
                                new ContainerControlledLifetimeManager());
                            break;
                        case Lifetime.PerRequest:
                            throw new InvalidOperationException("Unable to directly register a per request lifetime.");
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        /// <summary>
        /// Register the given instances into the container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(IUnityContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            foreach (var instanceRegistration in instanceRegistrations)
            {
                container.RegisterInstance(
                    instanceRegistration.RegistrationType,
                    instanceRegistration.Implementation,
                    new ContainerControlledLifetimeManager());
            }
        }

        /// <summary>
        /// Creates a per request child/nested container
        /// </summary>
        /// <param name="context">Current context</param>
        /// <returns>Request container instance</returns>
        protected override IUnityContainer CreateRequestContainer(NancyContext context)
        {
            return this.ApplicationContainer.CreateChildContainer();
        }

        /// <summary>
        /// Register the given module types into the request container
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="moduleRegistrationTypes">An <see cref="INancyModule"/> types</param>
        protected override void RegisterRequestContainerModules(IUnityContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            foreach (var moduleRegistrationType in moduleRegistrationTypes)
            {
                container.RegisterType(
                    typeof(INancyModule),
                    moduleRegistrationType.ModuleType,
                    moduleRegistrationType.ModuleType.FullName,
                    new ContainerControlledLifetimeManager());
            }
        }

        /// <summary>
        /// Retrieve all module instances from the container
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <returns>Collection of An <see cref="INancyModule"/> instances</returns>
        protected override IEnumerable<INancyModule> GetAllModules(IUnityContainer container)
        {
            return container.ResolveAll<INancyModule>();
        }

        /// <summary>
        /// Retreive a specific module instance from the container
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="moduleType">Type of the module</param>
        /// <returns>An <see cref="INancyModule"/> instance</returns>
        protected override INancyModule GetModule(IUnityContainer container, Type moduleType)
        {
            container.RegisterType(typeof(INancyModule), moduleType, new ContainerControlledLifetimeManager());

            return container.Resolve<INancyModule>();
        }
    }
}

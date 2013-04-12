namespace Nancy.Bootstrappers.Unity
{
    using System;
    using System.Collections.Generic;
    using Diagnostics;
    using Microsoft.Practices.Unity;
    using Bootstrapper;
    using Nancy.ErrorHandling;
    using Nancy.ModelBinding;
    using Nancy.ViewEngines;
    using Responses.Negotiation;
    using Validation;
    using ViewEngines.SuperSimpleViewEngine;

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
        /// Gets all registered application registration tasks
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="IApplicationRegistrations"/> instances.</returns>
        protected override IEnumerable<IApplicationRegistrations> GetApplicationRegistrationTasks()
        {
            return this.ApplicationContainer.ResolveAll<IApplicationRegistrations>();
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
                container.RegisterType(
                    typeRegistration.RegistrationType,
                    typeRegistration.ImplementationType,
                    new ContainerControlledLifetimeManager());
            }

            container.RegisterType(typeof(IEnumerable<IViewEngine>), typeof(UnityEnumerableShim<IViewEngine>));
            container.RegisterType(typeof(IEnumerable<IModelBinder>), typeof(UnityEnumerableShim<IModelBinder>));
            container.RegisterType(typeof(IEnumerable<ITypeConverter>), typeof(UnityEnumerableShim<ITypeConverter>));
            container.RegisterType(typeof(IEnumerable<IBodyDeserializer>), typeof(UnityEnumerableShim<IBodyDeserializer>));
            container.RegisterType(typeof(IEnumerable<IApplicationStartup>), typeof(UnityEnumerableShim<IApplicationStartup>));
            container.RegisterType(typeof(IEnumerable<IApplicationRegistrations>), typeof(UnityEnumerableShim<IApplicationRegistrations>));
            container.RegisterType(typeof(IEnumerable<ISerializer>), typeof(UnityEnumerableShim<ISerializer>));
            container.RegisterType(typeof(IEnumerable<IStatusCodeHandler>), typeof(UnityEnumerableShim<IStatusCodeHandler>));
            container.RegisterType(typeof(IEnumerable<IModelValidatorFactory>), typeof(UnityEnumerableShim<IModelValidatorFactory>));
            container.RegisterType(typeof(IEnumerable<IDiagnosticsProvider>), typeof(UnityEnumerableShim<IDiagnosticsProvider>));
            container.RegisterType(typeof(IEnumerable<IResponseProcessor>), typeof(UnityEnumerableShim<IResponseProcessor>));
            container.RegisterType(typeof(IEnumerable<ISuperSimpleViewEngineMatcher>), typeof(UnityEnumerableShim<ISuperSimpleViewEngineMatcher>));

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
                    container.RegisterType(
                        collectionTypeRegistration.RegistrationType,
                        implementationType,
                        implementationType.ToString(),
                        new ContainerControlledLifetimeManager());
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
        /// <returns>Request container instance</returns>
        protected override IUnityContainer CreateRequestContainer()
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
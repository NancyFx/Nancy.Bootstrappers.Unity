namespace Nancy.Bootstrappers.Unity
{
    using System.Collections.Generic;
    using Microsoft.Practices.Unity;
    using Bootstrapper;

    using Nancy.ErrorHandling;
    using Nancy.ModelBinding;
    using Nancy.ViewEngines;

    /// <summary>
    /// Defines the functionality of a Nancy boostrapper based on the Unity container.
    /// </summary>
    public abstract class UnityNancyBootstrapper : NancyBootstrapperWithRequestContainerBase<IUnityContainer>
    {
        /// <summary>
        /// Gets all registered application startup tasks
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="IApplicationStartup"/> instances. </returns>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return this.ApplicationContainer.ResolveAll<IApplicationStartup>();
        }

        /// <summary>
        /// Gets all registered request startup tasks
        /// </summary>
        /// <param name="container">Request container instance</param>
        /// <returns>An <see cref="IEnumerable{T}"/> instance containing <see cref="IRequestStartup"/> instances. </returns>
        protected override IEnumerable<IRequestStartup> GetRequestStartupTasks(IUnityContainer container)
        {
            return container.ResolveAll<IRequestStartup>();
        }

        /// <summary>
        /// Resolve INancyEngine
        /// </summary>
        /// <returns>INancyEngine implementation</returns>
        protected override INancyEngine GetEngineInternal()
        {
            return this.ApplicationContainer.Resolve<INancyEngine>();
        }

        /// <summary>
        /// Get the moduleKey generator
        /// </summary>
        /// <returns>IModuleKeyGenerator instance</returns>
        protected override IModuleKeyGenerator GetModuleKeyGenerator()
        {
            return this.ApplicationContainer.Resolve<IModuleKeyGenerator>();
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
        /// to take the responsibility of registering things like INancyModuleCatalog manually.
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
            container.RegisterType(typeof(IEnumerable<IRequestStartup>), typeof(UnityEnumerableShim<IRequestStartup>));
            container.RegisterType(typeof(IEnumerable<ISerializer>), typeof(UnityEnumerableShim<ISerializer>));
            container.RegisterType(typeof(IEnumerable<IErrorHandler>), typeof(UnityEnumerableShim<IErrorHandler>));

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
        /// <param name="moduleRegistrationTypes">NancyModule types</param>
        protected override void RegisterRequestContainerModules(IUnityContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            foreach (var moduleRegistrationType in moduleRegistrationTypes)
            {
                container.RegisterType(
                    typeof(NancyModule),
                    moduleRegistrationType.ModuleType,
                    moduleRegistrationType.ModuleKey,
                    new ContainerControlledLifetimeManager());
            }
        }

        /// <summary>
        /// Retrieve all module instances from the container
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <returns>Collection of NancyModule instances</returns>
        protected override IEnumerable<NancyModule> GetAllModules(IUnityContainer container)
        {
            return container.ResolveAll<NancyModule>();
        }

        /// <summary>
        /// Retreive a specific module instance from the container by its key
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="moduleKey">Module key of the module</param>
        /// <returns>NancyModule instance</returns>
        protected override NancyModule GetModuleByKey(IUnityContainer container, string moduleKey)
        {
            return container.Resolve<NancyModule>(moduleKey);
        }
    }
}
namespace Nancy.BootStrappers.Unity.Tests
{
    using System;
    using System.Collections.Generic;

    using Bootstrapper;
    using Microsoft.Practices.Unity;
    using Nancy.Bootstrappers.Unity;
    using Nancy.Tests.Fakes;

    public class FakeUnityNancyBootstrapper : UnityNancyBootstrapper
    {
        private readonly NancyInternalConfiguration configuration;
        public bool ApplicationContainerConfigured { get; set; }
        public bool RequestContainerConfigured { get; set; }

        public FakeUnityNancyBootstrapper()
            : this(null)
        {
        }

        public FakeUnityNancyBootstrapper(NancyInternalConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void ApplicationStartup(IUnityContainer container, Bootstrapper.IPipelines pipelines)
        {
            RequestContainerConfigured = true;
        }

        public IUnityContainer Container
        {
            get { return this.ApplicationContainer; }
        }

        protected override void ConfigureApplicationContainer(IUnityContainer existingContainer)
        {
            ApplicationContainerConfigured = true;
            base.ConfigureApplicationContainer(existingContainer);

            existingContainer.RegisterType<IFoo, Foo>(new ContainerControlledLifetimeManager());
            existingContainer.RegisterType<IDependency, Dependency>(new ContainerControlledLifetimeManager());
        }

        protected override IUnityContainer CreateRequestContainer()
        {
            this.RequestContainerConfigured = true;
            return base.CreateRequestContainer();
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get { return this.configuration ?? base.InternalConfiguration; }
        }
    }
}
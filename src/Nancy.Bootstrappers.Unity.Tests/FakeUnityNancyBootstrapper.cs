namespace Nancy.BootStrappers.Unity.Tests
{
    using Microsoft.Practices.Unity;
    using Nancy.Bootstrappers.Unity;
    using Nancy.Tests.Fakes;

    public class FakeUnityNancyBootstrapper : UnityNancyBootstrapper
    {
        public bool ApplicationContainerConfigured { get; set; }

        public IUnityContainer Container
        {
            get { return this.ApplicationContainer; }
        }

        public bool RequestContainerConfigured { get; set; }

        protected override void ApplicationStartup(IUnityContainer container, Bootstrapper.IPipelines pipelines)
        {
            RequestContainerConfigured = true;

            container.RegisterType<IFoo, Foo>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDependency, Dependency>(new ContainerControlledLifetimeManager());
        }

        protected override IUnityContainer CreateRequestContainer()
        {
            this.RequestContainerConfigured = true;
            return base.CreateRequestContainer();
        }

        protected override void ConfigureApplicationContainer(IUnityContainer existingContainer)
        {
            ApplicationContainerConfigured = true;
            base.ConfigureApplicationContainer(existingContainer);
        }
    }
}
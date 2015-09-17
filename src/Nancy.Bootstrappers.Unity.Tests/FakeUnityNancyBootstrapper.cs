namespace Nancy.BootStrappers.Unity.Tests
{
    using Bootstrapper;
    using Microsoft.Practices.Unity;
    using Bootstrappers.Unity;
    using Nancy.Tests.Fakes;

    public class FakeUnityNancyBootstrapper : UnityNancyBootstrapper
    {
        private readonly IUnityContainer container;

        private readonly NancyInternalConfiguration configuration;

        public FakeUnityNancyBootstrapper()
            : this(null, null)
        {
        }

        public FakeUnityNancyBootstrapper(NancyInternalConfiguration configuration)
            : this(configuration, null)
        {
        }

        public FakeUnityNancyBootstrapper(IUnityContainer container)
            : this(null, container)
        {
        }

        public FakeUnityNancyBootstrapper(NancyInternalConfiguration configuration, IUnityContainer container)
        {
            this.configuration = configuration;
            this.container = container;
        }

        public bool ApplicationContainerConfigured { get; private set; }

        public bool RequestContainerConfigured { get; private set; }

        protected override IUnityContainer GetApplicationContainer()
        {
            return this.container ?? base.GetApplicationContainer();
        }

        protected override void ApplicationStartup(IUnityContainer container, IPipelines pipelines)
        {
            this.RequestContainerConfigured = true;
        }

        public IUnityContainer Container
        {
            get { return this.ApplicationContainer; }
        }

        protected override void ConfigureApplicationContainer(IUnityContainer existingContainer)
        {
            this.ApplicationContainerConfigured = true;
            base.ConfigureApplicationContainer(existingContainer);

            existingContainer.RegisterType<IFoo, Foo>(new ContainerControlledLifetimeManager());
            existingContainer.RegisterType<IDependency, Dependency>(new ContainerControlledLifetimeManager());
        }

        public IUnityContainer CreateRequestContainer()
        {
            return this.CreateRequestContainer(new NancyContext());
        }

        protected override IUnityContainer CreateRequestContainer(NancyContext context)
        {
            this.RequestContainerConfigured = true;
            return base.CreateRequestContainer(context);
        }

        protected override NancyInternalConfiguration InternalConfiguration
        {
            get { return this.configuration ?? base.InternalConfiguration; }
        }
    }
}
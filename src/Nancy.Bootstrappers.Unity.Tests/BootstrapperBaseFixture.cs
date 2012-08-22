#if !__MonoCS__ 
namespace Nancy.BootStrappers.Unity.Tests
{
    using Bootstrapper;
    using Bootstrappers.Unity;
    using Microsoft.Practices.Unity;
    using Nancy.Tests.Unit.Bootstrapper.Base;

    public class BootstrapperBaseFixture : BootstrapperBaseFixtureBase<IUnityContainer>
    {
        private readonly UnityNancyBootstrapper bootstrapper;

        public BootstrapperBaseFixture()
        {
            this.bootstrapper = new FakeUnityNancyBootstrapper(this.Configuration);
        }

        protected override NancyBootstrapperBase<IUnityContainer> Bootstrapper
        {
            get { return this.bootstrapper; }
        }
    }
}
#endif
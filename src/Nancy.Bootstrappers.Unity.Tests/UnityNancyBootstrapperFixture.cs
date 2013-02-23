namespace Nancy.BootStrappers.Unity.Tests
{
    using System.Linq;
    using Nancy.Tests;
    using Nancy.Tests.Fakes;
    using Xunit;

    public class UnityNancyBootstrapperFixture
    {
        private readonly FakeUnityNancyBootstrapper bootstrapper;

        public UnityNancyBootstrapperFixture()
        {
            this.bootstrapper = new FakeUnityNancyBootstrapper();
            this.bootstrapper.Initialise();
        }

        [Fact]
        public void GetEngine_ReturnsEngine()
        {
            // Given, When
            var result = this.bootstrapper.GetEngine();

            // Then
            result.ShouldNotBeNull();
            result.ShouldBeOfType<INancyEngine>();
        }

        [Fact]
        public void GetAllModules_With_Different_Contexts_Returns_Different_Instances()
        {
            // Given
            this.bootstrapper.GetEngine();
            var context = new NancyContext();
            var context2 = new NancyContext();

            // When
            var output1 = this.bootstrapper.GetAllModules(context).FirstOrDefault(nm => nm.GetType() == typeof(FakeNancyModuleWithBasePath));
            var output2 = this.bootstrapper.GetAllModules(context2).FirstOrDefault(nm => nm.GetType() == typeof(FakeNancyModuleWithBasePath));

            // Then
            output1.ShouldNotBeNull();
            output2.ShouldNotBeNull();
            output1.ShouldNotBeSameAs(output2);
        }

        [Fact]
        public void GetModule_With_Different_Contexts_Returns_Different_Instances()
        {
            // Given
            this.bootstrapper.GetEngine();
            var context = new NancyContext();
            var context2 = new NancyContext();

            // When
            var result = this.bootstrapper.GetModule(typeof(FakeNancyModuleWithDependency), context) as FakeNancyModuleWithDependency;
            var result2 = this.bootstrapper.GetModule(typeof(FakeNancyModuleWithDependency), context2) as FakeNancyModuleWithDependency;

            // Then
            result.FooDependency.ShouldNotBeNull();
            result2.FooDependency.ShouldNotBeNull();
            result.ShouldNotBeSameAs(result2);
        }

        [Fact]
        public void GetAllModules_Configures_Child_Container()
        {
            // Given
            this.bootstrapper.GetEngine();
            this.bootstrapper.RequestContainerConfigured = false;

            // When
            this.bootstrapper.GetAllModules(new NancyContext());

            // Then
            this.bootstrapper.RequestContainerConfigured.ShouldBeTrue();
        }

        [Fact]
        public void GetModule_Configures_Child_Container()
        {
            // Given
            this.bootstrapper.GetEngine();
            this.bootstrapper.RequestContainerConfigured = false;

            // When
            this.bootstrapper.GetModule(typeof(FakeNancyModuleWithBasePath), new NancyContext());

            this.bootstrapper.RequestContainerConfigured.ShouldBeTrue();
        }

        [Fact]
        public void GetEngine_ConfigureApplicationContainer_Should_Be_Called()
        {
            // Given, When
            this.bootstrapper.GetEngine();

            // Then
            this.bootstrapper.ApplicationContainerConfigured.ShouldBeTrue();
        }
    }
}

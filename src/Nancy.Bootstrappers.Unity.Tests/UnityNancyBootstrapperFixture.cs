using System.Collections.Generic;
using Microsoft.Practices.Unity;
using Nancy.ErrorHandling;
using Nancy.ViewEngines;

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

        [Fact]
        public void Container_Should_Resolve_IEnumerable()
        {
            // Given, When
            var statusCodeHandlers = this.bootstrapper.Container.Resolve<IEnumerable<IStatusCodeHandler>>();

            // Then
            statusCodeHandlers.ShouldNotBeNull();
            statusCodeHandlers.Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        public void External_Container_Should_Resolve_IEnumerable()
        {
            // Given
            var container = new UnityContainer();
            var unityBootstrapper = new FakeUnityNancyBootstrapper(container);
            unityBootstrapper.Initialise();

            // When
            var statusCodeHandlers = unityBootstrapper.Container.Resolve<IEnumerable<IStatusCodeHandler>>();

            // Then
            statusCodeHandlers.ShouldNotBeNull();
            statusCodeHandlers.Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Child_Container_Should_Resolve_IEnumerable()
        {
            // Given
            var child = this.bootstrapper.CreateRequestContainer();

            // When
            var statusCodeHandlers = child.Resolve<IEnumerable<IViewEngine>>();

            // Then
            statusCodeHandlers.ShouldNotBeNull();
            statusCodeHandlers.Count().ShouldBeGreaterThan(0);
        }
    }
}

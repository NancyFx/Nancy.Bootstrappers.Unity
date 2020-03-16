using Unity.Builder;
using Unity.Extension;

namespace Nancy.Bootstrappers.Unity
{
    public class EnumerableExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            // Enumerable strategy
            Context.Strategies.Add(new EnumerableResolutionStrategy(), UnityBuildStage.TypeMapping);
        }
    }
}

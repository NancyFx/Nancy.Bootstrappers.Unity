A [bootstrapper](https://github.com/NancyFx/Nancy/wiki/Bootstrapper) implementation, for the [Nancy](http://nancyfx.org) framework, based on the Unity inversion of control container.

## Usage

When Nancy detects that the `UnityNancyBootstrapper` type is available in the AppDomain of your application, it will assume you want to use it, rather than the default one.

The easiest way to get the latest version of `UnityNancyBootstrapper` into your application is to install the `Nancy.Bootstrappers.Unity` nuget.

### Customizing

By inheriting from `UnityNancyBootstrapper` you will gain access to the `IUnityContainer` of the application and request containers and can perform what ever registrations that your application requires.

```c#
public class Bootstrapper : UnityNancyBootstrapper
{
    protected override void ApplicationStartup(IUnityContainer container, IPipelines pipelines)
    {
        // No registrations should be performed in here, however you may
        // resolve things that are needed during application startup.
    }

    protected override void ConfigureApplicationContainer(IUnityContainer existingContainer)
    {
        // Perform registation that should have an application lifetime
    }

    protected override void ConfigureRequestContainer(IUnityContainer container, NancyContext context)
    {
        // Perform registrations that should have a request lifetime
    }

    protected override void RequestStartup(IUnityContainer container, IPipelines pipelines, NancyContext context)
    {
        // No registrations should be performed in here, however you may
        // resolve things that are needed during request startup.
    }
}
```

You can also override the `GetApplicationContainer` method and return a pre-existing container instance, instead of having Nancy create one for you. This is useful if Nancy is co-existing with another application and you want them to share a single container.

It is recommended that a child container of the application's main container is used so that if the application stops and restarts Nancy, the main container is not disposed of.

```c#
protected override IUnityContainer GetApplicationContainer()
{
    // Return application container instance
	return parentContainer.CreateChildContainer();
}
```

## Changes
V2.0.0 supports Unity 5.11.4

## Code of Conduct

This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/) to clarify expected behavior in our community. For more information see the [.NET Foundation Code of Conduct](http://www.dotnetfoundation.org/code-of-conduct).

## Contribution License Agreement

Contributing to Nancy requires you to sign a [contribution license agreement](https://cla2.dotnetfoundation.org/) (CLA) for anything other than a trivial change. By signing the contribution license agreement, the community is free to use your contribution to .NET Foundation projects.

## .NET Foundation

This project is supported by the [.NET Foundation](http://www.dotnetfoundation.org).

## Copyright

Copyright © 2010 Andreas Håkansson, Steven Robbins and contributors

## License

Nancy.Bootstrappers.Unity is licensed under [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form"). Refer to license.txt for more information.

# Adding Support (for Programmers)

!!! note

    The following guidance is for programmers wishing to add update support to their own websites to Reloaded.  

Support for mod updates is provided using the [Sewer56/Update](https://github.com/Sewer56/Update) library.  

Use the following steps to add update support to your website.  

## Preface

Please use the namespace `Reloaded.Mod.Loader.Update.Providers.YOUR_SERVICE_HERE` for your website/service.  

## 1. Write an Update Resolver

Create a package resolver (`IPackageResolver`) by following the guidelines at [wiki:Update/Package Resolvers](https://sewer56.dev/Update/extensibility/package-resolvers/).  

You can find additional examples in the Update library itself.  

## 2. Write a Resolver Factory 

Create a class that implements the `IUpdateResolverFactory` class inside `Reloaded.Mod.Loader.Update.Providers`, returning your `IPackageResolver` via `GetResolver()`. 

**Example:** See `GameBananaUpdateResolverFactory`.

## 3. Register your Resolver Factory

When you are done, update `ResolverFactory.All` to include your new `IUpdateResolverFactory`.
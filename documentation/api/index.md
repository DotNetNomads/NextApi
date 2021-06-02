# NextApi API explorer

## Description

Here you can find definitions for NextApi classes.  
NextApi devided into several packages to help keep a codebase of projects very thin and clean!

## Packages
___NOTE!___ __Names of packages are clickable, you will be redirected to Nuget.Org with search request for the package. You can fetch any package into your project by Nuget, for more information visit the [home page](/).__

This reference contains information about the following packages:

 ### Shareable packages (used between server and client):
  - [__NextApi.Common__](https://www.nuget.org/packages/NextApi.Common/) - shareable logic between client and server.
  - [__NextApi.Server.Common__](https://www.nuget.org/packages/NextApi.Server.Common/) - reusable logic between dependent on server-side logic packages.
  - [__NextApi.UploadQueue.Common__](https://www.nuget.org/packages/NextApi.UploadQueue.Common/) - shareable logic with UploadQueue mechanism basics.
 ### Server-side packages
  - [__NextApi.Testing__](https://www.nuget.org/packages/NextApi.Testing/) - helps developers write integration tests very quickly.
  - [__NextApi.Server__](https://www.nuget.org/packages/NextApi.Server/) - server-side implementation of NextApi.
  - [__NextApi.Server.EfCore__](https://www.nuget.org/packages/NextApi.Server.EfCore/) - the server-side package provides integration with EF Core.
  - [__NextApi.Server.UploadQueue__](https://www.nuget.org/packages/NextApi.Server.UploadQueue/) - server-side logic for UploadQueue mechanism.
 ### Client-side packages
  - [__NextApi.Client__](https://www.nuget.org/packages/NextApi.Client/) - client-side implementation of NextApi.
  - [__NextApi.Client.Autofac__](https://www.nuget.org/packages/NextApi.Client.Autofac/) - extension package helps integrate NextApi client into Autofac DI provider. 
  - [__NextApi.Client.MicrosoftDI__](https://www.nuget.org/packages/NextApi.Client.MicrosoftDI/) - helps integrate NextApi into ASP.NET Core DI provider.
  - [__NextApi.Client.UploadQueue__](https://www.nuget.org/packages/NextApi.Client.UploadQueue/) - UploadQueue implementation for client-side. It's not required  but it helps to simplify interaction with UploadQueue services.

Find more information about NextApi's Nuget packages from [this issue](https://github.com/DotNetNomads/NextApi/issues/12) at Abitech Gitlab.

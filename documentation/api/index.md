# NextApi API explorer

## Description

Here you can find definitions for NextApi classes.  
NextApi devided into several packages to help keep a codebase of projects very thin and clean!

## Packages
___NOTE!___ __Names of packages are clickable, you will be redirected to Abitech Nexus with search request for the package. You can fetch any package into your project by Nuget, for more information visit the [home page](/).__

This reference contains information about the following packages:

 ### Shareable packages (used between server and client):
  - [__Abitech.NextApi.Common__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.Common%20AND%20version%3D%3E1.9) - shareable logic between client and server.
  - [__Abitech.NextApi.Server.Common__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.Server.Common%20AND%20version%3D%3E1.9) - reusable logic between dependent on server-side logic packages.
  - [__Abitech.NextApi.UploadQueue.Common__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.UploadQueue.Common%20AND%20version%3D%3E1.9) - shareable logic with UploadQueue mechanism basics.
 ### Server-side packages
  - [__Abitech.NextApi.Testing__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.Testing%20AND%20version%3D%3E1.9) - helps developers write integration tests very quickly.
  - [__Abitech.NextApi.Server__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.Server%20AND%20version%3D%3E1.9) - server-side implementation of NextApi.
  - [__Abitech.NextApi.Server.EfCore__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.Server.EfCore%20AND%20version%3D%3E1.9) - the server-side package provides integration with EF Core.
  - [__Abitech.NextApi.Server.UploadQueue__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.Server.UploadQueue%20AND%20version%3D%3E1.9) - server-side logic for UploadQueue mechanism.
 ### Client-side packages
  - [__Abitech.NextApi.Client__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.Client%20AND%20version%3D%3E1.9) - client-side implementation of NextApi.
  - [__Abitech.NextApi.Client.Autofac__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.Client.Autofac%20AND%20version%3D%3E1.9) - extension package helps integrate NextApi client into Autofac DI provider. 
  - [__Abitech.NextApi.Client.MicrosoftDI__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.Client.MicrosoftDI%20AND%20version%3D%3E1.9) - helps integrate NextApi into ASP.NET Core DI provider.
  - [__Abitech.NextApi.Client.UploadQueue__](http://nexus.abitech.kz/#browse/search=name.raw%3DAbitech.NextApi.Client.UploadQueue%20AND%20version%3D%3E1.9) - UploadQueue implementation for client-side. It's not required  but it helps to simplify interaction with UploadQueue services.
  
 ### Packages hierarchy or dependencies diagram
<img src="/images/packages-dependency.png" />

Find more information about NextApi's Nuget packages from [this issue](https://gitlab.abitech.kz/development/common/abitech.nextapi/issues/37) at Abitech Gitlab.

# UploadQueue

If you have already taken a look at the NextApi Nuget packages,
you must have noticed some strange names like
[__NextApi.UploadQueue.Common__](https://www.nuget.org/packages/NextApi.UploadQueue.Common/),
[__NextApi.Server.UploadQueue__](https://www.nuget.org/packages/NextApi.Server.UploadQueue/)
and [__NextApi.Client.UploadQueue__](https://www.nuget.org/packages/NextApi.Client.UploadQueue/).

So, what is UploadQueue and what is the purpose of these packages?  
You will find that out in this article!

## Online usage

Most server-client systems use the same principle: store data at
server side in a database and alter that data from clients.
This scheme works perfectly, if the time spent on changing the data
is very low.  
But what if a user tries to change something using his client, goes off to
have a cup of coffee, in the meantime another user changes the
same data, the first user comes back and saves his own changes?  
What will happen is the second user's changes will be overwritten
by the first user's stale data and his changes.  
Let's take a look at an example.

```c#
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string MiddleName { get; set; }
    public string Surname { get; set; }
}
```

Both user's try to edit the same row from the database
`1, Michael, Samuel, Jordan`.  
The first user changes it to `1, Mike, Samuel, Jordan` and goes to
make some coffee.  
The second user tries to change the row that has not yet been saved by the first user,
so he gets `1, Michael, Samuel, Jordan` and changes it to
`1, Michael, Sam, Jordan` and saves the row to the database.  
The first user comes back after his long break and saves his own changes.  
And now, the final row looks like this `1, Mike, Samuel, Jordan`.

See the problem here? To mitigate this, one could implement a simple
timeout, but that would be inconvenient for users and it does not
guarantee that there will be no conflicts at all.

This is where UploadQueue comes in handy!  
Making changes with UQ involves using a simple class
```c#
public class UploadQueueDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EntityRowGuid { get; set; } = default;
    public string EntityName { get; set; }
    public string ColumnName { get; set; }
    public OperationType OperationType { get; set; }
    public object NewValue { get; set; }
    public DateTimeOffset OccuredAt { get; set; }
    public object[] Extras { get; set; }
}
public enum OperationType
{
    None,
    Create,
    Update,
    Delete
}
```

So the example above for the first user would look like this:
```c#
var updateOperation = new UploadQueueDto
{
    Id = Guid.NewGuid(), // operation Id
    EntityRowGuid = entityId, // to use UQ, entities must implement the interface IUploadQueueEntity
    EntityName = nameof(Employee),
    ColumnName = nameof(Employee.Name),
    OperationType = OperationType.Update,
    NewValue = "Mike",
    OccuredAt = DateTimeOffset.Now // the time of occurence
};
// Upload this object using NextApiClient
```

And for the second user:
```c#
var updateOperation = new UploadQueueDto
{
    Id = Guid.NewGuid(), // operation Id
    EntityRowGuid = entityId, // to use UQ, entities must implement the interface IUploadQueueEntity
    EntityName = nameof(Employee),
    ColumnName = nameof(Employee.MiddleName),
    OperationType = OperationType.Update,
    NewValue = "Sam",
    OccuredAt = DateTimeOffset.Now // the time of occurence
};
// Upload this object using NextApiClient
```

This way the final row would be `"GuidId", Mike, Sam, Jordan`.  
___Moreover, if the users change the same column of the same row,
the latest change (based on OccuredAt) will be accepted, no matter
the order of uploading.___

## Offline usage

In the previous section we talked about altering data with a stable internet connection and it does prove to be useful in those scenarios.  
But some systems require offline data access and the time spent on changing the data is even longer and it depends on connection availability. Most synchronization frameworks resolve conflicts simply: either server or client wins.
As you saw, UploadQueue resolves conflicts gracefully, leaving everyone happy, which makes it a good solution for offline applications. UploadQueue was specifically developed to be used in offline applications. Infrastructure.Client is one of those applications.

In Infrastructure.Client we use the [Dotmim.Sync](https://github.com/Mimetis/Dotmim.Sync) synchronization framework to download data and UploadQueue to upload changes accordingly.  
There are some drawbacks of this pattern though:
1. UploadQueueDto operations have to be persisted on the client and applied to the downloaded data everytime it is being queried.
2. After operations are uploaded, they get applied to the server-side database, which is then synchronized with the data at the client side, meaning the same changes are downloaded back to the client, alongside other changes that might have been made by other users.

## Quick tutorial

To be able to upload UQ operations you will have to register UploadQueueService and ColumnChangesLogger alongside other NextApi services. UploadQueueService is the main service that will be handling your UQ operations and ColumnChangesLogger will track per column changes, in order to reject outdated changes. _UploadQueueService will not be able to function without ColumnChangesLogger._

In your Startup.cs file

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddNextApiServices(options =>
        {
            ...

            options.AddUploadQueueService("NextApi.TestServer");
        });
    
    ...

    services.AddColumnChangesLogger<ITestDbContext>();
}
```

"NextApi.TestServer" in .AddUploadQueueService() is the assembly name that contains all the entity models and corresponding IRepo classes of those entities that you will be able to create, update and delete through UploadQueueService. ___Your models must implement the IUploadQueueEntity interface.___

There you go! Now you can upload UQ operations by using NextApiClient or other apps like Postman.

In your C# application:

```c#
var client = new NextApiClient(url, tokenProvider, NextApiTransport.SignalR); // or NextApiTransport.Http
var updateOperation = new UploadQueueDto
{
    Id = Guid.NewGuid(), // operation Id
    EntityRowGuid = entityId, // to use UQ, entities must implement the interface IUploadQueueEntity
    EntityName = nameof(Employee),
    ColumnName = nameof(Employee.Name),
    OperationType = OperationType.Update,
    NewValue = "Mike",
    OccuredAt = DateTimeOffset.Now // the time of occurence
};
var queue = new List<UploadQueueDto> { updateOperation };
var res = await nextApiClient.Invoke<Dictionary<Guid, UploadQueueResult>>(
                        "UploadQueue",
                        "ProcessAsync",
                        new NextApiArgument("uploadQueue", queue));
```
_Or you can use the abstract wrapper class UploadQueueService from NextApi.Client.UploadQueue Nuget package._

The NextApi.UploadQueue.Common Nuget package contains all the shared classes and some useful extension methods, such as ApplyModifications() to apply all the changes to an unchanged freshly queried row.

The NextApi.Server.UploadQueue Nuget package contains everything needed for your ASP.NET server application to enable UQ functionality. UploadQueueService and ColumnChangesLogger are in this package as well.

### Intercepting changes
In some cases you might want to intercept an operation to check on something and possibly reject the change.  
Simply add a class in your project that inherits from UploadQueueChangesHandler<TEntity> class, which has hook methods such as OnBeforeUpdate, OnAfterUpdate and so on. Just throw an exception, if you want to reject a change in one of those methods.  
And in your Startup.cs file:
```c#
services.AddScoped<IUploadQueueChangesHandler<Employee>, EmployeeChangesHandler>();
```
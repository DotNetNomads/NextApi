using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abitech.NextApi.Server.Base;
using Abitech.NextApi.Server.Event;
using Abitech.NextApi.Server.Request;
using Abitech.NextApi.Server.Security;
using Abitech.NextApi.Server.Service;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Server
{
    /// <summary>
    /// Extensions for adding nextapi to app
    /// </summary>
    public static class NextApiExtensions
    {
        /// <summary>
        /// Used to initialize NextApi services
        /// </summary>
        public static IServiceCollection AddNextApiServices(this IServiceCollection serviceCollection,
            Action<NextApiServicesOptions> options = null)
        {
            var nextApiOptions = new NextApiServicesOptions();

            // mvc
            serviceCollection.AddMvc(mvcOptions => mvcOptions.EnableEndpointRouting = false);
            // signalr
            serviceCollection
                .AddSignalR(srOptions => { srOptions.EnableDetailedErrors = true; })
                .AddMessagePackProtocol(mpOptions =>
                {
                    mpOptions.FormatterResolvers = new List<IFormatterResolver>
                    {
                        TypelessContractlessStandardResolver.Instance
                    };
                });

            options?.Invoke(nextApiOptions);
            if (nextApiOptions.DisablePermissionValidation)
            {
                serviceCollection.AddTransient<INextApiPermissionProvider, DisabledNextApiPermissionProvider>();
            }

            serviceCollection.AddSingleton(nextApiOptions);
            serviceCollection.AddScoped<INextApiUserAccessor, NextApiUserAccessor>();
            serviceCollection.AddScoped<INextApiRequest, NextApiRequest>();
            serviceCollection.AddScoped<INextApiEventManager, NextApiEventManager>();
            // handles all requests redirected from HTTP and SignalR
            serviceCollection.AddScoped<NextApiHandler>();
            // handles all request from clients over HTTP
            serviceCollection.AddScoped<NextApiHttp>();
            NextApiServiceHelper
                .FindAllServices()
                .ForEach(type => { serviceCollection.AddTransient(type); });
            return serviceCollection;
        }

        /// <summary>
        /// Add permission provider for NextApi
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddPermissionProvider<T>(this IServiceCollection serviceCollection)
            where T : class, INextApiPermissionProvider
        {
            serviceCollection.AddTransient<INextApiPermissionProvider, T>();
        }

        /// <summary>
        /// Used to register NextApi in SignalR
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        public static void UseNextApiServices(this IApplicationBuilder builder, string path = "/nextApi")
        {
            RegisterHttp(builder, path);
            RegisterSignalR(builder, path);
        }

        private static void RegisterHttp(IApplicationBuilder builder, string path)
        {
            // HTTP
            builder.UseMvc(routes =>
            {
                routes.MapNextApiMethod($"{path}/http",
                    (http, context) => http.ProcessRequestAsync(context));
                routes.MapNextApiMethod($"{path}/http/permissions",
                    (http, context) => http.GetSupportedPermissions(context));
            });
        }

        private static void MapNextApiMethod(this IRouteBuilder routeBuilder, string path,
            Func<NextApiHttp, HttpContext, Task> action)
        {
            var services = routeBuilder.ServiceProvider;
            routeBuilder.MapRoute(path, async context =>
            {
                using (var scope = services.CreateScope())
                {
                    var nextApiHttp = scope.ServiceProvider.GetService<NextApiHttp>();
                    await action.Invoke(nextApiHttp, context);
                }
            });
        }

        private static void RegisterSignalR(IApplicationBuilder builder, string path)
        {
            // SignalR
            builder.UseSignalR(routes => routes.MapHub<NextApiHub>(new PathString(path)));
        }

        /// <summary>
        /// Middleware for parsing access_token from query string and passing them to request header
        /// </summary>
        /// <param name="app">Current application builder</param>
        public static void UseTokenQueryToHeaderFormatter(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]) &&
                    context.Request.Query.Any(q => q.Key == "access_token" && !string.IsNullOrWhiteSpace(q.Value)))
                {
                    var token = context.Request.Query["access_token"];
                    context.Request.Headers.Add("Authorization", new[] {$"Bearer {token}"});
                }

                await next.Invoke();
            });
        }
    }
}

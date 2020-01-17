using System;
using Abitech.NextApi.Server.EfCore.DAL;
using Microsoft.Extensions.DependencyInjection;

namespace Abitech.NextApi.Testing.Data
{
    /// <summary>
    /// Wrapper around DbContext.
    /// Should be used with `using` keyword. Cause DbContext is scoped by default.
    /// </summary>
    /// <typeparam name="TDbContext">DbContext type</typeparam>
    public sealed class DisposableDbContext<TDbContext> : IDisposable where TDbContext : INextApiDbContext
    {
        /// <summary>
        /// Access to DbContext instance
        /// </summary>
        public TDbContext Context { get; }

        private readonly IServiceScope _scope;

        /// <inheritdoc />
        public DisposableDbContext(IServiceScope scope, TDbContext context)
        {
            Context = context;
            _scope = scope;
        }

        /// <inheritdoc />
        public void Dispose() => _scope.Dispose();
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace TodoApi.Tests.Helpers
{
    public class TestScopeFactory : IServiceScopeFactory
    {
        private readonly TodoContext _context;

        public TestScopeFactory(TodoContext context)
        {
            _context = context;
        }

        public IServiceScope CreateScope()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_context);
            return new TestServiceScope(services.BuildServiceProvider());
        }

        private class TestServiceScope : IServiceScope
        {
            public TestServiceScope(IServiceProvider provider)
            {
                ServiceProvider = provider;
            }

            public IServiceProvider ServiceProvider { get; }

            public void Dispose()
            {
                // no-op
            }
        }
    }
}

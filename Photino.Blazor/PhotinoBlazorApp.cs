using Microsoft.Extensions.DependencyInjection;
using PhotinoNET;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photino.Blazor
{
    public class PhotinoBlazorApp : IAsyncDisposable
    {
        /// <summary>
        /// Gets configuration for the service provider.
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Gets configuration for the root components in the window.
        /// </summary>
        public BlazorWindowRootComponents RootComponents { get; private set; }

        internal void Initialize(IServiceProvider services, RootComponentList rootComponents)
        {
            Services = services;
            RootComponents = Services.GetService<BlazorWindowRootComponents>();
            MainWindow = Services.GetService<PhotinoWindow>();
            WindowManager = Services.GetService<PhotinoWebViewManager>();

            MainWindow
                .SetTitle("Photino.Blazor App")
                .SetUseOsDefaultLocation(false)
                .SetWidth(1000)
                .SetHeight(900)
                .SetLeft(450)
                .SetTop(100);

            MainWindow.RegisterCustomSchemeHandler(PhotinoWebViewManager.BlazorAppScheme, HandleWebRequest);

            foreach (var component in rootComponents)
            {
                RootComponents.Add(component.Item1, component.Item2);
            }
        }

        public PhotinoWindow MainWindow { get; private set; }

        public PhotinoWebViewManager WindowManager { get; private set; }

        public void Run()
        {
            if (string.IsNullOrWhiteSpace(MainWindow.StartUrl))
                MainWindow.StartUrl = "/";

            WindowManager.Navigate(MainWindow.StartUrl);
            MainWindow.WaitForClose();
        }

        public Stream HandleWebRequest(object sender, string scheme, string url, out string contentType)
                => WindowManager.HandleWebRequest(sender, scheme, url, out contentType!)!;

        private bool _disposed;

        public virtual async ValueTask DisposeAsyncCore()
        {
            if (!_disposed)
            {
                _disposed = true;

                // We explicitly dispose WebviewManager first 
                await WindowManager.DisposeAsync();
                // Now dispose all services in DI
                await DisposeAsync(Services);

                static async ValueTask DisposeAsync(object o)
                {
                    switch (o)
                    {
                        case IAsyncDisposable asyncDisposable:
                            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                            break;
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                    }
                }
            }
        }
        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await DisposeAsyncCore();
        }
    }
}

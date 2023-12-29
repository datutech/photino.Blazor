using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;

namespace Photino.Blazor.Sample
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);

            appBuilder.Services
                .AddSingleton<DisposableService>()
                .AddSingleton<AsyncDisposableService>()
                .AddLogging();

            // register root component and selector
            appBuilder.RootComponents.Add<App>("app");

            var app = appBuilder.Build();

            // customize window
            app.MainWindow
                .SetIconFile("favicon.ico")
                .SetTitle("Photino Blazor Sample");
            app.MainWindow.WindowClosing += (sender, args) =>
            {
                return false;
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
            {
                return;
            };
            app.Run();
        }
    }
}

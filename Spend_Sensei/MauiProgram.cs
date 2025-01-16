using Microsoft.Extensions.Logging;
using MudBlazor.Services; // Import MudBlazor
using Spend_Sensei.Services; // Import your services

namespace Spend_Sensei;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add Blazor WebView
        builder.Services.AddMauiBlazorWebView();

        // Add MudBlazor services
        builder.Services.AddMudServices();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Register application services
        builder.Services.AddSingleton<TransactionService>();

        return builder.Build();
    }
}
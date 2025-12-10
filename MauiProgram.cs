using CommunityToolkit.Maui;
using QrCode_Reader.Data;
using QrCode_Reader.Views;
using ZXing.Net.Maui.Controls;

namespace QrCode_Reader;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader();

        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "delivery.db3");
        builder.Services.AddSingleton(new LocalDatabase(dbPath));
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<ScannerPage>();
        builder.Services.AddTransient<ListPage>();
        return builder.Build();
    }
}

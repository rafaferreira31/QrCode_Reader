using CommunityToolkit.Maui;
using QrCode_Reader.Data;
using CommunityToolkit.Maui;

namespace QrCode_Reader
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().UseMauiCommunityToolkit();
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "delivery.db3");
            builder.Services.AddSingleton(new LocalDatabase(dbPath));
            builder.Services.AddSingleton<Views.ScannerPage>();
            return builder.Build();
        }
    }
}
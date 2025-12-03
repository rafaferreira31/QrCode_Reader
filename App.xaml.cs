using Microsoft.Extensions.DependencyInjection;
using QrCode_Reader.Views;

namespace QrCode_Reader
{
    public partial class App : Application
    {
        public App(ScannerPage page)
        {
            InitializeComponent();
            MainPage = new NavigationPage(page);
        }
    }
}
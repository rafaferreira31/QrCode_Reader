using QrCode_Reader.Data;
using QrCode_Reader.Views;

namespace QrCode_Reader;

public partial class AppShell : Shell
{
    private readonly LocalDatabase _db;

    public AppShell(LocalDatabase db)
	{
		InitializeComponent();

        _db = db;

        ConfigureShell();
    }

    private void ConfigureShell()
    {
        var scanner = new ScannerPage(_db);
        var list = new ListPage(_db);
        


        Items.Add(new TabBar
        {
            Items =
                {
                    new ShellContent { Title = "Scan", Icon ="scan", Content = scanner },
                    new ShellContent { Title = "List", Icon ="list", Content = list }
                }

        }
        );
    }
}
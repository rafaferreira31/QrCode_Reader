using QrCode_Reader.Data;
using QrCode_Reader.Helpers;

namespace QrCode_Reader.Views;

public partial class ScannerPage : ContentPage
{
    private readonly LocalDatabase _db;

    public ScannerPage(LocalDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    private async void Simulate_Clicked(object sender, EventArgs e)
    {
        string qr = "UNID08059";
        var id = QrHelper.ExtractClientId(qr);
        if (id != null)
        {
            await _db.MarkAsDeliveredAsync(id.Value);
            await DisplayAlert("OK", "Entregue!", "Fechar");
        }
    }

    private async void List_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListPage(_db));
    }

    private async void Import_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ImportPage(_db));
    }
}
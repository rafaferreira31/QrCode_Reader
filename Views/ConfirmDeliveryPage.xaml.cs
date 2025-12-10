using QrCode_Reader.Data;
using QrCode_Reader.Models;

namespace QrCode_Reader.Views;

public partial class ConfirmDeliveryPage : ContentPage
{
    private readonly LocalDatabase _db;
    private int _clientId;

    public ConfirmDeliveryPage(LocalDatabase db, Client client)
    {
        InitializeComponent();
        _db = db;
        _clientId = client.Id;

        NameLabel.Text = client.Name;
        IdLabel.Text = client.Id.ToString();
    }

    private async void Confirm_Clicked(object sender, EventArgs e)
    {
        await _db.MarkAsDeliveredAsync(_clientId);

        Navigation.RemovePage(this);
    }
}
using QrCode_Reader.Data;
using QrCode_Reader.Models;

namespace QrCode_Reader.Views;

public partial class ConfirmDeliveryPage : ContentPage
{
    private readonly LocalDatabase _db;
    private Client _client;

    public ConfirmDeliveryPage(LocalDatabase db, Client client)
    {
        InitializeComponent();

        _db = db;
        _client = client;

        NameLabel.Text = client.FullName;
        IdLabel.Text = client.UNID;
    }


    private async void Confirm_Clicked(object sender, EventArgs e)
    {
        _client.Delivered = true;
        _client.DeliverDate = DateTime.Now;
        _client.DeliveryNote = NoteEditor.Text;

        await _db.UpdateClientAsync(_client);

        await Navigation.PopAsync();
    }
}
using QrCode_Reader.Models;

namespace QrCode_Reader.Views;

public partial class ClientDetailsPage : ContentPage
{
    public ClientDetailsPage(Client client)
    {
        InitializeComponent();
        BindingContext = client;

        if(!client.Delivered)
        {
            DeliverDate.Text = "Not Delivered";
        }
        else
        {
            DeliverDate.Text = $"Delivered at\n{client.DeliverDate:dd/MM/yyyy HH:mm}";
        }

        NoteLabel.IsVisible = !string.IsNullOrWhiteSpace(client.DeliveryNote);
    }
}
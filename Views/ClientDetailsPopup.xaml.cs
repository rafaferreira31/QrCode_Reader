using CommunityToolkit.Maui.Views;
using QrCode_Reader.Data;
using QrCode_Reader.Models;

namespace QrCode_Reader.Views;

public partial class ClientDetailsPopup : Popup
{
    public Client Client { get; }
    private readonly LocalDatabase _db;

    private bool _isEditing;

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (_isEditing == value) return;
            _isEditing = value;
            OnPropertyChanged(nameof(IsEditing));
        }
    }

    private Client _backupClient;

    public ClientDetailsPopup(Client client, LocalDatabase db)
    {
        InitializeComponent();
        Client = client;
        _db = db;

        BindingContext = this;

        _backupClient = new Client
        {
            Delivered = client.Delivered,
            DeliveryNote = client.DeliveryNote
        };

       

        IsEditing = false;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await CloseAsync();
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        IsEditing = true;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            await _db.UpdateClientAsync(Client);

            IsEditing = false;
            await CloseAsync();
        }
        catch
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                "Could not save changes",
                "OK");
        }
    }

    private void OnCancelEditClicked(object sender, EventArgs e)
    {
        Client.Delivered = _backupClient.Delivered;
        Client.DeliveryNote = _backupClient.DeliveryNote;

        IsEditing = false;
    }

    private void Delivered_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            // Se marcou como entregue e ainda não tem data → define agora
            if (Client.DeliverDate == null)
                Client.DeliverDate = DateTime.Now;
        }
        else
        {
            // Opcional: se desmarcar, remove a data
            Client.DeliverDate = null;
        }

        // força atualização da UI
        OnPropertyChanged(nameof(Client));
    }

}
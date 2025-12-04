using QrCode_Reader.Data;
using QrCode_Reader.Helpers;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using System.Linq;

namespace QrCode_Reader.Views;

public partial class ScannerPage : ContentPage
{
    private readonly LocalDatabase _db;
    private bool _isProcessing;

    public ScannerPage(LocalDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    // ✅ Leitura REAL de QR
    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing)
            return;

        _isProcessing = true;

        var qrValue = e.Results.FirstOrDefault()?.Value;

        if (string.IsNullOrWhiteSpace(qrValue))
        {
            _isProcessing = false;
            return;
        }

        var clientId = QrHelper.ExtractClientId(qrValue);

        if (clientId == null)
        {
            await Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlert("QR inválido", qrValue, "Fechar");
            });

            _isProcessing = false;
            return;
        }

        await _db.MarkAsDeliveredAsync(clientId.Value);

        await Dispatcher.DispatchAsync(async () =>
        {
            ResultLabel.Text = $"Entregue: UNID{clientId}";
            await DisplayAlert("Sucesso",
                $"Entrega confirmada para UNID{clientId}",
                "OK");
        });

        await Task.Delay(1500); // tempo entre leituras
        _isProcessing = false;
    }

    // ✅ Simulação para PC/Emulador
    private async void Simulate_Clicked(object sender, EventArgs e)
    {
        string fakeQr = "UNID08059";

        var id = QrHelper.ExtractClientId(fakeQr);

        if (id != null)
        {
            await _db.MarkAsDeliveredAsync(id.Value);
            await DisplayAlert("Simulação", "Entrega marcada!", "OK");
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

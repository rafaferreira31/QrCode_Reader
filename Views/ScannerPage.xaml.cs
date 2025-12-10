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
            return; // Evita processar múltiplos scans simultâneos

        _isProcessing = true;

        try
        {
            var qrValue = e.Results.FirstOrDefault()?.Value;

            if (string.IsNullOrWhiteSpace(qrValue))
                return;

            var clientId = QrHelper.ExtractClientId(qrValue);

            if (clientId == null)
            {
                await Dispatcher.DispatchAsync(async () =>
                {
                    await DisplayAlert("QR inválido", qrValue, "Fechar");
                });
                return;
            }

            // Buscar cliente na base de dados
            var client = await _db.GetClientByIdAsync(clientId.Value);

            if (client == null)
            {
                await Dispatcher.DispatchAsync(async () =>
                {
                    await DisplayAlert("Erro", "Cliente não encontrado!", "OK");
                });
                return;
            }

            if (client.Delivered)
            {
                await Dispatcher.DispatchAsync(async () =>
                {
                    await DisplayAlert("Info", "Entrega já foi confirmada para este cliente.", "OK");
                });
                return;
            }

            // Navegar para a página de confirmação
            await Dispatcher.DispatchAsync(async () =>
            {
                await Navigation.PushAsync(new ConfirmDeliveryPage(_db, client));
            });
        }
        catch (Exception ex)
        {
            // Log do erro ou alerta para o usuário
            await Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlert("Erro inesperado", ex.Message, "OK");
            });
        }
        finally
        {
            _isProcessing = false; // Libera o scanner
        }
    }

    private async void List_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListPage(_db));
    }

}

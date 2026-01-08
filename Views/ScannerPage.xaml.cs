using QrCode_Reader.Data;
using QrCode_Reader.Helpers;
using ZXing.Net.Maui;

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

    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_isProcessing)
            return;

        _isProcessing = true;

        try
        {
            var qrValue = e.Results.FirstOrDefault()?.Value;

            if (string.IsNullOrWhiteSpace(qrValue))
            {
                _isProcessing = false;
                return;
            }

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);

            var clientId = QrHelper.ExtractClientId(qrValue);

            // Pausar scanner antes do processamento
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                cameraView.IsDetecting = false;
            });

            // --- QR INVÁLIDO ---
            if (clientId == null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlertAsync("QR inválido", qrValue, "Fechar");

                    // Reativar o scanner
                    cameraView.IsDetecting = true;
                    _isProcessing = false;
                });

                return;
            }

            var client = await _db.GetClientByIdAsync(clientId.Value);

            // --- CLIENTE NÃO ENCONTRADO ---
            if (client == null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlertAsync("Erro", "Cliente não encontrado!", "OK");

                    cameraView.IsDetecting = true;
                    _isProcessing = false;
                });
                return;
            }

            // --- ENTREGA JÁ CONFIRMADA ---
            if (client.Delivered)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlertAsync("Info", "Entrega já foi confirmada para este cliente.", "OK");

                    cameraView.IsDetecting = true;
                    _isProcessing = false;
                });
                return;
            }

            // --- CASO VÁ PARA A PÁGINA DE CONFIRMAÇÃO ---
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PushAsync(new ConfirmDeliveryPage(_db, client));
            });
        }
        finally
        {
            // Não reativa automaticamente aqui
            // Para evitar dupla leitura durante navegação
        }
    }



    protected override void OnAppearing()
    {
        base.OnAppearing();

        _isProcessing = false;
        cameraView.IsDetecting = true; // Retoma leitura
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        cameraView.IsDetecting = false; // Garante pausa
    }


    //Tamanho do scanner dinâmico
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        double square = Math.Min(width - 40, height - 200) * 0.7;
        scannerBorder.WidthRequest = square;
        scannerBorder.HeightRequest = square;
    }

    private async void List_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListPage(_db));
    }
}

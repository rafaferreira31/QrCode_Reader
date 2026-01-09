using QrCode_Reader.Data;
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

    /// <summary>
    /// Handles the event that occurs when one or more barcodes are detected by the camera, processing the detected QR
    /// code and initiating delivery confirmation if applicable.
    /// </summary>
    /// <remarks>This method temporarily pauses barcode detection while processing a detected QR code to
    /// prevent duplicate scans. If the QR code is invalid, the client is not found, or delivery has already been
    /// confirmed, an appropriate alert is displayed and scanning is re-enabled. If a valid, undelivered client is
    /// found, the user is navigated to the delivery confirmation page. This method is intended to be used as an event
    /// handler for barcode detection events.</remarks>
    /// <param name="sender">The source of the event, typically the camera view control that detected the barcodes.</param>
    /// <param name="e">A BarcodeDetectionEventArgs object that contains the event data, including the detected barcode results.</param>
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


            //var clientId = QrHelper.ExtractClientId(qrValue);

            // Pause scanner before processing.
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                cameraView.IsDetecting = false;
            });

            // --- INVALID QR ---
            if (qrValue == null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await DisplayAlertAsync("QR inválido", qrValue, "Fechar");

                    // Reactivate the scanner
                    cameraView.IsDetecting = true;
                    _isProcessing = false;
                });

                return;
            }

            var client = await _db.GetClientByIdAsync(qrValue);

            // --- CLIENT NOT FOUND ---
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

            // --- DELIVERY ALREADY CONFIRMED ---
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

            // --- IF IT GOES TO THE CONFIRMATION PAGE ---
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PushAsync(new ConfirmDeliveryPage(_db, client));
            });
        }
        finally
        {
            // It does not automatically reactivate here
            // To avoid double reading during navigation
        }
    }


    /// <summary>
    /// Called when the page appears on the screen. Resumes camera detection and resets processing state.
    /// </summary>
    /// <remarks>Override this method to perform actions each time the page becomes visible. This method
    /// ensures that camera detection is active when the page is shown.</remarks>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        _isProcessing = false;
        cameraView.IsDetecting = true; // Resume reading
    }
    
    /// <summary>
    /// Handles logic that should occur when the page is about to disappear from view.
    /// </summary>
    /// <remarks>This method stops camera detection when the page is no longer visible. Override this method
    /// to add additional cleanup or state management when the page disappears. Always call the base implementation to
    /// ensure proper behavior.</remarks>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        cameraView.IsDetecting = false; // Ensure pause
    }



    /// <summary>
    /// Handles changes to the size of the element by allocating space for its content.
    /// </summary>
    /// <remarks>Override this method to adjust layout or perform actions when the element's size changes.
    /// This method is called during the layout cycle after the size has been determined.</remarks>
    /// <param name="width">The new width of the element, in device-independent units.</param>
    /// <param name="height">The new height of the element, in device-independent units.</param>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        double square = Math.Min(width - 40, height - 200) * 0.7;
        scannerBorder.WidthRequest = square;
        scannerBorder.HeightRequest = square;
    }

    /// <summary>
    /// Handles the click event for the list navigation control and navigates to the list page.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control that was clicked.</param>
    /// <param name="e">An object that contains the event data.</param>
    private async void List_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListPage(_db));
    }
}

using QrCode_Reader.Data;
using QrCode_Reader.Models;
using System.Text;

namespace QrCode_Reader.Views;

public partial class ListPage : ContentPage
{
    private readonly LocalDatabase _db;
    private List<Client> allClients;
    private bool _isSearchVisible;

    public ListPage(LocalDatabase db)
    {
        InitializeComponent();
        _db = db;

        SearchBar.IsVisible = false;
    }

    /// <summary>
    /// Handles logic that should occur when the page appears on screen.
    /// </summary>
    /// <remarks>Overrides the base implementation to perform additional actions, such as refreshing data,
    /// each time the page becomes visible. This method is typically called by the framework and should not be invoked
    /// directly.</remarks>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadListAsync();
    }



    // =========================
    // IMPORT
    // =========================

    /// <summary>
    /// Handles the import action when the import button is clicked, allowing the user to select a CSV file and import
    /// its contents as a new project.
    /// </summary>
    /// <remarks>If the user cancels the file selection dialog, the import operation is not performed.
    /// Displays a success or error message based on the outcome of the import.</remarks>
    /// <param name="sender">The source of the event, typically the import button.</param>
    /// <param name="e">An EventArgs object that contains the event data.</param>
    private async void ImportClicked(object sender, EventArgs e)
    {
        var file = await FilePicker.PickAsync();

        if (file == null)
            return;

        try
        {
            await _db.ImportCsvAsNewProjectAsync(file.FullPath, file.FileName);
            await ReloadListAsync();
            await DisplayAlertAsync("OK", "Sucessfully Import!", "Close");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error Importing",
                "Selected csv Not Valid",
                "OK");
        }

        //FOR TESTING DATABASE ONLY
        // await _db.CopyDatabaseToDownloadsAsync();
    }



    // =========================
    // EXPORT
    // =========================

    /// <summary>
    /// Handles the export operation by generating a CSV file containing all client data and saving it to the device's
    /// storage.
    /// </summary>
    /// <remarks>If there are no clients to export, a warning message is displayed and no file is created. On
    /// Android devices, the file is saved to the Downloads folder; on other platforms, it is saved to the application's
    /// data directory. The method requests storage permissions on Android devices running versions earlier than
    /// 13.</remarks>
    /// <param name="sender">The source of the event, typically the export button.</param>
    /// <param name="e">An object that contains the event data.</param>
    private async void ExportClicked(object sender, EventArgs e)
    {
        var clients = await _db.GetAllClientsAsync();

        if (!clients.Any())
        {
            await DisplayAlertAsync("Warning", "No data to export.", "OK");
            return;
        }

        var csv = new StringBuilder();
        csv.AppendLine("UNID,NOME,COGNOME,DELIVERED,DATE,NOTE");

        foreach (var c in clients)
        {
            csv.AppendLine($"{c.UNID},{c.Name},{c.LastName},{c.Delivered},{c.DeliverDate},{c.DeliveryNote}");
        }
        var fileName = $"clientes_{DateTime.Now:yyyyMMdd_HHmm}.csv";

#if ANDROID
        string downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(
            Android.OS.Environment.DirectoryDownloads).AbsolutePath;
        var path = Path.Combine(downloadsPath, fileName);

        // Necessário para Android < 13
        if ((int)Android.OS.Build.VERSION.SdkInt < 33)
        {
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlertAsync("Permissão", "Não foi possível acessar a pasta de Downloads.", "OK");
                return;
            }
        }
#else
        var path = Path.Combine(FileSystem.AppDataDirectory, fileName);
#endif

        File.WriteAllText(path, csv.ToString());
        await DisplayAlertAsync("Sucess", $"Arquivo salvo em: {path}", "OK");

        //FOR TESTING DATABASE ONLY
        //await _db.CopyDatabaseToDownloadsAsync();
    }



    // =========================
    // RELOAD LIST
    // =========================

    /// <summary>
    /// Asynchronously reloads the list of clients from the database and updates the list view's data source.
    /// </summary>
    /// <remarks>Call this method to refresh the displayed client list to reflect the latest data from the
    /// database. This method should be awaited to ensure the list is updated before performing further actions that
    /// depend on the refreshed data.</remarks>
    /// <returns>A task that represents the asynchronous reload operation.</returns>
    private async Task ReloadListAsync()
    {
        allClients = await _db.GetAllClientsAsync();
        List.ItemsSource = allClients;
    }



    // =========================
    // OPEN / CLOSE SEARCH
    // =========================

    /// <summary>
    /// Handles the toggling of the search bar's visibility in response to a user action.
    /// </summary>
    /// <param name="sender">The source of the event that triggered the toggle action.</param>
    /// <param name="e">An object that contains the event data.</param>
    private async void OnSearchToggle(object sender, EventArgs e)
    {
        if (!_isSearchVisible)
        {
            SearchBar.IsVisible = true;
            SearchIcon.IsEnabled = false;
            Overlay.IsVisible = true;

            await Task.WhenAll(
                SearchBar.TranslateToAsync(0, 0, 220, Easing.CubicOut),
                SearchBar.FadeToAsync(1, 220)
            );

            SearchBar.Focus();
            _isSearchVisible = true;
        }
        else
        {
            await CloseSearchAsync();
        }
    }


    /// <summary>
    /// Asynchronously closes the search bar UI and resets the search state to its default values.
    /// </summary>
    /// <remarks>This method hides the search bar, clears its text, restores the original client list, and
    /// re-enables related UI elements. It should be awaited to ensure the UI is fully updated before performing further
    /// actions.</remarks>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    private async Task CloseSearchAsync()
    {
        await Task.WhenAll(
        SearchBar.TranslateToAsync(-300, 0, 200, Easing.CubicIn),
        SearchBar.FadeToAsync(0, 200)
    );

        SearchBar.IsVisible = false;
        SearchBar.Text = string.Empty;
        SearchBar.Unfocus();
        SearchIcon.IsEnabled = true;

        Overlay.IsVisible = false;
        _isSearchVisible = false;

        List.ItemsSource = allClients;
    }



    // =========================
    // CLOSE SEARCH ON OUTSIDE TAP
    // =========================

    /// <summary>
    /// Handles the event when a tap occurs outside the search area and closes the search interface if it is currently
    /// visible.
    /// </summary>
    /// <param name="sender">The source of the event, typically the UI element that was tapped.</param>
    /// <param name="e">An object that contains the event data.</param>
    private async void OnOutsideTapped(object sender, EventArgs e)
    {
        if (_isSearchVisible)
        {
            await CloseSearchAsync();
        }
    }



    // =========================
    // TEXT FILTERING
    // =========================

    /// <summary>
    /// Handles the event that occurs when the search text is changed, updating the displayed client list to match the
    /// current filter.
    /// </summary>
    /// <param name="sender">The source of the event, typically the search input control.</param>
    /// <param name="e">The event data containing information about the changed text.</param>
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (allClients == null || !allClients.Any())
            return;

        var text = e.NewTextValue?.ToLower();

        if (string.IsNullOrWhiteSpace(text))
        {
            List.ItemsSource = allClients;
            return;
        }

        List.ItemsSource = allClients
            .Where(c => !string.IsNullOrEmpty(c.Name) &&
                        c.Name.ToLower().Contains(text))
            .ToList();
    }



    // =========================
    // CLIENT DETAILS
    // =========================

    /// <summary>
    /// Handles the selection changed event for the client list and navigates to the details page for the selected
    /// client.
    /// </summary>
    /// <remarks>If no client is selected, the method returns without performing any action. After navigating
    /// to the client details page, the selection in the list is cleared to prevent repeated navigation when returning
    /// to the list.</remarks>
    /// <param name="sender">The source of the event, typically the client list control.</param>
    /// <param name="e">A SelectionChangedEventArgs object that contains data about the selection change event.</param>
    private async void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Client selectedClient)
            return;

        // Remove visual selection
        List.SelectedItem = null;

        await Navigation.PushAsync(new ClientDetailsPage(selectedClient));
    }
}

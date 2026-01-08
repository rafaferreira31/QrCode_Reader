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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ReloadListAsync();
    }

    // =========================
    // IMPORT
    // =========================
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
    }


    // =========================
    // EXPORT
    // =========================
    private async void ExportClicked(object sender, EventArgs e)
    {
        var clients = await _db.GetAllClientsAsync();

        if (!clients.Any())
        {
            await DisplayAlertAsync("Warning", "No data to export.", "OK");
            return;
        }

        var csv = new StringBuilder();
        csv.AppendLine("Id,Name,Delivered,DeliveryNote");

        foreach (var c in clients)
        {
            csv.AppendLine($"{c.Id},{c.Name},{c.Delivered},{c.DeliveryNote}");
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
    }


    // =========================
    // RELOAD LIST
    // =========================
    private async Task ReloadListAsync()
    {
        allClients = await _db.GetAllClientsAsync();
        List.ItemsSource = allClients;
    }

    // =========================
    // OPEN / CLOSE SEARCH
    // =========================
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
    private async void List_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Client selectedClient)
            return;

        // Remove seleção visual
        List.SelectedItem = null;

        await Navigation.PushAsync(new ClientDetailsPage(selectedClient));
    }
}

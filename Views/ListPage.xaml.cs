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
    // IMPORTAR
    // =========================
    private async void ImportClicked(object sender, EventArgs e)
    {
        var file = await FilePicker.PickAsync();

        if (file != null)
        {
            await _db.ImportCsvAsNewProjectAsync(file.FullPath, file.FileName);
            await ReloadListAsync();
            await DisplayAlert("OK", "Importado com sucesso!", "Fechar");
        }
    }

    // =========================
    // EXPORTAR
    // =========================
    private async void ExportClicked(object sender, EventArgs e)
    {
        var clients = await _db.GetAllClientsAsync();

        if (!clients.Any())
        {
            await DisplayAlert("Aviso", "Não há dados para exportar.", "OK");
            return;
        }

        var csv = new StringBuilder();
        csv.AppendLine("Id,Name,Delivered");

        foreach (var c in clients)
        {
            csv.AppendLine($"{c.Id},{c.Name},{c.Delivered}");
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
                await DisplayAlert("Permissão", "Não foi possível acessar a pasta de Downloads.", "OK");
                return;
            }
        }
#else
        var path = Path.Combine(FileSystem.AppDataDirectory, fileName);
#endif

        File.WriteAllText(path, csv.ToString());
        await DisplayAlert("Sucesso", $"Arquivo salvo em: {path}", "OK");
    }


    // =========================
    // RECARREGAR LISTA
    // =========================
    private async Task ReloadListAsync()
    {
        allClients = await _db.GetAllClientsAsync();
        List.ItemsSource = allClients;
    }

    // =========================
    // ABRIR / FECHAR PESQUISA
    // =========================
    private async void OnSearchToggle(object sender, EventArgs e)
    {
        if (!_isSearchVisible)
        {
            SearchBar.IsVisible = true;
            SearchIcon.IsEnabled = false;

            await Task.WhenAll(
                SearchBar.TranslateTo(0, 0, 220, Easing.CubicOut),
                SearchBar.FadeTo(1, 220)
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
            SearchBar.TranslateTo(-300, 0, 200, Easing.CubicIn),
            SearchBar.FadeTo(0, 200)
        );

        SearchBar.IsVisible = false;
        SearchBar.Text = string.Empty;
        SearchBar.Unfocus();
        SearchIcon.IsEnabled = true;

        _isSearchVisible = false;

        List.ItemsSource = allClients;
    }

    // =========================
    // FECHAR AO TOCAR FORA
    // =========================
    private async void OnOutsideTapped(object sender, EventArgs e)
    {
        if (_isSearchVisible)
        {
            await CloseSearchAsync();
        }
    }



    // =========================
    // FILTRAR TEXTO
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
}

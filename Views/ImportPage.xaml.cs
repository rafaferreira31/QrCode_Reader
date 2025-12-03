using QrCode_Reader.Data;

namespace QrCode_Reader.Views;

public partial class ImportPage : ContentPage
{
    private readonly LocalDatabase _db;

    public ImportPage(LocalDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    private async void Pick_Clicked(object sender, EventArgs e)
    {
        var file = await FilePicker.PickAsync();
        if (file != null)
        {
            await _db.ImportCsvAsNewProjectAsync(file.FullPath, file.FileName);
            await DisplayAlert("OK", "Importado com sucesso!", "Fechar");
        }
    }
}
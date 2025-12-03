using QrCode_Reader.Data;

namespace QrCode_Reader.Views;

public partial class ListPage : ContentPage
{
    private readonly LocalDatabase _db;

    public ListPage(LocalDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        List.ItemsSource = await _db.GetAllClientsAsync();
    }
}

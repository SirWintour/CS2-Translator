using Avalonia.Controls;
using CS2.Translator.UI.ViewModels;

namespace CS2.Translator.UI.Views;

public partial class MainView : UserControl
{
    private MainView()
    {
        InitializeComponent();
    }

    public MainView(MainViewModel vm) : this()
    {
        DataContext = vm;
    }
}
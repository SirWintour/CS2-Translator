using Avalonia.Controls;
using CS2.Translator.UI.ViewModels;

namespace CS2.Translator.UI.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        vm.CloseRequested += Close;
    }
}
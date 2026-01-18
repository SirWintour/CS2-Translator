using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Threading;
using CS2.Translator.UI.ViewModels;

namespace CS2.Translator.UI.Views;

public partial class MainView : UserControl
{
    private MainView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }
    public MainView(MainViewModel vm) : this()
    {
        DataContext = vm;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        vm.Chats.CollectionChanged += Chats_CollectionChanged;
    }

    private void Chats_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ChatScrollViewer.ScrollToHome();
        }, DispatcherPriority.Background);
    }
}
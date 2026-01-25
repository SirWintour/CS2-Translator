using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using CS2.Translator.UI.ViewModels;
using CS2.Translator.Core.Helper;

namespace CS2.Translator.UI.Views;

public partial class MainView : UserControl
{
    private MainView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DebugLog("MainView constructor initialized");
    }
    public MainView(MainViewModel vm) : this()
    {
        DataContext = vm;
        DebugLog("MainView initialized with ViewModel");
    }
    private static void DebugLog(string msg)
    {
        string formatted = $"[MainView] | {msg}";
        Console.WriteLine(formatted);
        DebugLogger.Log(formatted);
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            DebugLog("MainView loaded event triggered");

            if (DataContext is not MainViewModel vm)
            {
                DebugLog("DataContext is not MainViewModel → no binding");
                return;
            }

            vm.Chats.CollectionChanged += Chats_CollectionChanged;
            vm.PropertyChanged += Vm_PropertyChanged;
            DebugLog("Subscribed to Chats.CollectionChanged event");
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "OnLoaded");
        }
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(MainViewModel.NameFontSize) ||
                e.PropertyName == nameof(MainViewModel.TranslationFontSize))
            {
                DebugLog($"Font size changed → {e.PropertyName}");

                Dispatcher.UIThread.Post(() =>
                {
                    try
                    {
                        // force UI refresh
                        InvalidateVisual();
                        DebugLog("UI invalidated due to font size change");
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.LogException(ex, "InvalidateVisual");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "Vm_PropertyChanged");
        }
    }

    private void Chats_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        try
        {
            DebugLog($"Chats_CollectionChanged invoked → Action={e.Action}, NewItems={e.NewItems?.Count ?? 0}, OldItems={e.OldItems?.Count ?? 0}");

            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    ChatScrollViewer.ScrollToHome();
                    DebugLog("ChatScrollViewer.ScrollToHome executed");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogException(ex, "ScrollToHome");
                }
            }, DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "Chats_CollectionChanged");
        }
    }
}
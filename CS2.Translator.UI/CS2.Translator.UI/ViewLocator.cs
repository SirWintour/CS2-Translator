using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CS2.Translator.UI.ViewModels;
using CS2.Translator.Core.Helper;

namespace CS2.Translator.UI;

public class ViewLocator : IDataTemplate
{
    private static void DebugLog(string msg)
    {
        string formatted = $"[ViewLocator] | {msg}";
        Console.WriteLine(formatted);
        DebugLogger.Log(formatted);
    }
    public Control Build(object? data)
    {
        try
        {
            if (data is null)
            {
                DebugLog("Build() called with null data");
                return new TextBlock { Text = "null" };
            }

            var viewModelType = data.GetType();
            var viewName = viewModelType.FullName!.Replace("ViewModel", "View");
            DebugLog($"Resolving view for: {viewModelType.FullName} - {viewName}");

            var viewType = Type.GetType(viewName);

            if (viewType != null)
            {
                DebugLog($"Found view type: {viewType.FullName}");
                var view = (Control)Activator.CreateInstance(viewType)!;
                DebugLog($"Created instance of {viewType.Name}");
                return view;
            }
            else
            {
                DebugLog($"⚠️ View not found for {viewName}");
                return new TextBlock { Text = $"View not found: {viewName}" };
            }
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "ViewLocator.Build");
            return new TextBlock { Text = $"Error loading view: {ex.Message}" };
        }
    }

    public bool Match(object? data)
    {
        var matches = data is ViewModelBase;
        DebugLog($"Match() called for '{data?.GetType().Name ?? "null"}' - {matches}");
        return matches;
    }
}
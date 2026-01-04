using Avalonia;
using Avalonia.Markup.Xaml;


namespace CS2.Translator.UI;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace CSharpMath.Avalonia.Example;

public class MainView : UserControl {
    public MainView() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    private void LightThemeRbn_OnIsCheckedChanged(object? sender, RoutedEventArgs e) {
        Application.Current!.RequestedThemeVariant = sender is RadioButton { IsChecked: true } ? ThemeVariant.Light : ThemeVariant.Dark;
    }
}
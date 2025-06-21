using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BlackBook;

public partial class App : Application {
    protected override void OnStartup (StartupEventArgs e) {
        base.OnStartup(e);

        EventManager.RegisterClassHandler(typeof(Grid),
            UIElement.MouseLeftButtonDownEvent,
            new MouseButtonEventHandler(SyxBarDrag));

    }

    private static void SyxBarDrag (object sender, MouseButtonEventArgs e) {
        if (sender is Grid g && g.Name == "SyxBar") {
            Window w = Window.GetWindow(g);
            if (e.ClickCount == 2)
                w.WindowState = w.WindowState == WindowState.Normal
                               ? WindowState.Maximized
                               : WindowState.Normal;
            else
                w.DragMove();
        }
    }

    public App () {
        EventManager.RegisterClassHandler(typeof(Window),
            Button.ClickEvent,
            new RoutedEventHandler(SyxChrome_CloseBtn_Click));
    }

    public static void SyxChrome_CloseBtn_Click (object sender, RoutedEventArgs e) {
        if (e.OriginalSource is Button btn && btn.Content?.ToString() == "✕") {
            Window window = Window.GetWindow(btn);
            if (window != null)
                window.Close();
        }
    }

}

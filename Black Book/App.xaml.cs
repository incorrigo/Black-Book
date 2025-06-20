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

}

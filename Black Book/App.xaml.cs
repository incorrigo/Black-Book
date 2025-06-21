using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BlackBook;

public partial class App : Application {

    private void App_Startup (object sender, StartupEventArgs e) {
        var main = new MainWindow();
        main.Show();
    }

    protected override void OnStartup (StartupEventArgs e) {
        // 🔥 CRITICAL: Force software rendering to allow transparency
        System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

        base.OnStartup(e);

        EventManager.RegisterClassHandler(typeof(Grid),
            UIElement.MouseLeftButtonDownEvent,
            new MouseButtonEventHandler(SyxBarDrag));

        EventManager.RegisterClassHandler(typeof(Window),
            Button.ClickEvent,
            new RoutedEventHandler(SyxChrome_CloseBtn_Click));
    }


    private static void SyxBarDrag (object sender, MouseButtonEventArgs e) {
        if (sender is Grid g && g.Name == "SyxBar") {
            Window w = Window.GetWindow(g);
            if (e.ClickCount == 2) {
                w.WindowState = w.WindowState == WindowState.Normal
                    ? WindowState.Maximized
                    : WindowState.Normal;
            }
            else {
                w.DragMove();
            }
        }
    }

    private static void SyxChrome_CloseBtn_Click (object sender, RoutedEventArgs e) {
        if (e.OriginalSource is Button btn && btn.Content?.ToString() == "✕") {
            Window w = Window.GetWindow(btn);
            w?.Close();
        }
    }
}

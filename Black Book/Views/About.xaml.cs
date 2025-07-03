// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
// 2025-07-01 [Tuesday]
// [BlackBook/Views/About.xaml.cs]

using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace BlackBook.Views;

public partial class About : Window {
    public About () {
        InitializeComponent();
    }

    private void Close_Click (object sender, RoutedEventArgs e) {
        this.Close();
    }

    private void Hyperlink_RequestNavigate (object sender, RequestNavigateEventArgs e) {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

}

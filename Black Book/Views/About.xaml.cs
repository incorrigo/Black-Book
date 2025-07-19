///////
/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
/// h t t p s : / / i n c o r r i g o . i o /
////
/// Secure Profile Manager

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

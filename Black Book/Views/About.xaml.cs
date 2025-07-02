// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
// 2025-07-01 [Tuesday]
// [BlackBook/Views/About.xaml.cs]

using System.Windows;

namespace BlackBook.Views;

public partial class About : Window {
    public About () {
        InitializeComponent();
    }

    private void Close_Click (object sender, RoutedEventArgs e) {
        this.Close();
    }
}

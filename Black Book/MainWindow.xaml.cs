using System.Windows;

namespace BlackBook;

public partial class MainWindow : Window {
    public MainWindow () {
        InitializeComponent();
    }

    private void CreateKey_Click (object sender, RoutedEventArgs e) {
        new Views.CertificateSetupWindow().ShowDialog();
    }

    private void OpenBook_Click (object sender, RoutedEventArgs e) {
        new Views.SituationManager().ShowDialog();
    }

    private void Quit_Click (object sender, RoutedEventArgs e) {
        this.Close();
    }
}

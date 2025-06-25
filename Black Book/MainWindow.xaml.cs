using System.Windows;
using BlackBook.Views;

namespace BlackBook;

public partial class MainWindow : Window {
    public MainWindow () {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded (object sender, RoutedEventArgs e) {
        Title = $"Black Book – {SessionManager.CurrentUserName}";
    }

    private void CreateKey_Click (object sender, RoutedEventArgs e) {
        var setup = new CertificateSetupWindow();
        setup.Owner = this;
        setup.ShowDialog();
    }

    private void Logout_Click (object sender, RoutedEventArgs e) {
        SessionManager.CurrentUserName = "";
        SessionManager.CurrentPassword = "";    // ← clear password if you like
        SessionManager.Data = null;

        var login = new InitialLoginWindow();
        login.Show();
        Close();
    }



    private void OpenBook_Click (object sender, RoutedEventArgs e) {
        var entry = new CorrespondenceEntryWindow();
        entry.Owner = this;
        entry.ShowDialog();
    }

    private void Quit_Click (object sender, RoutedEventArgs e) {
        Close();
    }
}

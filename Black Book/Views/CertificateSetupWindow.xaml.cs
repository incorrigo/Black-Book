using System.Windows;
using BlackBook.Security;

namespace BlackBook.Views;

public partial class CertificateSetupWindow : Window {
    public CertificateSetupWindow () {
        InitializeComponent();
    }

    private void CreateKey_Click (object sender, RoutedEventArgs e) {
        var userName = UserNameBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)) {
            MessageBox.Show("Name and password can't be empty.");
            return;
        }

        var cert = SecurityManager.GenerateCertificate(userName, password);
        SecurityManager.ExportCertificate(cert, "usercert.pfx", password);

        MessageBox.Show("Certificate created and saved successfully.");
        Close();
    }
}

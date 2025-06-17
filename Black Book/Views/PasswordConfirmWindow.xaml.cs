using System.Windows;
using System.Windows.Media;

namespace BlackBook.Views;

public partial class PasswordConfirmWindow : Window {
    private readonly string originalPassword;
    public bool PasswordConfirmed { get; private set; }

    public PasswordConfirmWindow (string originalPassword) {
        InitializeComponent();
        this.originalPassword = originalPassword;
        ActionButton.Content = "Cancel";
    }

    private void ConfirmPasswordBox_PasswordChanged (object sender, RoutedEventArgs e) {
        bool match = ConfirmPasswordBox.Password == originalPassword;

        ConfirmPasswordBox.BorderBrush = match
            ? new SolidColorBrush(Color.FromRgb(0, 200, 0))  // Green border for match
            : new SolidColorBrush(Color.FromRgb(85, 85, 85)); // Default grey otherwise

        ActionButton.Content = match ? "Done" : "Cancel";
    }

    private void ActionButton_Click (object sender, RoutedEventArgs e) {
        if (ActionButton.Content.ToString() == "Done") {
            PasswordConfirmed = true;
        }
        Close();
    }
}

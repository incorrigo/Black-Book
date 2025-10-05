// BlackBook/Views/Dialogs/PasswordInputDialog.xaml.cs
/////
//// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
//// h t t p s : / / i n c o r r i g o . i o /
////
//// Password Input Dialog (Code-behind)
using System.Windows;

namespace BlackBook.Services.Dialogs;

public partial class PasswordInputDialog : Window {
    public string Password { get; private set; } = string.Empty;

    public PasswordInputDialog (string title, string prompt) {
        InitializeComponent();
        Owner = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
        TitleBlock.Text = title;
        PromptBlock.Text = prompt;
        Loaded += (_, __) => PasswordBox.Focus();
    }

    private void Ok_Click (object sender, RoutedEventArgs e) {
        Password = PasswordBox.Password;
        DialogResult = true;
        Close();
    }
}

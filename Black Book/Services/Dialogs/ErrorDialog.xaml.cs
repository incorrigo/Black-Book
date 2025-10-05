// BlackBook/Views/Dialogs/ErrorDialog.xaml.cs
/////
//// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
//// h t t p s : / / i n c o r r i g o . i o /
////
//// Error Dialog Window (Code-behind)
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BlackBook.Services.Dialogs;

public partial class ErrorDialog : Window {
    public ErrorDialog (string title, string message, string? details) {
        InitializeComponent();
        Owner = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
        TitleBlock.Text = title;
        MessageBlock.Text = message;

        SetIcon(DialogIcon.Error);

        if (!string.IsNullOrWhiteSpace(details)) {
            DetailsExpander.Visibility = Visibility.Visible;
            DetailsBox.Text = details!;
        }
    }

    private void Copy_Click (object sender, RoutedEventArgs e) {
        try { Clipboard.SetText($"{TitleBlock.Text}\r\n{MessageBlock.Text}\r\n\r\n{DetailsBox.Text}"); } catch { }
    }

    public static Task<DialogResult2> ShowAsync (string title, string message, string? details) {
        var dlg = new ErrorDialog(title, message, details);
        _ = dlg.ShowDialog();
        return Task.FromResult(DialogResult2.OK);
    }

    private void SetIcon (DialogIcon icon) {
        System.Drawing.Icon? sysIcon = System.Drawing.SystemIcons.Error; // fixed for error dialog
        using var bmp = sysIcon.ToBitmap();
        using var ms = new MemoryStream();
        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        ms.Position = 0;

        var img = new BitmapImage();
        img.BeginInit();
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.StreamSource = ms;
        img.EndInit();
        img.Freeze();

        IconImage.Source = img;
    }
}

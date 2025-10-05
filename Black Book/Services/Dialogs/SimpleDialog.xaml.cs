// BlackBook/Views/Dialogs/SimpleDialog.xaml.cs
/////
//// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
//// h t t p s : / / i n c o r r i g o . i o /
////
//// Simple Dialog Window (Code-behind)
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BlackBook.Services.Dialogs;

public partial class SimpleDialog : Window {
    public bool UserChoseYes { get; private set; }

    public SimpleDialog (string title, string message, DialogIcon icon, bool okOnly) {
        InitializeComponent();
        Owner = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
        TitleBlock.Text = title;
        MessageBlock.Text = message;

        SetIcon(icon);

        if (okOnly) {
            OkButton.Visibility = Visibility.Visible;
        }
        else {
            OkButton.Visibility = Visibility.Collapsed;
            YesButton.Visibility = Visibility.Visible;
            NoButton.Visibility = Visibility.Visible;
        }
    }

    private void Ok_Click (object sender, RoutedEventArgs e) {
        DialogResult = true;
        Close();
    }

    private void Yes_Click (object sender, RoutedEventArgs e) {
        UserChoseYes = true;
        DialogResult = true;
        Close();
    }

    private void No_Click (object sender, RoutedEventArgs e) {
        UserChoseYes = false;
        DialogResult = false;
        Close();
    }

    private void SetIcon (DialogIcon icon) {
        // fully-qualify to avoid ambiguity and missing usings
        System.Drawing.Icon? sysIcon = icon switch {
            DialogIcon.Error => System.Drawing.SystemIcons.Error,
            DialogIcon.Warning => System.Drawing.SystemIcons.Warning,
            DialogIcon.Question => System.Drawing.SystemIcons.Question,
            DialogIcon.Info => System.Drawing.SystemIcons.Information,
            _ => null
        };

        if (sysIcon == null) return;

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

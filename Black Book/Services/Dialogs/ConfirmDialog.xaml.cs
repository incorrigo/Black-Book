// BlackBook/Views/Dialogs/ConfirmDialog.xaml.cs
/////
//// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
//// h t t p s : / / i n c o r r i g o . i o /
////
//// Dangerous Confirm Dialog (Code-behind)
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BlackBook.Services.Dialogs;

public partial class ConfirmDialog : Window {
    private readonly string _keyword;

    public ConfirmDialog (string title, string message, string keyword) {
        InitializeComponent();
        Owner = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
        TitleBlock.Text = title;
        QuestionBlock.Text = $"{message}\r\nType {keyword} to proceed.";
        _keyword = keyword;

        SetIcon(DialogIcon.Warning);

        KeywordBox.TextChanged += (_, __) => YesButton.IsEnabled = string.Equals(KeywordBox.Text.Trim(), _keyword);
    }

    private void Yes_Click (object sender, RoutedEventArgs e) {
        DialogResult = true;
        Close();
    }

    public static Task<DialogResult2> ShowAsync (string title, string message, string confirmWord) {
        var dlg = new ConfirmDialog(title, message, confirmWord);
        var ok = dlg.ShowDialog() == true;
        return Task.FromResult(ok ? DialogResult2.Yes : DialogResult2.No);
    }

    private void SetIcon (DialogIcon icon) {
        System.Drawing.Icon sysIcon = System.Drawing.SystemIcons.Warning; // fixed for confirm
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

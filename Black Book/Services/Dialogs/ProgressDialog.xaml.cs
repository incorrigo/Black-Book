// BlackBook/Views/Dialogs/ProgressDialog.xaml.cs
/////
//// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
//// h t t p s : / / i n c o r r i g o . i o /
////
//// Progress Dialog Window (Code-behind)
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BlackBook.Services.Dialogs;

public partial class ProgressDialog : Window {
    private readonly CancellationTokenSource _cts = new();

    public ProgressDialog (string title, string message) {
        InitializeComponent();
        Owner = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
        TitleBlock.Text = title;
        MessageBlock.Text = message;
        CancelButton.Click += (_, __) => _cts.Cancel();
    }

    public static async Task RunAsync (string title, string message, Func<IProgress<double>, Task> work) {
        var dlg = new ProgressDialog(title, message);
        dlg.Show();

        var progress = new Progress<double>(v => {
            if (v < 0) v = 0;
            if (v > 100) v = 100;
            dlg.Bar.Value = v;
        });

        try {
            await work(progress);
        }
        catch (OperationCanceledException) {
            // caller can surface a message if needed
        }
        finally {
            dlg.Close();
        }
    }
}

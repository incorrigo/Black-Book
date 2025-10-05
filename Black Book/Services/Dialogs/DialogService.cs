// BlackBook/Services/Dialogs/DialogService.cs
/////
//// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
//// h t t p s : / / i n c o r r i g o . i o /
////
//// Unified Dialog Service
using System;
using System.Threading.Tasks;
using System.Windows;

namespace BlackBook.Services.Dialogs;

public enum DialogIcon {
    None,
    Info,
    Warning,
    Error,
    Question
}

public enum DialogResult2 {
    None,
    OK,
    Cancel,
    Yes,
    No
}

public interface IDialogService {
    Task<DialogResult2> InfoAsync (string title, string message);
    Task<DialogResult2> WarnAsync (string title, string message);
    Task<DialogResult2> ErrorAsync (string title, string message, string? details = null);
    Task<DialogResult2> AskAsync (string title, string question);
    Task<DialogResult2> ConfirmDangerAsync (string title, string question, string confirmWord = "DELETE");
    Task<DialogResult2> InputPasswordThenAsync (string title, string prompt, Func<string, Task> onOk);
    Task ShowProgressAsync (string title, string message, Func<IProgress<double>, Task> work);
}

public sealed class DialogService : IDialogService {
    public Task<DialogResult2> InfoAsync (string title, string message) =>
        ShowSimpleAsync(title, message, DialogIcon.Info, okOnly: true);

    public Task<DialogResult2> WarnAsync (string title, string message) =>
        ShowSimpleAsync(title, message, DialogIcon.Warning, okOnly: true);

    public Task<DialogResult2> ErrorAsync (string title, string message, string? details = null) =>
        ErrorDialog.ShowAsync(title, message, details);

    public Task<DialogResult2> AskAsync (string title, string question) =>
        ShowSimpleAsync(title, question, DialogIcon.Question, okOnly: false);

    public Task<DialogResult2> ConfirmDangerAsync (string title, string question, string confirmWord = "DELETE") =>
        ConfirmDialog.ShowAsync(title, question, confirmWord);

    public async Task<DialogResult2> InputPasswordThenAsync (string title, string prompt, Func<string, Task> onOk) {
        var dlg = new PasswordInputDialog(title, prompt);
        if (dlg.ShowDialog() == true) {
            await onOk(dlg.Password);
            return DialogResult2.OK;
        }
        return DialogResult2.Cancel;
    }

    public Task ShowProgressAsync (string title, string message, Func<IProgress<double>, Task> work) =>
        ProgressDialog.RunAsync(title, message, work);

    private static Task<DialogResult2> ShowSimpleAsync (string title, string message, DialogIcon icon, bool okOnly) {
        var dlg = new SimpleDialog(title, message, icon, okOnly);
        var res = dlg.ShowDialog() == true ? DialogResult2.OK : DialogResult2.Cancel;
        if (!okOnly) res = dlg.UserChoseYes ? DialogResult2.Yes : DialogResult2.No;
        return Task.FromResult(res);
    }
}

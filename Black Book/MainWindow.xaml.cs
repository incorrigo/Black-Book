/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
// 2025‑06‑26 [Thursday]
///
// [BlackBook/MainWindow.xaml.cs]

using BlackBook.Views;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace BlackBook;

public partial class MainWindow : Window {
    public MainWindow () {
        InitializeComponent();
        DataContext = SessionManager.Data;         // bind the entire profile container
    }

    #region File menu
    private async void Logout_Click (object sender, RoutedEventArgs e) {
        try {
            await SessionManager.SaveAndClearAsync();
            new InitialLoginWindow().Show();
            Close();
        }
        catch (Exception ex) {
            MessageBox.Show($"Logout failed:\n{ex.Message}", "Black Book",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private void Exit_Click (object? s, RoutedEventArgs e) => Close();
    #endregion

    #region New‑entity shortcuts
    private void NewPerson_Click (object? sender, RoutedEventArgs e) {
        var dlg = new PersonEntryWindow { Owner = this };
        dlg.ShowDialog();
    }



    private void NewSituation_Click (object? sender, RoutedEventArgs e) {
        var host = new Window {
            Title = "Black Book | New Situation",
            Content = new SituationManager { Margin = new Thickness(12) },
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this
        };
        host.ShowDialog();
    }


    private void NewCorrespondence_Click (object? s, RoutedEventArgs e) =>
        new CorrespondenceEntryWindow { Owner = this }.ShowDialog();
    #endregion
}

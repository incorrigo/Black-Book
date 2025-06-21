using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BlackBook;         // <‑‑ must match x:Class minus .MainWindow

public partial class MainWindow : Window {
    private readonly string keyDir =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Keys");

    public MainWindow () {
        InitializeComponent();
        Loaded += (_, _) => RefreshButtons();
    }

    private void SyxBar_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
        if (e.ClickCount == 2) {
            // Optionally maximize/restore, if you want (not needed if NoResize)
            // WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }
        else if (e.LeftButton == MouseButtonState.Pressed) {
            DragMove();
            e.Handled = true;
        }
    }

    // Custom close button handler
    private void CloseBtn_Click (object sender, RoutedEventArgs e) {
        this.Close();
    }


    /* ---------- UI logic ---------- */

    private void RefreshButtons () {
        Directory.CreateDirectory(keyDir);
        var hasKey = Directory.EnumerateFiles(keyDir, "*.pfx").Any();
        CreateKeyBtn.IsEnabled = true;
        OpenBookBtn.IsEnabled = hasKey;
    }

    /* ---------- button handlers ---------- */

    private void CreateKey_Click (object sender, RoutedEventArgs e) {
        new Views.CertificateSetupWindow { Owner = this }.ShowDialog();
        RefreshButtons();
    }

    private void OpenBook_Click (object sender, RoutedEventArgs e) {
        var keys = Directory.EnumerateFiles(keyDir, "*.pfx").ToList();
        if (keys.Count == 0) return;

        string chosen = keys.Count == 1 ? keys[0] : PromptForKey(keys);
        if (chosen == null) return;

        // TODO: password prompt + cert load
        new Views.SituationManager { Owner = this }.Show();
        Hide();
    }

    private void Quit_Click (object sender, RoutedEventArgs e) => Close();

    /* ---------- helpers ---------- */

    private static string? PromptForKey (IList<string> paths) {
        var dlg = new OpenFileDialog {
            Title = "Choose certificate",
            Filter = "PKCS#12 (*.pfx)|*.pfx",
            InitialDirectory = Path.GetDirectoryName(paths[0])
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }
}

// ProfileDeleteProgressWindow.xaml.cs
using System.Windows;

namespace BlackBook.Views;

public partial class ProfileDeleteProgressWindow : Window {
    public ProfileDeleteProgressWindow () {
        InitializeComponent();
    }

    public void SetProgress (double progressFraction) {
        ProgressBar.Value = progressFraction * 100;
    }
}

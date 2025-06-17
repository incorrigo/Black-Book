using BlackBook.Views;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Black_Book
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded (object sender, RoutedEventArgs e) {
            if (!File.Exists("usercert.pfx")) {
                var setupWindow = new CertificateSetupWindow();
                setupWindow.ShowDialog();
            }
        }

    }
}
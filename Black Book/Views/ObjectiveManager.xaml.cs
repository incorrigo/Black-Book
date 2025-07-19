using System.Windows.Controls;
using BlackBook.Storage;

namespace BlackBook.Views;

public partial class ObjectiveManager : UserControl {
    public ObjectiveManager () {
        InitializeComponent();
        DataContext = SessionManager.Data!;
    }
}

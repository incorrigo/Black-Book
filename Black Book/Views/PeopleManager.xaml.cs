using System.Windows.Controls;

namespace BlackBook.Views;

public partial class PeopleManager : UserControl {
    public PeopleManager () {
        InitializeComponent();
        DataContext = SessionManager.Data;      // bind live profile container
    }
}

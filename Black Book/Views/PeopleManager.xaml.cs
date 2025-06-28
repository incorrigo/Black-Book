// PeopleManager.xaml.cs
using System.Windows.Controls;

namespace BlackBook.Views {
    public partial class PeopleManager : UserControl {
        public PeopleManager () {
            InitializeComponent();
            DataContext = SessionManager.Data;  // bind to the live profile container (BlackBookContainer)
        }

        // Allow external code (like MainWindow) to select a person programmatically
        public void SelectPerson (BlackBook.Models.Person person) {
            PeopleList.SelectedItem = person;
            if (PeopleList.SelectedItem != null) {
                PeopleList.ScrollIntoView(PeopleList.SelectedItem);
            }
        }
    }
}

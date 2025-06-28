// MainWindow.xaml.cs
using System.Windows;
using BlackBook.Views;
using BlackBook.Models;

namespace BlackBook.Views {
    public partial class MainWindow : Window {
        public MainWindow () {
            InitializeComponent();
            // Ensure the data context is set so embedded user controls receive the data context if needed
            DataContext = SessionManager.Data;
            // Optionally, set minimum window size for usability
            this.MinWidth = 600;
            this.MinHeight = 400;
        }

        // File -> Close Profile (log out to initial screen, if desired)
        private void CloseProfile_Click (object sender, RoutedEventArgs e) {
            // Close the main window and perhaps reopen the login window
            new InitialLoginWindow().Show();
            this.Close();
        }

        // File -> Exit
        private void Exit_Click (object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        // New -> Person...
        private async void NewPerson_Click (object sender, RoutedEventArgs e) {
            var personWindow = new PersonEntryWindow();
            personWindow.Owner = this;
            bool? result = personWindow.ShowDialog();
            if (result == true) {
                // A new person was added. Select them in the People list.
                var people = SessionManager.Data!.People;
                if (people.Count > 0) {
                    // Select the last person (assuming new added to end) and scroll into view
                    PeopleManagerView.SelectPerson(people[^1]);
                }
            }
        }

        // New -> Company...
        private async void NewCompany_Click (object sender, RoutedEventArgs e) {
            var companyWindow = new CompanyEntryWindow();
            companyWindow.Owner = this;
            bool? result = companyWindow.ShowDialog();
            if (result == true) {
                // (Optional) If needed, we could handle any post-add action for companies.
                // We might refresh any company lists or update PeopleManager if needed.
            }
        }

        // New -> Situation...
        private async void NewSituation_Click (object sender, RoutedEventArgs e) {
            var situationWindow = new SituationEntryWindow();
            situationWindow.Owner = this;
            bool? result = situationWindow.ShowDialog();
            if (result == true) {
                // A new situation added. Select it in the Situations list.
                var situations = SessionManager.Data!.Situations;
                if (situations.Count > 0) {
                    SituationManagerView.SelectSituation(situations[^1]);
                }
                // Also switch to the Situations tab to show the user the new situation
                MainTabControl.SelectedIndex = 1; // Situations tab
            }
        }

        // New -> Correspondence...
        private async void NewInteraction_Click (object sender, RoutedEventArgs e) {
            var interactionWindow = new CorrespondenceEntryWindow();
            interactionWindow.Owner = this;
            interactionWindow.ShowDialog();
            // If saved, CorrespondenceEntryWindow already saves data and closes itself with a success message.
            // We can decide to refresh or update UI if needed, but the data bindings auto-update from SessionManager.Data.
            // Optionally, switch to Situations tab if a situation was involved, or to Contacts if relevant.
        }
    }
}

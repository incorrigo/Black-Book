// CompanyManager.xaml.cs
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;

namespace BlackBook.Views;

public partial class CompanyManager : UserControl {
    public CompanyManager () {
        InitializeComponent();
        DataContext = SessionManager.Data;
    }

    public void SelectCompany (Company company) {
        CompanyList.SelectedItem = company;
        if (CompanyList.SelectedItem != null) {
            CompanyList.ScrollIntoView(CompanyList.SelectedItem);
        }
    }

    private void CompanyList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
        if (CompanyList.SelectedItem is Company company) {
            var window = new CompanyEntryWindow(company);
            window.Owner = Window.GetWindow(this);
            bool? result = window.ShowDialog();
            if (result == true) {
                CompanyList.Items.Refresh();
                int idx = CompanyList.SelectedIndex;
                CompanyList.SelectedIndex = -1;
                CompanyList.SelectedIndex = idx;
                // Refresh person list company names in Contacts tab in case name changed
                var mainWin = (MainWindow)Window.GetWindow(this);
                mainWin.PeopleManagerView.PeopleList.Items.Refresh();
            }
        }
    }

    private void EditCompany_Click (object sender, RoutedEventArgs e) {
        CompanyList_MouseDoubleClick(sender, null!);
    }

    private async void DeleteCompany_Click (object sender, RoutedEventArgs e) {
        if (CompanyList.SelectedItem is Company company) {
            bool hasContacts = SessionManager.Data!.People.Any(p => p.CompanyId == company.Id);
            bool hasInteractions = SessionManager.Data.Interactions.Any(i => i.CompanyId == company.Id);
            if (hasContacts || hasInteractions) {
                MessageBox.Show("This company is linked to contacts and correspondence\r\n" +
                            "\r\nIt cannot be deleted until these links have been removed",
                                "Black Book", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var confirm = MessageBox.Show($"Permanently delete {company.Name}?",
                                          "Delete Company", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;
            SessionManager.Data.Companies.Remove(company);
            try {
                await SecureProfileManager.SaveProfileAsync(SessionManager.CurrentUserName, SessionManager.CurrentPassword,
                                                            SessionManager.Data,
                                                            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                                                            CancellationToken.None);
            }
            catch (System.Exception ex) {
                MessageBox.Show($"Failed to save data:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show($"Company \"{company.Name}\" has been deleted", "Black Book", MessageBoxButton.OK, MessageBoxImage.None);
        }
    }

    private void AddPerson_Click (object sender, RoutedEventArgs e) {
        if (CompanyList.SelectedItem is Company company) {
            var personWindow = new PersonEntryWindow();
            personWindow.Owner = Window.GetWindow(this);
            personWindow.PrefillCompany(company.Name);
            bool? result = personWindow.ShowDialog();
            if (result == true) {
                CompanyList.Items.Refresh();
                int idx = CompanyList.SelectedIndex;
                CompanyList.SelectedIndex = -1;
                CompanyList.SelectedIndex = idx;
            }
        }
    }

    private void AddCorrespondence_Click (object sender, RoutedEventArgs e) {
        if (CompanyList.SelectedItem is Company company) {
            var window = new CorrespondenceEntryWindow();
            window.Owner = Window.GetWindow(this);
            window.PrefillCompany(company);
            window.ShowDialog();
        }
    }

    private void PeopleList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
        if (PeopleList.SelectedItem is Person person) {
            var mainWin = (MainWindow)Window.GetWindow(this);
            mainWin.PeopleManagerView.SelectPerson(person);
            mainWin.MainTabControl.SelectedIndex = 0;
        }
    }

    private void InteractionList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
        if (InteractionList.SelectedItem is Interaction interaction) {
            var reader = new InteractionReader(interaction, SessionManager.Data!);
            reader.Owner = Window.GetWindow(this);
            reader.ShowDialog();
        }
    }

    private void InteractionList_PreviewMouseWheel (object sender, MouseWheelEventArgs e) {
        if (!e.Handled) {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            ((Control)sender).RaiseEvent(eventArg);
        }
    }


}

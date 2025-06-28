// SituationManager.xaml.cs
using System.Windows;
using System.Windows.Controls;

namespace BlackBook.Views {
    public partial class SituationManager : UserControl {
        public SituationManager () {
            InitializeComponent();
            DataContext = SessionManager.Data;  // bind to profile data (Situations, etc.)
        }

        // Allow external code to select a situation programmatically
        public void SelectSituation (BlackBook.Models.Situation situation) {
            SituationsList.SelectedItem = situation;
            if (SituationsList.SelectedItem != null) {
                SituationsList.ScrollIntoView(SituationsList.SelectedItem);
            }
        }

        // When an interaction is selected in history and Reply is clicked, prepare a response entry
        private void ReplyToSelected_Click (object sender, System.Windows.RoutedEventArgs e) {
            if (HistoryList.SelectedItem is not BlackBook.Models.Interaction selected) {
                return;
            }
            // Open the correspondence window pre-filled for a reply
            var replyWindow = new CorrespondenceEntryWindow();
            replyWindow.Owner = Window.GetWindow(this);
            // Pre-fill fields by simulating what was done in original Interaction selection logic:
            // (We can call a method on CorrespondenceEntryWindow if exposed, but we'll rely on its existing logic by selecting in UI)
            // Instead, simpler: we can set properties if we expose them in CorrespondenceEntryWindow. 
            // For now, just show the window and let user type (improvement: pre-fill person/company).
            replyWindow.ShowDialog();
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlackBook.Helpers;

public static class PlaceholderHelper {

    /// <summary>
    // PlaceholderHelper is an implementation of attached property that enables placeholder text
    // without wasting time fucking about with third party packages
    /// </summary>
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.RegisterAttached(
            "PlaceholderText", typeof(string), typeof(PlaceholderHelper),
            new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

    public static string GetPlaceholderText (Control control) {
        return (string)control.GetValue(PlaceholderTextProperty);
    }

    public static void SetPlaceholderText (Control control, string value) {
        control.SetValue(PlaceholderTextProperty, value);
    }

    private static void OnPlaceholderTextChanged (DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is TextBox textBox) {
            textBox.Loaded += Control_Loaded;
            textBox.TextChanged += Control_TextChanged;
        }
        else if (d is ComboBox comboBox) {
            comboBox.Loaded += Control_Loaded;
            comboBox.SelectionChanged += ComboBox_SelectionChanged;
            comboBox.LostFocus += ComboBox_LostFocus;
        }
    }

    private static void Control_Loaded (object sender, RoutedEventArgs e) {
        UpdatePlaceholder(sender as Control);
    }

    private static void Control_TextChanged (object sender, TextChangedEventArgs e) {
        UpdatePlaceholder(sender as Control);
    }

    private static void ComboBox_SelectionChanged (object sender, SelectionChangedEventArgs e) {
        UpdatePlaceholder(sender as Control);
    }

    private static void ComboBox_LostFocus (object sender, RoutedEventArgs e) {
        UpdatePlaceholder(sender as Control);
    }

    private static void UpdatePlaceholder (Control control) {
        if (control is null) return;

        var placeholderText = GetPlaceholderText(control);

        bool showPlaceholder = control switch {
            TextBox tb => string.IsNullOrEmpty(tb.Text),
            ComboBox cb => (cb.SelectedIndex == -1 && string.IsNullOrEmpty(cb.Text)),
            _ => false
        };

        if (showPlaceholder) {
            control.Foreground = Brushes.Gray;
            switch (control) {
                case TextBox tb:
                    tb.Text = placeholderText;
                    break;
                case ComboBox cb:
                    cb.Text = placeholderText;
                    break;
            }
        }
        else {
            if (control.Foreground == Brushes.Gray) {
                control.Foreground = Brushes.Black;
                switch (control) {
                    case TextBox tb when tb.Text == placeholderText:
                        tb.Clear();
                        break;
                    case ComboBox cb when cb.Text == placeholderText:
                        cb.Text = string.Empty;
                        break;
                }
            }
        }
    }
}

// PlaceholderHelper.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;


/////
/// This class creates a native placeholder text implementation for WPF controls
/// so that there is no need to use third party conveniences
/////


namespace BlackBook.Helpers;

public static class PlaceholderHelper {

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.RegisterAttached(
            "PlaceholderText", typeof(string), typeof(PlaceholderHelper),
            new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

    public static string GetPlaceholderText (Control control)
        => (string)control.GetValue(PlaceholderTextProperty);

    public static string GetControlText (Control control)
           => control switch {
               TextBox tb => tb.Text,
               ComboBox cb => cb.Text,
               _ => string.Empty
           };

    public static void SetPlaceholderText (Control control, string value)
        => control.SetValue(PlaceholderTextProperty, value);

    private static void OnPlaceholderTextChanged (DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is not Control control) return;

        control.Loaded += Control_Loaded;

        if (control is TextBox tb) {
            tb.TextChanged += (s, _) => ShowOrHidePlaceholder(tb);
        }
        else if (control is ComboBox cb) {
            cb.SelectionChanged += (s, _) => ShowOrHidePlaceholder(cb);
            cb.LostFocus += (s, _) => ShowOrHidePlaceholder(cb);
            cb.GotFocus += (s, _) => ShowOrHidePlaceholder(cb);
            cb.IsEditable = true;

            // Explicit fix: subscribe to internal textbox's TextChanged event
            cb.Loaded += (s, _) => {
                if (cb.Template.FindName("PART_EditableTextBox", cb) is TextBox editableTextBox) {
                    editableTextBox.TextChanged += (sender, __) => ShowOrHidePlaceholder(cb);
                }
            };
        }
    }


    private static void Control_Loaded (object sender, RoutedEventArgs e) {
        if (sender is Control control) {
            var layer = AdornerLayer.GetAdornerLayer(control);
            if (layer != null) {
                layer.Add(new PlaceholderAdorner(control, GetPlaceholderText(control)));
                ShowOrHidePlaceholder(control);
            }
        }
    }

    private static void ShowOrHidePlaceholder (Control control) {
        var layer = AdornerLayer.GetAdornerLayer(control);
        if (layer == null) return;

        var adorners = layer.GetAdorners(control);
        if (adorners == null) return;

        bool enable = GetEnablePlaceholder(control);

        foreach (var adorner in adorners) {
            if (adorner is PlaceholderAdorner placeholderAdorner) {
                bool hidePlaceholder = control switch {
                    TextBox tb => !string.IsNullOrEmpty(tb.Text),
                    ComboBox cb => (cb.SelectedIndex != -1 || !string.IsNullOrEmpty(cb.Text)),
                    _ => true
                };

                placeholderAdorner.Visibility = (!enable || hidePlaceholder)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }
    }

    public static readonly DependencyProperty EnablePlaceholderProperty =
        DependencyProperty.RegisterAttached(
            "EnablePlaceholder", typeof(bool), typeof(PlaceholderHelper),
            new PropertyMetadata(true, OnEnablePlaceholderChanged));

    public static bool GetEnablePlaceholder (Control control)
        => (bool)control.GetValue(EnablePlaceholderProperty);

    public static void SetEnablePlaceholder (Control control, bool value)
        => control.SetValue(EnablePlaceholderProperty, value);

    private static void OnEnablePlaceholderChanged (DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is Control control) {
            UpdatePlaceholderVisibility(control);
        }
    }

    private static void UpdatePlaceholderVisibility (Control control) {
        var layer = AdornerLayer.GetAdornerLayer(control);
        if (layer == null) return;

        var adorners = layer.GetAdorners(control);
        if (adorners == null) return;

        bool enable = GetEnablePlaceholder(control);

        foreach (var adorner in adorners) {
            if (adorner is PlaceholderAdorner placeholderAdorner) {
                placeholderAdorner.Visibility = (enable && string.IsNullOrEmpty(GetControlText(control)))
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }

    // Inner class for rendering placeholder text
    private sealed class PlaceholderAdorner : Adorner {
        private readonly string _placeholder;
        private readonly TextBlock _textBlock;

        public PlaceholderAdorner (UIElement adornedElement, string placeholder)
            : base(adornedElement) {
            IsHitTestVisible = false;
            _placeholder = placeholder;

            _textBlock = new TextBlock {
                Text = _placeholder,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.Gray,
                Margin = new Thickness(4, 2, 0, 0)
            };
        }

        protected override void OnRender (DrawingContext drawingContext) {
            if (AdornedElement is Control control && string.IsNullOrEmpty(GetControlText(control))) {

                var typeface = new Typeface(
                    _textBlock.FontFamily,
                    _textBlock.FontStyle,
                    _textBlock.FontWeight,
                    _textBlock.FontStretch);

                var formattedText = new FormattedText(
                    _placeholder,
                    System.Globalization.CultureInfo.CurrentUICulture,
                    _textBlock.FlowDirection,
                    typeface,
                    _textBlock.FontSize,
                    _textBlock.Foreground,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                drawingContext.DrawText(formattedText, new Point(_textBlock.Margin.Left, _textBlock.Margin.Top));
            }
        }

    }
}

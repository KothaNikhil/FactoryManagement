using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FactoryManagement.Controls
{
    public partial class SummaryCard : UserControl
    {
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(SummaryCard),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(object),
                typeof(SummaryCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(string),
                typeof(SummaryCard),
                new PropertyMetadata("📊"));

        public static readonly DependencyProperty CardBackgroundProperty =
            DependencyProperty.Register(
                nameof(CardBackground),
                typeof(Brush),
                typeof(SummaryCard),
                new PropertyMetadata(null));

        public static readonly DependencyProperty LabelForegroundProperty =
            DependencyProperty.Register(
                nameof(LabelForeground),
                typeof(Brush),
                typeof(SummaryCard),
                new PropertyMetadata(null));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public Brush CardBackground
        {
            get => (Brush)GetValue(CardBackgroundProperty);
            set => SetValue(CardBackgroundProperty, value);
        }

        public Brush LabelForeground
        {
            get => (Brush)GetValue(LabelForegroundProperty);
            set => SetValue(LabelForegroundProperty, value);
        }

        public SummaryCard()
        {
            InitializeComponent();
        }
    }
}

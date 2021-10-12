using System.Windows;

namespace BlmConverter
{
    public partial class ErrorWindow
    {
        public ErrorWindow()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(ErrorWindow),
            new PropertyMetadata(default(string))
        );

        public string Message
        {
            get => (string)GetValue(PropertyTypeProperty);
            set => SetValue(PropertyTypeProperty, value);
        }

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
            nameof(Caption),
            typeof(string),
            typeof(ErrorWindow),
            new PropertyMetadata(default(string))
        );

        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public static void Show(string caption, string message, Window owner = null)
        {
            var window = new ErrorWindow
            {
                Caption = caption,
                Message = message,
                Owner = owner
            };

            window.ShowDialog();
        }
    }
}
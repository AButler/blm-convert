using System.Windows;
using System.Windows.Controls;

namespace BlmConverter
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileTextBoxPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }
            
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if(files == null || files.Length != 1)
            {
                return;
            }
            
            e.Effects = DragDropEffects.Link;
            
            e.Handled = true;
        }

        private void FileTextBoxPreviewDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }
            
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if(files != null && files.Length == 1)
            {
                ((TextBox)sender).Text = files[0];
            }
        }
    }
}
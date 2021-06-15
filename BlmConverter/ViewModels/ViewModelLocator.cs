namespace BlmConverter.ViewModels
{
    public static class ViewModelLocator
    {
        static ViewModelLocator()
        {
            MainWindow = new MainWindowViewModel();
        }

        public static MainWindowViewModel MainWindow { get; }
    }
}
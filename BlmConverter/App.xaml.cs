using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using BlmConverter.ViewModels;
using CommandLine;
using CommandLine.Text;

namespace BlmConverter
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var clickOnceArguments = GetClickOnceArguments();

            if (clickOnceArguments != null)
            {
                Run(clickOnceArguments);
            }
            else
            {
                var result = Parser.Default.ParseArguments<Arguments>(e.Args);

                result
                    .WithParsed(Run)
                    .WithNotParsed(errors => ShowErrors(result));
            }

            base.OnStartup(e);
        }

        private void Run(Arguments args)
        {
            var vm = ViewModelLocator.MainWindow;

            vm.InputFilename = args.InputFilename;
            if (!string.IsNullOrWhiteSpace(args.OutputFilename))
            {
                vm.OutputFilename = args.OutputFilename;
            }

            if (args.AutoConvert)
            {
                vm.OpenCsvAfterConversion = false;
                vm.ConvertCommand.Execute(null);
                Current.Shutdown();
            }
            else
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }

        private void ShowErrors(ParserResult<Arguments> result)
        {
            var message = HelpText.AutoBuild(result).ToString();
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Current.Shutdown(1);
        }

        private Arguments GetClickOnceArguments()
        {
            var clickOnceActivationArguments = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
            if (clickOnceActivationArguments == null)
            {
                return null;
            }

            var clickOnceArguments = clickOnceActivationArguments.ActivationData;

            if (clickOnceArguments == null || clickOnceArguments.Length < 1)
            {
                return null;
            }

            if (Uri.TryCreate(clickOnceArguments[0], UriKind.Absolute, out var fileUri))
            {
                return new Arguments
                {
                    AutoConvert = true,
                    InputFilename = fileUri.LocalPath
                };
            }

            return Arguments.Default;
        }
    }
}
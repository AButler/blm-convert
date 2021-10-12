using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BlmConverter.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;

namespace BlmConverter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _isBusy;
        private bool _openCsvAfterConversion;
        private string _inputFilename;
        private string _outputFilename;

        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        public bool OpenCsvAfterConversion
        {
            get => _openCsvAfterConversion;
            set => Set(ref _openCsvAfterConversion, value);
        }

        public string InputFilename
        {
            get => _inputFilename;
            set
            {
                var oldValue = _inputFilename;
                Set(ref _inputFilename, value);
                if (!EqualityComparer<string>.Default.Equals(oldValue, value))
                {
                    InputFilenameChanged(oldValue, value);
                }
            }
        }

        public string OutputFilename
        {
            get => _outputFilename;
            set => Set(ref _outputFilename, value);
        }

        public ICommand SelectInputFileCommand { get; }
        public ICommand SelectOutputFileCommand { get; }
        public RelayCommand ConvertCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand ExitCommand { get; }

        public MainWindowViewModel()
        {
            OpenCsvAfterConversion = Settings.Default.OpenCsvAfterConversion;

            SelectInputFileCommand = new RelayCommand(SelectInputFile, CanSelectFile);
            SelectOutputFileCommand = new RelayCommand(SelectOutputFile, CanSelectFile);
            ConvertCommand = new RelayCommand(Convert, CanConvert);
            AboutCommand = new RelayCommand(About);
            ExitCommand = new RelayCommand(Exit);
        }

        private void Convert()
        {
            IsBusy = true;

            Settings.Default.OpenCsvAfterConversion = OpenCsvAfterConversion;
            Settings.Default.Save();

            try
            {
                var rowsWritten = Task.Run(ConvertAsync).Result;

                if (OpenCsvAfterConversion)
                {
                    Process.Start(new ProcessStartInfo(OutputFilename)
                    {
                        UseShellExecute = true
                    });
                }

                var rowText = rowsWritten != 1 ? "rows" : "row";
                var message = $"Conversion complete. {rowsWritten} {rowText} written.";
                MessageBox.Show(message, "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                var message = ex is AggregateException aggex
                    ? aggex.InnerExceptions.Count > 1 ? ex.ToString() : aggex.InnerExceptions[0].ToString()
                    : ex.ToString();

                ErrorWindow.Show("Error", message, Application.Current.MainWindow);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<int> ConvertAsync()
        {
            if (!File.Exists(InputFilename))
            {
                throw new FileNotFoundException("BLM file not found", InputFilename);
            }

            using (var blmFileReader = new BlmFileReader.BlmFileReader(File.OpenRead(InputFilename)))
            {
                await blmFileReader.ReadHeader();

                using (var csvFile = new StreamWriter(File.Create(OutputFilename), blmFileReader.Encoding))
                {
                    var headerRow = string.Join(",", blmFileReader.Definitions);
                    await csvFile.WriteLineAsync(headerRow);

                    var rowsWritten = 0;
                    var record = await blmFileReader.ReadRecord();

                    while (record != null)
                    {
                        var dataRow = string.Join(",", record.Fields.Values.Select(EscapeCsvValue));
                        await csvFile.WriteLineAsync(dataRow);

                        rowsWritten++;

                        record = await blmFileReader.ReadRecord();
                    }

                    return rowsWritten;
                }
            }
        }

        private bool CanConvert()
        {
            return !IsBusy && File.Exists(InputFilename);
        }

        private void SelectInputFile()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select BLM file",
                Filter = "BLM Files (*.blm)|*.blm|All Files|*.*",
                CheckFileExists = true
            };

            var result = dlg.ShowDialog(Application.Current.MainWindow);
            if (!result.GetValueOrDefault())
            {
                return;
            }

            InputFilename = dlg.FileName;
        }

        private void SelectOutputFile()
        {
            var dlg = new SaveFileDialog
            {
                Title = "Select output file",
                Filter = "CSV Files (*.csv)|*.csv|All Files|*.*",
                DefaultExt = ".csv",
                FileName = OutputFilename,
                AddExtension = true
            };

            var result = dlg.ShowDialog(Application.Current.MainWindow);
            if (!result.GetValueOrDefault())
            {
                return;
            }

            OutputFilename = dlg.FileName;
        }

        private bool CanSelectFile()
        {
            return !IsBusy;
        }

        private void About()
        {
            var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var message = $"BLM Converter {version}";
            MessageBox.Show(message, "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void InputFilenameChanged(string oldValue, string newValue)
        {
            ConvertCommand.RaiseCanExecuteChanged();

            if (string.IsNullOrWhiteSpace(newValue))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(OutputFilename) && !string.Equals(GenerateOutputFilename(oldValue), OutputFilename, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            try
            {
                OutputFilename = GenerateOutputFilename(newValue);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Could not generate output filename from: \"{newValue}\"\r\n{ex}");
                OutputFilename = "";
            }
        }

        private static string GenerateOutputFilename(string inputFilename)
        {
            return Path.ChangeExtension(inputFilename, ".csv");
        }

        private static string EscapeCsvValue(string value)
        {
            const string doubleQuote = "\"";

            var escapedValue = value.Replace(doubleQuote, doubleQuote + doubleQuote);

            return $"{doubleQuote}{escapedValue}{doubleQuote}";
        }
    }
}
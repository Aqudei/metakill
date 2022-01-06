using MahApps.Metro.Controls.Dialogs;
using metakill.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace metakill.ViewModels
{
    class ShellViewModel : BindableBase
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly Dispatcher Dispatcher;

        public string Log { get => log; set => SetProperty(ref log, value); }

        public ICollectionView InFiles { get; set; }
        private ObservableCollection<InFile> _inFiles = new ObservableCollection<InFile>();

        private DelegateCommand _addFilesCommand;
        public DelegateCommand AddFilesCommand => _addFilesCommand = _addFilesCommand ?? new DelegateCommand(AddFiles);

        private DelegateCommand _checkAllCommand;
        public DelegateCommand CheckAllCommand => _checkAllCommand = _checkAllCommand ?? new DelegateCommand(CheckAll);

        private DelegateCommand _removeSelectedCommand;
        public DelegateCommand RemoveSelectedCommand => _removeSelectedCommand = _removeSelectedCommand ?? new DelegateCommand(RemoveSelected);

        private DelegateCommand _startProcessingCommand;
        private string log;

        public DelegateCommand StartProcessingCommand => _startProcessingCommand = _startProcessingCommand ?? new DelegateCommand(StartProcessing);

        private async void StartProcessing()
        {
            await Task.Run(async () =>
            {
                var progress = await _dialogCoordinator.ShowProgressAsync(this, "Processing", "Please wait...");
                progress.SetIndeterminate();
                var items = _inFiles.Where(f => f.Status == "PENDING").ToArray();
                for (int i = 0; i < items.Length; i++)
                {
                    InFile item = items[i];
                    progress.SetMessage($"Removing metadata of {item.FileName}...");

                    try
                    {
                        await Dispatcher.BeginInvoke(new Action(() => item.Status = "Processing"), DispatcherPriority.Normal);
                        RemoveMetaData(item.FileName);
                        await Dispatcher.BeginInvoke(new Action(() => item.Status = "Success"), DispatcherPriority.Normal);
                    }
                    catch (Exception e)
                    {
                        await Dispatcher.BeginInvoke(new Action(() => item.Status = $"Error - {e.Message}"),
                            DispatcherPriority.Normal);
                    }

                }
                await progress.CloseAsync();
            });
        }

        private void RemoveSelected()
        {

            var delFiles = _inFiles.Where(f => f.Selected).ToArray();
            for (int i = delFiles.Length; i-- > 0;)
            {
                _inFiles.Remove(delFiles[i]);
            }
        }

        private void CheckAll()
        {
            foreach (var item in _inFiles)
            {
                item.Selected = true;
            }
        }

        private void AddFiles()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = true
            };

            var rslt = ofd.ShowDialog();
            if (rslt.HasValue && rslt.Value)
            {
                var files = ofd.FileNames.Select(f => new InFile
                {
                    FileName = f,
                    Selected = false,
                    Status = "PENDING"
                });

                foreach (var file in files)
                {
                    if (!_inFiles.Contains(file))
                    {
                        _inFiles.Add(file);
                    }
                    else
                    {
                        file.Status = "PENDING";
                        file.Selected = false;
                    }
                }
            }
        }

        public ShellViewModel(IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            Dispatcher = Application.Current.Dispatcher;
            InFiles = CollectionViewSource.GetDefaultView(_inFiles);
        }

        private void RemoveMetaData(string filename)
        {
            Log += $"Filename: {filename}{Environment.NewLine}Result:{Environment.NewLine}";
            var procInfo = new ProcessStartInfo
            {
                FileName = "exiftool.exe",
                Arguments = $"-overwrite_original -all= \"{filename}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };


            using (var process = Process.Start(procInfo))
            {

                var line = process.StandardOutput.ReadToEnd();
                Log += $"{line}{Environment.NewLine}";
                process.WaitForExit();
            }

            Log += $"- - - - - - - -{Environment.NewLine}";
        }
    }
}

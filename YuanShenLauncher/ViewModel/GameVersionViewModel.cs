using Downloader;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Launcher.Model;

namespace Launcher.ViewModel
{
    public class GameVersionViewModel : ViewModelBase
    {
        private MHYGameServer server;
        public MHYGameServer Server
        {
            get => server;
            set => Set(ref server, value);
        }

        private string sourcePath;
        public string SourcePath
        {
            get => sourcePath;
            set => Set(ref sourcePath, value);
        }

        private List<MHYPkgVersion> damagedFiles;
        public List<MHYPkgVersion> DamagedFiles
        {
            get => damagedFiles;
            set => Set(ref damagedFiles, value);
        }

        private int verifyProgress;
        public int VerifyProgress
        {
            get => verifyProgress;
            set => Set(ref verifyProgress, value);
        }

        public RelayCommand SelectSourceCmd { get; set; }
        public void SelectSource()
        {
            OpenFileDialog d = new OpenFileDialog
            {
                Filter = "原神|YuanShen.exe;GenshinImpact.exe;pkg_version"
            };
            if ((bool)d.ShowDialog())
            {
                SourcePath = Path.GetDirectoryName(d.FileName);
                BackgroundWorker verifyWorker = new BackgroundWorker();
                verifyWorker.WorkerReportsProgress = true;
                verifyWorker.DoWork += new DoWorkEventHandler((sender, args) =>
                {
                    DamagedFiles = MHYGameHelper.VerifyPackage(SourcePath, MHYGameHelper.ParsePkgVersion(Path.Combine(SourcePath, "Audio_Chinese_pkg_version")), verifyWorker.ReportProgress);
                });
                verifyWorker.ProgressChanged += new ProgressChangedEventHandler((sender, args) =>
                {
                    VerifyProgress = args.ProgressPercentage;
                });

                verifyWorker.RunWorkerAsync();
            }
        }

        private string targetPath;
        public string TargetPath
        {
            get => targetPath;
            set => Set(ref targetPath, value);
        }

        public RelayCommand SelectTargetCmd { get; set; }
        public void SelectTarget()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TargetPath = dialog.SelectedPath;
            }
        }

        private CreateDeltaVersionResult deltaVersionResult;
        public CreateDeltaVersionResult DeltaVersionResult
        {
            get => deltaVersionResult;
            set
            {
                Set(ref deltaVersionResult, value);
                DuplicatedFileSize = DeltaVersionResult.DuplicatedFiles.Sum(item => item.FileSize);
                DeltaFileSize = DeltaVersionResult.DeltaFiles.Sum(item => item.FileSize);
            }
        }

        private long duplicatedFileSize;
        public long DuplicatedFileSize
        {
            get => duplicatedFileSize;
            set => Set(ref duplicatedFileSize, value);
        }

        private long deltaFileSize;
        public long DeltaFileSize
        {
            get => deltaFileSize;
            set => Set(ref deltaFileSize, value);
        }

        private int filesToDownload;
        public int FilesToDownload
        {
            get => filesToDownload;
            set => Set(ref filesToDownload, value);
        }

        private double downloadRate;
        public double DownloadRate
        {
            get => downloadRate;
            set => Set(ref downloadRate, value);
        }

        public RelayCommand<bool> CreateDeltaVersionCmd { get; set; }
        public async void CreateDeltaVersion(bool dryRun)
        {
            DeltaVersionResult = await MHYGameHelper.CreateDeltaVersion("pkg_version", SourcePath, null, Server.Api, null, true);
            
            if (!dryRun)
            {
                DownloadService dl = new DownloadService();
                FilesToDownload = DeltaVersionResult.DeltaFiles.Count();
                //string current;
                //dl.DownloadStarted += (object sender, DownloadStartedEventArgs e) =>
                //{
                //    current = e.FileName;
                //};
                dl.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs args) =>
                {
                    FilesToDownload--;
                };
                dl.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                {
                    DownloadRate = e.BytesPerSecondSpeed;
                };
                await MHYGameHelper.CreateDeltaVersion("pkg_version", SourcePath, dl, Server.Api, TargetPath, false);
            }
        }

        public GameVersionViewModel()
        {
            SelectSourceCmd = new RelayCommand(SelectSource);
            SelectTargetCmd = new RelayCommand(SelectTarget);
            CreateDeltaVersionCmd = new RelayCommand<bool>(CreateDeltaVersion);
        }
    }
}

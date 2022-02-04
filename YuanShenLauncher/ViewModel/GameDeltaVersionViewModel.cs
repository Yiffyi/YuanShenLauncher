using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Launcher.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Launcher.ViewModel
{
    public class GameDeltaVersionViewModel : ViewModelBase
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
                //BackgroundWorker verifyWorker = new BackgroundWorker();
                //verifyWorker.WorkerReportsProgress = true;
                //verifyWorker.DoWork += new DoWorkEventHandler((sender, args) =>
                //{
                //    DamagedFiles = MHYGameHelper.VerifyPackage(SourcePath, MHYGameHelper.ParsePkgVersion(Path.Combine(SourcePath, "Audio_Chinese_pkg_version")), verifyWorker.ReportProgress);
                //});
                //verifyWorker.ProgressChanged += new ProgressChangedEventHandler((sender, args) =>
                //{
                //    VerifyProgress = args.ProgressPercentage;
                //});

                //verifyWorker.RunWorkerAsync();
            }
        }

        private string targetPath;
        public string TargetPath
        {
            get => targetPath;
            set => Set(ref targetPath, value);
        }

        private bool busy;
        public bool Busy
        {
            get => busy;
            set
            {
                Set(ref busy, value);
                SelectSourceCmd.RaiseCanExecuteChanged();
                SelectTargetCmd.RaiseCanExecuteChanged();
                SolveDeltaVersionCmd.RaiseCanExecuteChanged();
                LinkDuplicatedFilesCmd.RaiseCanExecuteChanged();
                GenerateAria2ListCmd.RaiseCanExecuteChanged();
                StartAria2Cmd.RaiseCanExecuteChanged();
            }
        }

        private int progress;
        public int Progress
        {
            get => progress;
            set => Set(ref progress, value);
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

        private SolveDeltaVersionResult deltaVersionResult;
        public SolveDeltaVersionResult DeltaVersionResult
        {
            get => deltaVersionResult;
            set
            {
                Set(ref deltaVersionResult, value);
                DuplicatedFileSize = DeltaVersionResult.DuplicatedFiles.Sum(item => item.FileSize);
                DeltaFileSize = DeltaVersionResult.DeltaFiles.Sum(item => item.FileSize);
            }
        }

        private List<string> localLanguagePacks;
        public List<string> LocalLanguagePacks
        {
            get => localLanguagePacks;
            set => Set(ref localLanguagePacks, value);
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

        public RelayCommand SolveDeltaVersionCmd { get; set; }
        public async void SolveDeltaVersion()
        {
            Busy = true;
            DeltaVersionResult = await MHYGameHelper.SolveDeltaVersion(SourcePath, Server.Api);
            Busy = false;
        }

        public RelayCommand LinkDuplicatedFilesCmd { get; set; }
        public void LinkDuplicatedFiles()
        {
            Task.Run(() =>
            {
                Busy = true;

                MHYGameHelper.LinkDeltaVersion(DeltaVersionResult, TargetPath, p =>
                {
                    if (p != Progress) Progress = p;
                });

                if (DeltaVersionResult.Sdk != null)
                {
                    Progress = 0;
                    MHYGameHelper.DownloadSdk(DeltaVersionResult.Sdk.Path, TargetPath, p =>
                    {
                        if (p != Progress) Progress = p;
                    });
                }
                MHYGameHelper.WriteIni(DeltaVersionResult, Server, TargetPath);

                Busy = false;
            });
        }

        private bool aria2Popup;
        public bool Aria2Popup
        {
            get => aria2Popup;
            set
            {
                Set(ref aria2Popup, value);
            }
        }

        private string aria2Workdir;

        private string aria2Cmdline;
        public string Aria2Cmdline
        {
            get => aria2Cmdline;
            set
            {
                Set(ref aria2Cmdline, value);
            }
        }

        public RelayCommand<string> GenerateAria2ListCmd { get; set; }
        public void GenerateAria2List(string choice)
        {
            SaveFileDialog d = new SaveFileDialog
            {
                Filter = "Aria2 输入列表|*.txt"
            };
            if ((bool)d.ShowDialog())
            {
                Aria2Popup = false;
                switch (choice)
                {
                    case "delta":
                        {
                            var r = DeltaVersionResult;
                            MHYGameHelper.PkgVersionToAria2(r.PkgVersions, r.DeltaFiles, r.DecompressedPath, TargetPath, new StreamWriter(d.FileName, false));

                            var dir = Path.GetDirectoryName(d.FileName);
                            var conf = Path.Combine(dir, "aria2.conf");
                            MHYGameHelper.RecommendedAria2Conf(new StreamWriter(conf, false));
                            NativeMethod.ExtractAria2(dir);

                            aria2Workdir = dir;
                            Aria2Cmdline = $"aria2c.exe --conf-path=aria2.conf --input-file={d.SafeFileName}";
                            Aria2Popup = true;
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException("Only 'delta' Allowed.");
                }

            }
        }

        public RelayCommand StartAria2Cmd { get; set; }
        public void StartAria2()
        {
            Task.Run(() =>
            {
                Busy = true;
                int ret = NativeMethod.RunCmd(Aria2Cmdline, workingDir: aria2Workdir, wait: true, pauseAfterFinish: true);
                Busy = false;
            });
        }

        public GameDeltaVersionViewModel()
        {
            SelectSourceCmd = new RelayCommand(SelectSource, () => !Busy);
            SelectTargetCmd = new RelayCommand(SelectTarget, () => !Busy);
            SolveDeltaVersionCmd = new RelayCommand(SolveDeltaVersion, () => !Busy);
            LinkDuplicatedFilesCmd = new RelayCommand(LinkDuplicatedFiles, () => !Busy && DeltaVersionResult != null);
            GenerateAria2ListCmd = new RelayCommand<string>(GenerateAria2List, (string _) => !Busy && DeltaVersionResult != null);
            StartAria2Cmd = new RelayCommand(StartAria2, () => !Busy);
        }
    }
}

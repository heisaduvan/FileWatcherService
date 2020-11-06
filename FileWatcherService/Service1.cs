using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace FileWatcherService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        private static List<FileSystemWatcher> fileSystemWatchers = new List<FileSystemWatcher>();
        private static string RootFolder = @"C:/Projects";
        private static string BackUpFolder = @"C:/BACKUP";
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void OnStart(string[] args)
        {
            Logger.SuccessLog("Servis çalışmaya başladı.");
            string[] subdirectoryEntries = Directory.GetDirectories(RootFolder);
            foreach(string path in subdirectoryEntries)
            {
                FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
                fileSystemWatcher.Path = path;
                fileSystemWatcher.IncludeSubdirectories = true;
                fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                fileSystemWatcher.Changed += OnChanged;
                fileSystemWatcher.Created += OnChanged;
                fileSystemWatcher.Deleted += OnChanged;
                fileSystemWatcher.Renamed += OnRenamed;
                fileSystemWatcher.EnableRaisingEvents = true;
                fileSystemWatchers.Add(fileSystemWatcher);
            }
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            CopyInitialize(e);
            Logger.SuccessLog($"File: {e.FullPath} {e.ChangeType}");
        }
        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            CopyInitialize(e);
            Logger.SuccessLog($"File: {e.OldFullPath} renamed to {e.FullPath}");
        }
        private static string GetSourcePath(FileSystemEventArgs eventArgs)
        {
            string[] folderRootNames = eventArgs.FullPath.Split('\\');
            if(folderRootNames != null)
                return folderRootNames[0] + "\\" + folderRootNames[1];
            Logger.ErrorLog("Dosya konumu null olamaz. " + eventArgs.FullPath);
            throw new Exception("Dosya konumu null olamaz.");
        }
        private static void CopyInitialize(FileSystemEventArgs eventArgs)
        {
            string sourcePath = GetSourcePath(eventArgs);
            var source = new DirectoryInfo(sourcePath);
            string folderRootName = sourcePath.Replace(RootFolder + "\\", "");
            string time = DateTime.Now.ToString("yyyyMMdd hhmmss tt", CultureInfo.InvariantCulture);
            string userName = Environment.UserName;
            string folderName = time + "_" + userName;
            string targetLocation = BackUpFolder + "\\" + folderRootName + "\\" + folderName;
            Directory.CreateDirectory(targetLocation);
            var target = new DirectoryInfo(targetLocation);
            CopyToBackupDirectory(source, target);
        }
        private static void CopyToBackupDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            try
            {
                foreach(FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }
                foreach(DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyToBackupDirectory(diSourceSubDir, nextTargetSubDir);
                }
            }
            catch(Exception ex)
            {
                Logger.SuccessLog("Dosyaların backup'ı alınırken hata oluştu." + ex.Message);
            }
            Logger.SuccessLog("Dosyaların backup'ı başarılı bir şekilde alındı.");
        }
        protected override void OnStop()
        {
            Logger.ErrorLog("Servis durdu.");
        }
    }
}

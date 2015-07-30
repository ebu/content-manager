using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace io.ebu.eis.data.file
{
    public class SystemWatchfolder
    {
        private readonly string _path;
        private readonly string _filter;
        private readonly bool _readInitialFiles;
        private readonly List<FileSystemWatcher> _watchers;
        private readonly ISystemFileRouter _router;

        public SystemWatchfolder(string path, string watchFilter, bool readInitialFiles, ISystemFileRouter router)
        {
            _path = path;
            _filter = watchFilter;
            _readInitialFiles = readInitialFiles;
            _router = router;
            
            _watchers = new List<FileSystemWatcher>();

            foreach (var f in _filter.Split('|'))
            {
                var watcher = new FileSystemWatcher
                {
                    Path = _path,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    Filter = f
                };
                // Add event handlers.
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                // Ignore deletions
                //watcher.Deleted += OnChanged;
                watcher.Renamed += OnRenamed;

                _watchers.Add(watcher);
            }
        }
        
        public void Start()
        {
            try
            {
                // Read initial files in folder
                if (_readInitialFiles)
                {
                    foreach (var f in Directory.EnumerateFiles(_path, _filter))
                    {
                        _router.RouteFile(f);
                    }
                }

                // Begin watching.
                foreach (var watcher in _watchers)
                {
                    watcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception e)
            {
                // TODO Log
            }
        }
        public void Stop()
        {
            try
            {
                // Stop watching.
                foreach (var watcher in _watchers)
                {
                    watcher.EnableRaisingEvents = false;
                }
            }
            catch (Exception e)
            {
                // TODO Log
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            // Ignore deleted
            if (e.ChangeType != WatcherChangeTypes.Deleted)
            {
                // If file still exists
                if (File.Exists(e.FullPath))
                {
                    // Route the trigger
                    _router.RouteFile(e.FullPath);
                }
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            // Route the trigger
            var filename = Path.GetFileName(e.FullPath);
            foreach(var f in _filter.Split('|'))
            {
                if (FitsMask(filename, f))
                {
                    _router.RouteFile(e.FullPath);
                    return;
                }
            }
        }

        private bool FitsMask(string fileName, string fileMask)
        {
            string pattern =
                 '^' +
                 Regex.Escape(fileMask.Replace(".", "__DOT__")
                                 .Replace("*", "__STAR__")
                                 .Replace("?", "__QM__"))
                                 .Replace("__DOT__", "[.]")
                                 .Replace("__STAR__", ".*")
                                 .Replace("__QM__", ".")
                 + '$';
            return new Regex(pattern, RegexOptions.IgnoreCase).IsMatch(fileName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace JealousSite
{
    public class Watcher : IDisposable
    {
        private readonly Builder builder = new Builder();
        private readonly FileSystemWatcher wStructure;
        private readonly FileSystemWatcher wTexts;

        public Watcher()
        {
            wStructure = new FileSystemWatcher("./structure");
            wStructure.IncludeSubdirectories = false;
            wStructure.Changed += onStructureChanged;
            wStructure.Created += onStructureChanged;
            wStructure.Renamed += onStructureChanged;
            wStructure.Filter = "*.html";
            wStructure.EnableRaisingEvents = true;
            wTexts = new FileSystemWatcher("./texts");
            wTexts.IncludeSubdirectories = true;
            wTexts.Changed += onTextChanged;
            wTexts.Created += onTextChanged;
            wTexts.Renamed += onTextChanged;
            wTexts.Filter = "*.html";
            wTexts.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            wStructure.Dispose();
            wTexts.Dispose();
        }

        private void onTextChanged(object sender, FileSystemEventArgs e)
        {
        }

        private void onStructureChanged(object sender, FileSystemEventArgs e)
        {
            builder.RebuildAll();
        }
    }
}

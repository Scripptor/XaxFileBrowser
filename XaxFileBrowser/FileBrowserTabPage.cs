using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XaxFileBrowser
{
    public class FileBrowserTabPage : TabPage
    {
        public delegate void PathChangedEvent(FileBrowserTabPage browserTab);

        public event PathChangedEvent PathChanged;

        FileBrowserControl fileBrowser;

        string directoryName, initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public string DirectoryName { get => directoryName; }
        public string FullPath { get => fileBrowser.Path; }

        public string InitialDirectory { get => initialDirectory; }

        public FileBrowserTabPage(string initialDirectory = "") : base()
        {
            if (string.IsNullOrEmpty(initialDirectory))
                initialDirectory = this.initialDirectory;

            fileBrowser = new FileBrowserControl();
            fileBrowser.PathChanged += FileBrowser_PathChanged;
            fileBrowser.TabOpened += FileBrowser_TabOpened;
            if (initialDirectory.ToUpper().Equals("%ROOT%"))
            {
                fileBrowser.GotoRoot();
            }
            else
            {
                fileBrowser.Path = initialDirectory;
            }
            Padding = new Padding(3);
            Controls.Add(fileBrowser);
        }

        public void Close()
        {
            Parent.Controls.Remove(this);
        }

        private void FileBrowser_TabOpened(string directory)
        {
            FileBrowserTabPage newTab = new FileBrowserTabPage(directory);
            Parent.Controls.Add(newTab);
        }

        private void FileBrowser_PathChanged(string directory)
        {
            Text = directory;
            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            directoryName = dirInfo.Exists ? dirInfo.Name : directory;
            PathChanged?.Invoke(this);
        }
    }
}

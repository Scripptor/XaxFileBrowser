using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XaxFileBrowser
{
    public partial class XaxFileBrowserForm : Form
    {
        FileBrowserTabPage fileBrowserTab;
        string title = "Xax File Browser v0.1 - ";
        string folderBrowsing = "sad asdas";

        public XaxFileBrowserForm()
        {
            InitializeComponent();
            // browserTabs.TabPages.Insert(index, tabPage) won't work without the below line!
            _ = browserTabs.Handle;

            Text = "Xax File Browser v0.1 - ";
            fileBrowserTab = new FileBrowserTabPage("%ROOT%");//(@"C:\");// Users\Zachary\Pictures\";
            AddTab(fileBrowserTab);
            AddTab(new FileBrowserTabPage());

            browserTabs.KeyDown += BrowserTabs_KeyDown;
            browserTabs.Selected += BrowserTabs_Selected;
            newToolStripMenuItem.Click += NewToolStripMenuItem_Click;
            newToolStripButton.Click += NewToolStripMenuItem_Click;
            openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            openToolStripButton.Click += OpenToolStripMenuItem_Click;
            browserTabs.Selecting += BrowserTabs_Selecting;
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;

            foreach (var tab in browserTabs.TabPages)
            {
                if (tab is FileBrowserTabPage)
                    (tab as FileBrowserTabPage).PathChanged += Form1_PathChanged;
            }

            var _tab = browserTabs.SelectedTab as FileBrowserTabPage;
            if (_tab is not null)
                Form1_PathChanged(_tab);
        }

        private void AddTab(TabPage page)
        {
            int lastIndex = browserTabs.TabCount - 1;
            browserTabs.TabPages.Insert(lastIndex, page);
            
            browserTabs.SelectedIndex = lastIndex;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

            if (folderBrowser.ShowDialog() != DialogResult.OK)
                return;

            var dir = new System.IO.DirectoryInfo(folderBrowser.SelectedPath);
            if (!dir.Exists)
                return;
            var tab = new FileBrowserTabPage(folderBrowser.SelectedPath);
            AddTab(tab);
            browserTabs.SelectedTab = tab;
        }

        private void BrowserTabs_Selecting(object sender, TabControlCancelEventArgs e)
        {
            var tab = e.TabPage;

            if (tab is not FileBrowserTabPage)
            {
                //e.Cancel = true;
                FileBrowserTabPage newTab1 = new FileBrowserTabPage("%ROOT%");
                AddTab(newTab1);
                //browserTabs.SelectedIndex = 0;
            }
        }

        private void BrowserTabs_Selected(object sender, TabControlEventArgs e)
        {
            FileBrowserTabPage tab = e.TabPage as FileBrowserTabPage;
            if (tab is not null)
                Form1_PathChanged(tab);
        }

        private void Form1_PathChanged(FileBrowserTabPage tab)
        {
            Text = title + tab.DirectoryName;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTab(new FileBrowserTabPage("%ROOT%"));
        }

        private void BrowserTabs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (e.Shift)
                {
                    // close all tabs
                    foreach (var tab in browserTabs.TabPages)
                    {
                        if (tab is FileBrowserTabPage)
                            (tab as FileBrowserTabPage).Close();
                    }
                }
                else
                {
                    var selectedTab = browserTabs.SelectedTab as FileBrowserTabPage;
                    if (selectedTab is null)
                        return;
                    selectedTab.Close();
                }
            }
        }
    }
}

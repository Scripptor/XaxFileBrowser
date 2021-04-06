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
            Text = "Xax File Browser v0.1 - ";
            fileBrowserTab = new FileBrowserTabPage("%ROOT%");//(@"C:\");// Users\Zachary\Pictures\";
            browserTabs.TabPages.Add(fileBrowserTab);
            browserTabs.TabPages.Add(new FileBrowserTabPage());

            browserTabs.KeyDown += BrowserTabs_KeyDown;
            browserTabs.Selected += BrowserTabs_Selected;
            newToolStripMenuItem.Click += NewToolStripMenuItem_Click;

            foreach (var tab in browserTabs.TabPages)
            {
                if (tab is FileBrowserTabPage)
                    (tab as FileBrowserTabPage).PathChanged += Form1_PathChanged;
            }

            var _tab = browserTabs.SelectedTab as FileBrowserTabPage;
            if (_tab is not null)
                Form1_PathChanged(_tab);
        }

        private void BrowserTabs_Selected(object sender, TabControlEventArgs e)
        {
            FileBrowserTabPage tab = browserTabs.SelectedTab as FileBrowserTabPage;
            if (tab is not null)
                Form1_PathChanged(tab);
        }

        private void Form1_PathChanged(FileBrowserTabPage tab)
        {
            Text = title + tab.DirectoryName;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            browserTabs.TabPages.Add(new FileBrowserTabPage("%ROOT%"));
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XaxFileBrowser
{
    public class FileBrowserControl : ListView
    {
        public static FileTypeInfoStorage KnownFileTypes = FileTypeInfoStorage.Load("FileTypes.xml");
        private static ImageList iconCache = new ImageList();

        #region Delegates
        public delegate void DirectoryChangedEvent(string directory);
        public delegate void TabOpenedEvent(string directory);
        #endregion

        #region Events
        public event DirectoryChangedEvent PathChanged;
        public event TabOpenedEvent TabOpened;
        #endregion

        #region Members
        ListViewGroup directoryGroup, fileGroup;
        string path = "";
        int nameColWidth = 256;
        bool controlKeyPressed = false;
        Stack<string> previousDirectories = new Stack<string>();
        bool inRoot = false;
        #endregion

        #region Properties
        public string Path { get => path; set => TryChangePath(value); }
        #endregion

        public FileBrowserControl()
        {
            directoryGroup = new ListViewGroup("directory", "Directory");
            fileGroup = new ListViewGroup("file", "File");

            //Icon icon = Icon.ExtractAssociatedIcon("C:/Windows/");
            if (!iconCache.Images.ContainsKey("%dir%"))
                iconCache.Images.Add("%dir%", SystemIcons.Shield);//icon);
            Dock = DockStyle.Fill;
            View = View.Details;
            SmallImageList = iconCache;

            AllowColumnReorder = true;
           // Sorting = SortOrder.Ascending;
            GridLines = true;
            FullRowSelect = true;
            //CheckBoxes = true;
            LabelEdit = false;
            LabelWrap = false;

            HideSelection = false;
            UseCompatibleStateImageBehavior = false;

            Columns.Add("name", "Name", nameColWidth);
            Columns.Add("type", "Type", -2);
            Columns.Add("dom", "Date Modified", -2);

            SelectedIndexChanged += FileBrowserControl_SelectedIndexChanged;
            ItemActivate += FileBrowserControl_ItemActivate;
            KeyDown += FileBrowserControl_KeyDown;
            KeyUp += FileBrowserControl_KeyUp;
            ColumnWidthChanged += FileBrowserControl_ColumnWidthChanged;
        }

        private void FileBrowserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (!e.Control)
                controlKeyPressed = false;
        }

        private void FileBrowserControl_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            var nameCol = Columns["name"];
            if (nameCol.Index != e.ColumnIndex)
                return;

            nameColWidth = nameCol.Width;
        }

        private void FileBrowserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
                controlKeyPressed = true;

            if (e.KeyCode == Keys.Back)
            {
                if (previousDirectories.Count == 0)
                    return;

                string prevDir = previousDirectories.Pop();
                if (prevDir.Equals("%ROOT%"))
                    GotoRoot();
                else
                {
                    Path = prevDir;
                    previousDirectories.Pop(); // lazy lazy lazy
                }
            }
        }

        private void FileBrowserControl_ItemActivate(object sender, EventArgs e)
        {
            /*if (string.IsNullOrEmpty(path))
                return;*/

            string npath = inRoot ? "" : path;

            if (SelectedItems?.Count > 0)
            {
                if (SelectedItems.Count > 2
                    && MessageBox.Show("About to launch {SelectedItems.Count} files, this could take long, so are you sure?", "Launching many files.", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

                foreach (ListViewItem item in SelectedItems)
                {
                    string filepath = System.IO.Path.Join(npath, item.Text);

                    //if (item.Group.Equals(directoryGroup))
                    if (filepath.EndsWith("\\") || filepath.EndsWith("/"))
                    {
                        DirectoryInfo dir = new DirectoryInfo(filepath);
                        if (!dir.Exists)
                            return;
                        if (controlKeyPressed)
                            TabOpened?.Invoke(dir.FullName);
                        else
                            Path = dir.FullName;
                        return;
                    }
                    //else if (item.Group.Equals(fileGroup))
                    else
                    {
                        FileInfo fileInfo = new FileInfo(filepath);
                        try
                        {
                            var process = new System.Diagnostics.Process();
                            process.StartInfo = new System.Diagnostics.ProcessStartInfo(@$"{filepath}")
                            {
                                UseShellExecute = true
                            };
                            process.Start();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, $"{fileInfo.Name} Failed to launch!");
                        }
                    }
                }
            }
        }

        private void FileBrowserControl_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void GotoRoot()
        {
            inRoot = true;
            path = "";
            Items.Clear();

            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                var item = new ListViewItem(drive.Name);
                item.SubItems.Add(drive.DriveType.ToString());
                Items.Add(item);
            }

            PathChanged?.Invoke("This PC");
        }

        private void TryChangePath(string path)
        {
            string oldPath = this.path;
            bool wasInRoot = inRoot;
            try
            {
                if (string.IsNullOrEmpty(path))
                    return;
                if (!Directory.Exists(path))
                {
                    MessageBox.Show($"Couldn't change path to the specified location \"{path}\", it doesn't exist!", "Error: Directory doesn't exist!", MessageBoxButtons.OK);
                    return;
                }

                if (!string.IsNullOrEmpty(this.path))
                    previousDirectories.Push(this.path);
                else if (inRoot)
                    previousDirectories.Push("%ROOT%");
                
                this.path = path;
                inRoot = false;
                // remove all existing children
                Items.Clear();

                string[] childDirs = Directory.GetDirectories(path);
                string[] childFiles = Directory.GetFiles(path);
                BeginUpdate();
                foreach (string dirPath in childDirs)
                {
                    DirectoryInfo dir = new DirectoryInfo(dirPath);
                    Icon icon = SystemIcons.WinLogo;

                    ListViewItem dirItem = new ListViewItem(dir.Name + "\\");
                    dirItem.SubItems.Add("Directory");
                    dirItem.SubItems.Add(dir.LastWriteTime.ToString());
                    dirItem.Group = directoryGroup;
                    string iconKey = "%dir%";

                    /*if (!icons.Images.ContainsKey(dir.FullName))
                    {
                        icon = Icon.ExtractAssociatedIcon(dir.FullName);
                        if (icon.GetHashCode() != icons.Images[iconKey].GetHashCode())
                        {
                            iconKey = dir.FullName;
                            icons.Images.Add(iconKey, icon);
                        }
                    }*/
                    dirItem.ImageKey = iconKey;

                    if (dir.Attributes.HasFlag(FileAttributes.System))
                        dirItem.BackColor = Color.Goldenrod;
                    else if (dir.Attributes.HasFlag(FileAttributes.ReadOnly))
                        dirItem.BackColor = Color.GreenYellow;
                    else if (dir.Attributes.HasFlag(FileAttributes.Hidden))
                        dirItem.BackColor = Color.Magenta;
                    else
                        dirItem.BackColor = Color.Wheat;

                    if (dir.Name == "Windows")
                    {
                        string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        if (dir.FullName.ToUpper().Equals(winDir))
                            dirItem.BackColor = Color.Cyan;
                    }

                    Items.Add(dirItem);
                }

                foreach (string filePath in childFiles)
                {
                    FileInfo file = new FileInfo(filePath);

                    Icon icon = SystemIcons.Shield;
                    ListViewItem fileItem = new ListViewItem(file.Name, fileGroup);

                    fileItem.SubItems.Add(GetFileTypeInfo(file.Extension));
                    fileItem.SubItems.Add(file.LastWriteTime.ToString());


                    string iconKey = file.Extension;
                    if (!iconCache.Images.ContainsKey(file.Extension))
                    {
                        if (file.Extension == ".ico")
                            iconKey = file.FullName;

                        icon = Icon.ExtractAssociatedIcon(file.FullName);
                        iconCache.Images.Add(iconKey, icon);
                    }
                    fileItem.ImageKey = iconKey;

                    if (file.IsReadOnly)
                    {
                        fileItem.BackColor = Color.DimGray;
                        fileItem.ForeColor = Color.White;
                    }
                    if (file.Attributes.HasFlag(FileAttributes.System))
                    {
                        fileItem.BackColor = Color.DarkSlateGray;
                        if (file.IsReadOnly)
                            fileItem.BackColor = Color.SlateGray;
                        else
                            fileItem.ForeColor = Color.LightSlateGray;
                    }
                    else if (file.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        fileItem.BackColor = Color.Black;
                        if (file.IsReadOnly)
                            fileItem.ForeColor = Color.Gray;
                        else
                            fileItem.ForeColor = Color.White;
                    }
                    else if (file.Attributes.HasFlag(FileAttributes.Temporary))
                    {
                        fileItem.BackColor = Color.Black;
                        if (file.IsReadOnly)
                            fileItem.ForeColor = Color.Gray;
                        else
                            fileItem.ForeColor = Color.White;
                    }
                    else if (file.Attributes.HasFlag(FileAttributes.Archive))
                    {
                        fileItem.BackColor = Color.Aquamarine;
                        if (file.IsReadOnly)
                            fileItem.ForeColor = Color.Gray;
                        else
                            fileItem.ForeColor = Color.Black;
                    }
                    else if (file.Attributes.HasFlag(FileAttributes.Compressed))
                    {
                        fileItem.BackColor = Color.Purple;
                        if (file.IsReadOnly)
                            fileItem.ForeColor = Color.Gray;
                        else
                            fileItem.ForeColor = Color.Aqua;
                    }
                    else if (file.Attributes.HasFlag(FileAttributes.Encrypted))
                    {
                        fileItem.BackColor = Color.Orange;
                        if (file.IsReadOnly)
                            fileItem.ForeColor = Color.DarkGray;
                        else
                            fileItem.ForeColor = Color.DarkOrange;
                    }
                    else if (file.Attributes.HasFlag(FileAttributes.Device))
                    {
                        fileItem.BackColor = Color.DarkRed;
                        if (file.IsReadOnly)
                            fileItem.ForeColor = Color.LightPink;
                        else
                            fileItem.ForeColor = Color.LightSalmon;
                    }
                    Items.Add(fileItem);
                }
                for (int i = 1; i < Columns.Count; i++)
                {
                    AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                }
                EndUpdate();
                PathChanged?.Invoke(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Couldn't change directory!");
                this.path = oldPath;
                TryChangePath(oldPath);
                previousDirectories.Pop();
                previousDirectories.Pop();
                inRoot = wasInRoot;
            }
        }

        private string GetFileTypeInfo(string extension)
        {
            if (string.IsNullOrEmpty(extension) || !extension.StartsWith("."))
                return "Unspecified File Type";

            extension = extension.Substring(1, extension.Length-1).ToLower();

            foreach (var fileType in KnownFileTypes.FileTypes)
            {
                foreach (string typeExtension in fileType.Extensions)
                {
                    if (extension.Equals(typeExtension))
                        return fileType.Name;
                }
            }

            return "Unknown File Type";
        }
    }
}

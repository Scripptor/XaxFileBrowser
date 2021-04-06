using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XaxFileBrowser
{
    [Serializable]
    public class FileTypeInfoStorage
    {
        List<FileTypeInfo> fileTypes = new List<FileTypeInfo>();

        public List<FileTypeInfo> FileTypes { get => fileTypes; private set => fileTypes = value; }

        public static FileTypeInfoStorage Load(string filepath)
        {
            FileTypeInfoStorage storage = new FileTypeInfoStorage();

            if (!File.Exists(filepath))
            {
                //MessageBox.Show();
                return storage;
            }

            string xml = "";

            using (var reader = new StreamReader(filepath))
                xml = reader.ReadToEnd();

            XDocument xdoc = XDocument.Parse(xml);
            XElement root = xdoc.Root;
            if (root is null)
                return storage;

            if (root.Name != "FileTypes")
                return storage;

            Type fitType = typeof(FileTypeInfo);
            FieldInfo nameField = fitType.GetField("name", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo extensionsField = fitType.GetField("extensions", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (XElement child in root.Elements("FileType"))
            {
                FileTypeInfo fileType = new FileTypeInfo();
                string name = "";
                List<string> extensions = new List<string>();

                var xName = child.Element("Name");
                if (xName is not null)
                    name = xName.Value;
                var xExtensions = child.Element("Extensions");
                if (xExtensions is not null)
                {
                    foreach (var ext in xExtensions.Elements("Ext"))
                    {
                        extensions.Add(ext.Value);
                    }
                }
                nameField.SetValue(fileType, name);
                extensionsField.SetValue(fileType, extensions);
                storage.FileTypes.Add(fileType);
            }

            return storage;
        }
    }
}

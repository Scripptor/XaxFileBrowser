using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XaxFileBrowser
{
    [Serializable]
    public class FileTypeInfo
    {
        string name;
        List<string> extensions;

        public string Name { get => name; private set => name = value; }
        public List<string> Extensions { get => extensions; private set => extensions = value; }
    }
}

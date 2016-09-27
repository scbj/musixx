using Musixx.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musixx.Models
{
    public class File : IFile
    {
        public string Id { get; }

        public string Name { get; }

        public string MD5 { get; }

        public long Size { get; }

        public Uri Uri { get; }

        public File(string id, string name, string md5, long size, string path)
        {
            Id = id;
            Name = name;
            MD5 = md5;
            Size = size;
            Uri = new Uri(path);
        }
    }
}

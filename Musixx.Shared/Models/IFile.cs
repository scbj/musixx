using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musixx.Shared.Models
{
    public interface IFile
    {
        string Id { get; }
        string Name { get; }
        string MD5 { get; }
        long Size { get; }
        Uri Uri { get; }
    }
}

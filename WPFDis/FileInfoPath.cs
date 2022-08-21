using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFDis
{
    public struct FileInfoPath
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileOccupiedPlace { get; set; }

        public FileInfoPath(string FileName, string FileType, string FileOccupiedPlace) 
        { 
            this.FileName = FileName;
            this.FileType = FileType;
            this.FileOccupiedPlace = FileOccupiedPlace;
        }
    }
}

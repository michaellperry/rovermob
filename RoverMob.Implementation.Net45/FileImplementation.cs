using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Implementation
{
    public static class FileImplementation
    {
        public static async Task<Stream> OpenForRead(string folderName, string fileName)
        {
            var path = GetPath(folderName, fileName);
            return File.OpenRead(path);
        }

        public static async Task<Stream> OpenForWrite(string folderName, string fileName)
        {
            var path = GetPath(folderName, fileName);
            return File.OpenWrite(path);
        }

        private static string GetPath(string folderName, string fileName)
        {
            var applicationData = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.Create);
            var path = Path.Combine(applicationData, folderName, fileName);
            return path;
        }
    }
}

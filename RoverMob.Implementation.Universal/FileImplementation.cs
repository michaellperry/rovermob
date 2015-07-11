using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace RoverMob.Implementation
{
    public static class FileImplementation
    {
        public static async Task<Stream> OpenForRead(string folderName, string fileName)
        {
            var RoverMobFolder = await ApplicationData.Current.LocalFolder
                 .CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
            var file = await RoverMobFolder
                .CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            return await file.OpenStreamForReadAsync();
        }

        public static async Task<Stream> OpenForWrite(string folderName, string fileName)
        {
            var RoverMobFolder = await ApplicationData.Current.LocalFolder
                 .CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
            var file = await RoverMobFolder
                .CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            return await file.OpenStreamForWriteAsync();
        }
    }
}

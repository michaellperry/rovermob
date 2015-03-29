using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;

namespace RoverMob.Messaging
{
    public class FileBookmarkStore : IBookmarkStore
    {
        private readonly string _folderName;

        public FileBookmarkStore(string folderName)
        {
            _folderName = folderName;
        }

        public async Task<string> LoadBookmarkAsync(string topic)
        {
            var file = await CreateFileAsync(topic);
            var stream = await file.OpenStreamForReadAsync();
            using (var reader = new StreamReader(stream))
            {
                var bookmark = reader.ReadToEnd();
                return bookmark;
            }
        }

        public async Task SaveBookmarkAsync(string topic, string bookmark)
        {
            var file = await CreateFileAsync(topic);
            var stream = await file.OpenStreamForWriteAsync();
            stream.SetLength(0);
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine(bookmark);
            }
        }

        private async Task<StorageFile> CreateFileAsync(string bookmark)
        {
            var RoverMobFolder = await ApplicationData.Current.LocalFolder
                .CreateFolderAsync(_folderName, CreationCollisionOption.OpenIfExists);
            string fileName = String.Format("b_{0}.txt", bookmark);
            var bookmarkFile = await RoverMobFolder
                .CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            return bookmarkFile;
        }
    }
}

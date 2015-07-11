using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RoverMob.Implementation;

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
            var stream = await OpenForRead(topic);
            using (var reader = new StreamReader(stream))
            {
                var bookmark = reader.ReadToEnd();
                return bookmark;
            }
        }

        public async Task SaveBookmarkAsync(string topic, string bookmark)
        {
            Stream stream = await OpenForWrite(topic);
            stream.SetLength(0);
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine(bookmark);
            }
        }

        private async Task<Stream> OpenForRead(string topic)
        {
            string fileName = String.Format("b_{0}.txt", topic);
            return await FileImplementation.OpenForRead(
                _folderName, fileName);
        }

        private async Task<Stream> OpenForWrite(string topic)
        {
            string fileName = String.Format("b_{0}.txt", topic);
            return await FileImplementation.OpenForWrite(
                _folderName, fileName);
        }
    }
}

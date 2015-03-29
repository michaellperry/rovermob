using RoverMob.Protocol;
using RoverMob.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace RoverMob.Messaging
{
    public class FileMessageStore : Process, IMessageStore
    {
        private readonly string _folderName;

        private JsonSerializer _serializer = new JsonSerializer();

        public FileMessageStore(string folderName)
        {
            _folderName = folderName;
        }

        public Task<ImmutableList<Message>> LoadAsync(Guid objectId)
        {
            var completion = new TaskCompletionSource<ImmutableList<Message>>();
            Perform(() => LoadInternalAsync(objectId, completion));
            return completion.Task;
        }

        public void Save(Message message)
        {
            Perform(() => SaveInternalAsync(message));
        }

        private async Task LoadInternalAsync(Guid objectId, TaskCompletionSource<ImmutableList<Message>> completion)
        {
            try
            {
                var file = await CreateFileAsync(objectId);
                var messages = await ReadMessagesAsync(file);
                var result = messages
                    .Select(m => Message.FromMemento(m))
                    .ToImmutableList();
                completion.SetResult(result);
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        }

        private async Task SaveInternalAsync(Message message)
        {
            var file = await CreateFileAsync(message.ObjectId);
            var messages = await ReadMessagesAsync(file);
            if (!messages.Any(m => m.Hash == message.Hash.ToString()))
            {
                messages.Add(message.GetMemento());
                await WriteMessagesAsync(file, messages);
            }
        }

        private async Task<StorageFile> CreateFileAsync(Guid objectId)
        {
            var RoverMobFolder = await ApplicationData.Current.LocalFolder
                .CreateFolderAsync(_folderName, CreationCollisionOption.OpenIfExists);
            string fileName = String.Format("obj_{0}.json",
                objectId.ToCanonicalString());
            var objectFile = await RoverMobFolder
                .CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            return objectFile;
        }

        private async Task<List<MessageMemento>> ReadMessagesAsync(StorageFile objectFile)
        {
            List<MessageMemento> messageList;
            var inputStream = await objectFile.OpenStreamForReadAsync();
            using (JsonReader reader = new JsonTextReader(new StreamReader(inputStream)))
            {
                messageList = _serializer.Deserialize<List<MessageMemento>>(reader);
            }

            if (messageList == null)
                messageList = new List<MessageMemento>();
            return messageList;
        }

        private async Task WriteMessagesAsync(StorageFile objectFile, List<MessageMemento> messageList)
        {
            var outputStream = await objectFile.OpenStreamForWriteAsync();
            using (JsonWriter writer = new JsonTextWriter(new StreamWriter(outputStream)))
            {
                _serializer.Serialize(writer, messageList);
            }
        }
    }
}

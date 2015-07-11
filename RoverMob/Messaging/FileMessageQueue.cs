using RoverMob.Protocol;
using RoverMob.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RoverMob.Implementation;

namespace RoverMob.Messaging
{
    public class FileMessageQueue : Process, IMessageQueue
    {
        private readonly string _folderName;

        private JsonSerializer _serializer = new JsonSerializer();

        public FileMessageQueue(string folderName)
        {
            _folderName = folderName;
        }

        public void Enqueue(Message message)
        {
            Perform(() => EnqueueInternalAsync(message));
        }

        private async Task EnqueueInternalAsync(Message message)
        {
            var messageList = await ReadMessagesAsync();

            var memento = message.GetMemento();
            messageList.Add(memento);

            await WriteMessagesAsync(messageList);
        }

        public void Confirm(Message message)
        {
            Perform(() => ConfirmInternalAsync(message));
        }

        private async Task ConfirmInternalAsync(Message message)
        {
            var messageList = await ReadMessagesAsync();

            string hash = message.Hash.ToString();
            messageList.RemoveAll(o => o.Hash == hash);

            await WriteMessagesAsync(messageList);
        }

        public Task<ImmutableList<Message>> LoadAsync()
        {
            var completion = new TaskCompletionSource<ImmutableList<Message>>();
            Perform(() => LoadInternalAsync(completion));
            return completion.Task;
        }

        private async Task LoadInternalAsync(TaskCompletionSource<ImmutableList<Message>> completion)
        {
            try
            {
                var messages = await ReadMessagesAsync();
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

        private async Task<List<MessageMemento>> ReadMessagesAsync()
        {
            List<MessageMemento> messageList;
            var inputStream = await FileImplementation.OpenForRead(
                _folderName, "MessageQueue.json");
            using (JsonReader reader = new JsonTextReader(new StreamReader(inputStream)))
            {
                messageList = _serializer.Deserialize<List<MessageMemento>>(reader);
            }

            if (messageList == null)
                messageList = new List<MessageMemento>();
            return messageList;
        }

        private async Task WriteMessagesAsync(List<MessageMemento> messageList)
        {
            var outputStream = await FileImplementation.OpenForWrite(
                _folderName, "MessageQueue.json");
            outputStream.SetLength(0);
            using (JsonWriter writer = new JsonTextWriter(new StreamWriter(outputStream)))
            {
                _serializer.Serialize(writer, messageList);
            }
        }
    }
}

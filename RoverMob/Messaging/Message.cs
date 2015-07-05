using RoverMob.Protocol;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.IO;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace RoverMob.Messaging
{
    public class Message
    {
        private readonly ImmutableList<string> _topics;
        private readonly string _type;
        private readonly ImmutableList<Predecessor> _predecessors;
        private readonly Guid _objectId;
        private readonly ExpandoObject _body;
        private readonly MessageHash _hash;

        private Message(
            ImmutableList<string> topics,
            string type,
            ImmutableList<Predecessor> predecessors,
            Guid objectId,
            ExpandoObject body,
            MessageHash hash)
        {
            _topics = topics;
            _type = type;
            _predecessors = predecessors;
            _objectId = objectId;
            _body = body;
            _hash = hash;
        }

        public ImmutableList<string> Topics
        {
            get { return _topics; }
        }

        public string Type
        {
            get { return _type; }
        }

        public ImmutableList<MessageHash> GetPredecessors(string role)
        {
            return _predecessors
                .Where(p => p.Role == role)
                .Select(p => p.Hash)
                .ToImmutableList();
        }

        public Guid ObjectId
        {
            get { return _objectId; }
        }

        public dynamic Body
        {
            get { return _body; }
        }

        public MessageHash Hash
        {
            get { return _hash; }
        }

        public static Message CreateMessage(
            string topic,
            string messageType,
            Guid objectId,
            object body)
        {
            return CreateMessage(
                topic,
                messageType,
                Predecessors.Set,
                objectId,
                body);
        }

        public static Message CreateMessage(
            string topic,
            string messageType,
            Predecessors predecessors,
            Guid objectId,
            object body)
        {
            return CreateMessage(
                new TopicSet().Add(topic),
                messageType,
                predecessors,
                objectId,
                body);
        }

        public static Message CreateMessage(
            TopicSet topicSet,
            string messageType,
            Predecessors predecessors,
            Guid objectId,
            object body)
        {
            // Convert the anonymous typed object to an ExpandoObject.
            var expandoBody = JsonConvert.DeserializeObject<ExpandoObject>(
                JsonConvert.SerializeObject(body));
            var predecessorList = predecessors.ToImmutableList();
            object document = new
            {
                MessageType = messageType,
                Predecessors = predecessorList
                    .Select(p => new
                    {
                        Role = p.Role,
                        Hash = p.Hash.ToString()
                    })
                    .ToArray(),
                ObjectId = objectId,
                Body = expandoBody
            };
            var messageHash = new MessageHash(ComputeHash(document));

            return new Message(
                topicSet.ToImmutableList(),
                messageType,
                predecessorList,
                objectId,
                expandoBody,
                messageHash);
        }

        private static byte[] ComputeHash(object document)
        {
            var sha = new Sha256Digest();
            var stream = new DigestStream(new MemoryStream(), null, sha);
            using (var writer = new StreamWriter(stream))
            {
                string mementoToString = JsonConvert.SerializeObject(document);
                writer.Write(mementoToString);
            }
            byte[] buffer = new byte[sha.GetDigestSize()];
            sha.DoFinal(buffer, 0);
            return buffer;
        }

        public MessageMemento GetMemento()
        {
            return new MessageMemento()
            {
                Topics = Topics.ToList(),
                Hash = Hash.ToString(),
                MessageType = Type,
                Predecessors = _predecessors
                    .Select(p => new PredecessorMemento
                    {
                        Role = p.Role,
                        Hash = p.Hash.ToString()
                    })
                    .ToList(),
                ObjectId = ObjectId,
                Body = Body
            };
        }

        public static Message FromMemento(MessageMemento memento)
        {
            return new Message(
                memento.Topics.ToImmutableList(),
                memento.MessageType,
                memento.Predecessors
                    .Select(h => new Predecessor(h.Role, MessageHash.Parse(h.Hash)))
                    .ToImmutableList(),
                memento.ObjectId,
                memento.Body,
                MessageHash.Parse(memento.Hash));
        }
    }
}

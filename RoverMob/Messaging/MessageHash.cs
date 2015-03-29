using System;
using System.Linq;

namespace RoverMob.Messaging
{
    public class MessageHash : IComparable<MessageHash>
    {
        private byte[] _code;

        public MessageHash(byte[] code)
        {
            _code = code;
        }

        public byte[] Code
        {
            get { return _code; }
        }

        public override string ToString()
        {
            return Convert.ToBase64String(Code);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var that = (MessageHash)obj;
            return Enumerable.SequenceEqual(this._code, that._code);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var b in _code)
                hash = hash * 37 + b;

            return hash;
        }

        public int CompareTo(MessageHash other)
        {
            for(int index = 0; index < _code.Length; index++)
            {
                if (_code[index] < other._code[index])
                    return -1;
                if (_code[index] > other._code[index])
                    return 1;
            }

            return 0;
        }

        public static MessageHash Parse(string str)
        {
            return new MessageHash(Convert.FromBase64String(str));
        }
    }
}

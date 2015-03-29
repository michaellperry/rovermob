/*
 * 
 * LICENSE
 * 
 * Copyright (c) 2000 - 2011 The Legion of the Bouncy Castle Inc.
 * (http://www.bouncycastle.org)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the 
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies
 * or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 * 
 */

using System;
using System.IO;

namespace Org.BouncyCastle.Crypto.IO
{
	public class DigestStream
		: Stream
	{
		protected readonly Stream stream;
		protected readonly IDigest inDigest;
		protected readonly IDigest outDigest;

		public DigestStream(
			Stream	stream,
			IDigest	readDigest,
			IDigest	writeDigest)
		{
			this.stream = stream;
			this.inDigest = readDigest;
			this.outDigest = writeDigest;
		}

		public virtual IDigest ReadDigest()
		{
			return inDigest;
		}

		public virtual IDigest WriteDigest()
		{
			return outDigest;
		}

		public override int Read(
			byte[]	buffer,
			int		offset,
			int		count)
		{
			int n = stream.Read(buffer, offset, count);
			if (inDigest != null)
			{
				if (n > 0)
				{
					inDigest.BlockUpdate(buffer, offset, n);
				}
			}
			return n;
		}

		public override int ReadByte()
		{
			int b = stream.ReadByte();
			if (inDigest != null)
			{
				if (b >= 0)
				{
					inDigest.Update((byte)b);
				}
			}
			return b;
		}

		public override void Write(
			byte[]	buffer,
			int		offset,
			int		count)
		{
			if (outDigest != null)
			{
				if (count > 0)
				{
					outDigest.BlockUpdate(buffer, offset, count);
				}
			}
			stream.Write(buffer, offset, count);
		}

		public override void WriteByte(
			byte b)
		{
			if (outDigest != null)
			{
				outDigest.Update(b);
			}
			stream.WriteByte(b);
		}

		public override bool CanRead
		{
			get { return stream.CanRead; }
		}

		public override bool CanWrite
		{
			get { return stream.CanWrite; }
		}

		public override bool CanSeek
		{
			get { return stream.CanSeek; }
		}

		public override long Length
		{
			get { return stream.Length; }
		}

		public override long Position
		{
			get { return stream.Position; }
			set { stream.Position = value; }
		}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                stream.Dispose();

            base.Dispose(disposing);
        }

		public override  void Flush()
		{
			stream.Flush();
		}

		public override long Seek(
			long		offset,
			SeekOrigin	origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(
			long length)
		{
			stream.SetLength(length);
		}
	}
}


/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: https://catlib.io/
 */

using System;
using System.IO;

namespace CatLib
{
    /// <summary>
    /// Represents a ring buffer.
    /// </summary>
    public class RingBufferStream : Stream
    {
        private readonly long capacity;
        private readonly byte[] buffer;
        private readonly bool exposable;
        private readonly long mask;
        private long write;
        private long read;

        /// <summary>
        /// Initializes a new instance of the <see cref="RingBufferStream"/> class.
        /// </summary>
        /// <param name="capacity">Capacity, need to be the power of 2 (upward).</param>
        /// <param name="exposable">Whether possible to access internal arrays.</param>
        public RingBufferStream(int capacity = 8192, bool exposable = true)
        {
            Guard.Requires<ArgumentOutOfRangeException>(capacity > 0);
            this.capacity = capacity.ToPrime();
            buffer = new byte[this.capacity];
            mask = this.capacity - 1;
            write = 0;
            read = 0;
            this.exposable = exposable;
        }

        /// <summary>
        /// Gets the capacity of the ring buffer.
        /// </summary>
        public int Capacity => (int)capacity;

        /// <summary>
        /// Gets writeable count.
        /// </summary>
        public int WriteableCount => (int)GetCanWriteSize();

        /// <summary>
        /// Gets readable count.
        /// </summary>
        public int ReadableCount => (int)GetCanReadSize();

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override long Length => write;

        /// <inheritdoc />
        public override long Position { get => read; set => Seek(value, SeekOrigin.Begin); }

        /// <summary>
        /// Gets the original array of ring buffer.
        /// </summary>
        /// <returns>Returns the original array of ring buffer.</returns>
        public byte[] GetBuffer()
        {
            if (!exposable)
            {
                throw new UnauthorizedAccessException("Unable to access original array");
            }

            return buffer;
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            var readSize = Peek(buffer, offset, count);
            read += readSize;
            return readSize;
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            Guard.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(count >= 0);
            Guard.Requires<ArgumentOutOfRangeException>((buffer.Length - offset) >= count);

            var writeSize = GetCanWriteSize();

            if (writeSize < count)
            {
                throw new RuntimeException("The memory is no longer writable because the buffer area is full.");
            }

            if (writeSize > count)
            {
                writeSize = count;
            }

            if (writeSize <= 0)
            {
                return;
            }

            var nextWritePos = write + writeSize;
            var realWritePos = write & mask;
            var realNextWritePos = nextWritePos & mask;

            if (realNextWritePos >= realWritePos)
            {
                Buffer.BlockCopy(buffer, offset, this.buffer, (int)realWritePos, (int)writeSize);
            }
            else
            {
                var tail = (int)(capacity - realWritePos);
                Buffer.BlockCopy(buffer, offset, this.buffer, (int)realWritePos, tail);

                if ((writeSize - tail) > 0)
                {
                    Buffer.BlockCopy(buffer, offset + tail, this.buffer, 0, (int)writeSize - tail);
                }
            }

            write = nextWritePos;
        }

        /// <summary>
        /// Read the data of the ring buffer into <paramref name="buffer"/>,
        /// but do not advance the read position.
        /// </summary>
        /// <param name="buffer">The read data is filled into the current buffer.</param>
        /// <param name="offset">The starting offset of the buffer array.</param>
        /// <param name="count">How many lengths of data expected to read.</param>
        /// <returns>Actual read length.</returns>
        public int Peek(byte[] buffer, int offset, int count)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            Guard.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(count >= 0);
            Guard.Requires<ArgumentOutOfRangeException>((buffer.Length - offset) >= count);

            var readSize = GetCanReadSize();
            if (readSize > count)
            {
                readSize = count;
            }

            if (readSize <= 0)
            {
                return 0;
            }

            var nextReadPos = read + readSize;

            var realReadPos = read & mask;
            var realNextReadPos = nextReadPos & mask;

            if (realNextReadPos >= realReadPos)
            {
                Buffer.BlockCopy(this.buffer, (int)realReadPos, buffer, offset, (int)readSize);
            }
            else
            {
                var tail = (int)(capacity - realReadPos);
                Buffer.BlockCopy(this.buffer, (int)realReadPos, buffer, offset, tail);

                if (readSize - tail > 0)
                {
                    Buffer.BlockCopy(this.buffer, 0, buffer, offset + tail, (int)readSize - tail);
                }
            }

            return (int)readSize;
        }

        /// <summary>
        /// Clear the ring buffer.
        /// </summary>
        public void Clear()
        {
            write = 0;
            read = 0;
            Array.Clear(buffer, 0, buffer.Length);
        }

        /// <inheritdoc />
        public override void Flush()
        {
            // ignore.
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Get the size of the byte stream that can be read.
        /// </summary>
        /// <returns>todo:1.</returns>
        private long GetCanReadSize()
        {
            return write - read;
        }

        /// <summary>
        /// Get the size of the byte stream that can be written.
        /// </summary>
        private long GetCanWriteSize()
        {
            return Math.Max(0, capacity - GetCanReadSize());
        }
    }
}

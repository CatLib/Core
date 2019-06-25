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
using System.Threading;

namespace CatLib.Support
{
    /// <summary>
    /// The pipeline stream.
    /// </summary>
#pragma warning disable S3881
    public class PipelineStream : Stream
#pragma warning restore S3881
    {
        private readonly object locker;
        private readonly int capacity;
        private readonly int sleep;
        private readonly RingBufferStream ringBuffer;
        private volatile int count;
        private volatile bool disabled;
        private volatile bool closed;
        private long position;
        private long length;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineStream"/> class.
        /// </summary>
        /// <param name="capacity">The stream capacity.</param>
        /// <param name="sleep">The thread sleep time.</param>
        public PipelineStream(int capacity = 4096, int sleep = 1)
        {
            this.capacity = capacity.ToPrime();
            this.sleep = Math.Max(0, sleep);
            ringBuffer = new RingBufferStream(this.capacity, false);
            locker = new object();
        }

        /// <summary>
        /// Trigger when reading is complete.
        /// </summary>
        public event Action<Stream> OnRead;

        /// <inheritdoc />
        public override bool CanRead => count > 0 && !disabled;

        /// <inheritdoc />
        public override bool CanWrite => count < capacity && !closed;

        /// <inheritdoc />
        public override long Position
        {
            get => position;
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override long Length => length;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <summary>
        /// Gets a value indicating whether is closed the stream.
        /// </summary>
        public bool Closed => closed;

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            length = Math.Max(0, value);
        }

        /// <inheritdoc />
        public override void Flush()
        {
            // ignore
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            while (true)
            {
                while (this.count <= 0)
                {
                    AssertDisabled();
                    if (closed)
                    {
                        return 0;
                    }

                    Thread.Sleep(sleep);
                }

                AssertDisabled();

                lock (locker)
                {
                    AssertDisabled();
                    if (this.count <= 0)
                    {
                        if (closed)
                        {
                            return 0;
                        }

                        continue;
                    }

                    try
                    {
                        var read = ringBuffer.Read(buffer, offset, count);
                        this.count -= read;
                        position += read;

                        OnRead?.Invoke(this);

                        return read;
                    }
                    finally
                    {
                        Guard.Requires<AssertException>(this.count >= 0);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            while (true)
            {
                while ((capacity - this.count) < count)
                {
                    AssertDisabled();
                    AssertClosed();

                    Thread.Sleep(sleep);
                }

                AssertDisabled();
                AssertClosed();

                lock (locker)
                {
                    AssertDisabled();
                    AssertClosed();

                    if ((capacity - this.count) < count)
                    {
                        continue;
                    }

                    ringBuffer.Write(buffer, offset, count);
                    this.count += count;
                    return;
                }
            }
        }

        /// <inheritdoc />
        public override void Close()
        {
            if (closed)
            {
                return;
            }

            lock (locker)
            {
                closed = true;
            }
        }

        /// <summary>
        /// Assert whether the stream closed.
        /// </summary>
        protected void AssertClosed()
        {
            if (closed)
            {
                throw new ObjectDisposedException(nameof(PipelineStream), $"Stream is {nameof(closed)} Cannot write");
            }
        }

        /// <summary>
        /// Assert whether the stream disabled.
        /// </summary>
        protected void AssertDisabled()
        {
            if (disabled)
            {
                throw new ObjectDisposedException(nameof(PipelineStream), $"Stream is {disabled}");
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!disposing || disabled)
            {
                return;
            }

            lock (locker)
            {
                if (disabled)
                {
                    return;
                }

                try
                {
                    disabled = true;
                    closed = true;
                    ringBuffer.Dispose();
                }
                finally
                {
                    base.Dispose(true);
                }
            }
        }
    }
}

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

namespace CatLib
{
    /// <summary>
    /// The pipeline stream.
    /// </summary>
    public class PipelineStream : WrapperStream
    {
        /// <summary>
        /// The count can be read.
        /// </summary>
        private volatile int count;

        /// <summary>
        /// The stream capacity.
        /// </summary>
        private readonly int capacity;

        /// <summary>
        /// The thread sleep millisecond.
        /// </summary>
        private readonly int sleep;

        /// <summary>
        /// The <see cref="RingBuffer"/> instance.
        /// </summary>
        private readonly RingBuffer ringBuffer;

        /// <summary>
        /// Trigger when reading is complete.
        /// </summary>
        public event Action<Stream> OnRead;

        /// <summary>
        /// Whether the stream is disabled.
        /// </summary>
        private volatile bool disabled;

        /// <summary>
        /// Whether the stream is closed.
        /// </summary>
        private volatile bool closed;

        /// <inheritdoc />
        public override bool CanRead => count > 0 && !disabled;

        /// <inheritdoc />
        public override bool CanWrite => count < capacity && !closed;

        /// <summary>
        /// The stream position.
        /// </summary>
        private long position;

        /// <inheritdoc />
        public override long Position
        {
            get => position;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// The stream length.
        /// </summary>
        private long length;

        /// <inheritdoc />
        public override long Length => length;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <summary>
        /// Whether is closed the stream.
        /// </summary>
        public bool Closed => closed;

        /// <summary>
        /// Initialize an new <see cref="PipelineStream"/> instnace.
        /// </summary>
        /// <param name="capacity">The stream capacity.</param>
        /// <param name="sleep">The thread sleep time.</param>
        public PipelineStream(int capacity = 4096, int sleep = 1)
        {
            this.capacity = capacity.ToPrime();
            this.sleep = Math.Max(0, sleep);
            ringBuffer = new RingBuffer(this.capacity, false);
        }

        /// <summary>
        /// Release <see cref="PipelineStream"/> instance.
        /// </summary>
        ~PipelineStream()
        {
            Dispose(!disabled);
        }

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

                lock (ringBuffer.SyncRoot)
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

                lock (ringBuffer.SyncRoot)
                {
                    AssertDisabled();
                    AssertClosed();

                    if ((capacity - this.count) < count)
                    {
                        continue;
                    }

                    Guard.Requires<AssertException>(ringBuffer.Write(buffer, offset, count) == count);
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

            lock (ringBuffer.SyncRoot)
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

            lock (ringBuffer.SyncRoot)
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

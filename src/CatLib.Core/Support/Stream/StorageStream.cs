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
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CatLib
{
    /// <summary>
    /// The memory storage stream.
    /// </summary>
    public class StorageStream : WrapperStream
    {
        /// <summary>
        /// The stream position.
        /// </summary>
        private long position;

        /// <summary>
        /// Whether the stream is disabled.
        /// </summary>
        private bool disabled;

        /// <summary>
        /// Whether the stream is writeable.
        /// </summary>
        private readonly bool writable;

        /// <summary>
        /// The <see cref="IStorage"/> instance.
        /// </summary>
        private readonly IStorage storage;

        /// <inheritdoc />
        public override long Position
        {
            get
            {
                AssertDisabled();
                return position;
            }
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <inheritdoc />
        public override long Length
        {
            get
            {
                AssertDisabled();
                return storage.Length;
            }
        }

        /// <inheritdoc />
        public override bool CanWrite => !Disposed && writable;

        /// <inheritdoc />
        public override bool CanSeek => !Disposed;

        /// <inheritdoc />
        public override bool CanRead => !Disposed;

        /// <summary>
        /// Whether the stream is disabled.
        /// </summary>
        protected bool Disposed => disabled || storage.Disabled;

        /// <summary>
        /// Initialize an new <see cref="StorageStream"/> stream instance.
        /// </summary>
        /// <param name="storage">The <see cref="IStorage"/> instance.</param>
        /// <param name="writable">Whether the storage is writeable.</param>
        /// <param name="timeout">The locker time for stream read or write.</param>
        public StorageStream(IStorage storage, bool writable = true, int timeout = 1000)
        {
            Guard.Requires<ArgumentNullException>(storage != null);

            if (storage.Disabled)
            {
                throw new ObjectDisposedException(nameof(storage), $"Storage is {storage.Disabled}");
            }

            this.storage = storage;
            this.writable = writable;
            position = 0;
            disabled = false;

            if (storage.Locker == null)
            {
                return;
            }

            if (writable)
            {
                if (!storage.Locker.TryEnterWriteLock(timeout))
                {
                    throw GetOccupyException();
                }
            }
            else
            {
                if (!storage.Locker.TryEnterReadLock(timeout))
                {
                    throw GetOccupyException();
                }
            }
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            AssertDisabled();

            long tempPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        tempPosition = offset;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        tempPosition = unchecked(position + offset);

                        break;
                    }
                case SeekOrigin.End:
                    {
                        tempPosition = unchecked(Length + offset);
                        break;
                    }
                default:
                    throw new ArgumentException($"Unknow {nameof(SeekOrigin)}:{origin}");
            }

            if (tempPosition < 0)
            {
                throw new IOException($"Seek {position} less than 0");
            }

            if (tempPosition > Length)
            {
                throw new IOException($"Seek {position} is large then length : {Length}");
            }

            position = tempPosition;
            return position;
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            AssertWritable();
            AssertDisabled();
            storage.Write(buffer, offset, count, position);
            position += count;
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            AssertDisabled();
            var read = storage.Read(buffer, offset, count, position);
            position += read;
            return read;
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override void Flush()
        {
            // ignore
        }

        /// <summary>
        /// Gets the thread occupy exception.
        /// </summary>
        /// <returns>异常</returns>
        [ExcludeFromCodeCoverage]
        protected IOException GetOccupyException()
        {
            return new IOException("The resource is already occupied by other threads");
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing || disabled || storage == null)
                {
                    return;
                }

                disabled = true;

                if (storage.Disabled)
                {
                    return;
                }

                if (writable)
                {
                    storage.Locker.ExitWriteLock();
                }
                else
                {
                    storage.Locker.ExitReadLock();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Assert the stream is disabled.
        /// </summary>
        private void AssertDisabled()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(null, $"[{GetType()}] Stream is closed.");
            }
        }

        /// <summary>
        /// Assert the stream is writeable.
        /// </summary>
        private void AssertWritable()
        {
            if (!writable)
            {
                throw new NotSupportedException("Not supported writable");
            }
        }
    }
}
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
    /// A <see cref="SegmentStream"/> can be used to wrap a stream of a specified slice。
    /// Make the stream of the specified shard access like the traditional
    /// stream from the beginning to the end。.
    /// </summary>
    public class SegmentStream : WrapperStream
    {
        /// <summary>
        /// The initial position of the base stream.
        /// </summary>
        private readonly long initialPosition;

        /// <summary>
        /// The slice size.
        /// </summary>
        private readonly long partSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentStream"/> class.
        /// </summary>
        /// <param name="stream">The base stream.</param>
        /// <param name="partSize">The slice size.</param>
        public SegmentStream(Stream stream, long partSize = 0)
            : base(stream)
        {
            if (!stream.CanSeek)
            {
                throw new InvalidOperationException($"Base stream of {nameof(SegmentStream)} must be seekable");
            }

            initialPosition = stream.Position;
            var remainingSize = stream.Length - stream.Position;

            if (partSize == 0 || remainingSize < partSize)
            {
                this.partSize = remainingSize;
            }
            else
            {
                this.partSize = partSize;
            }
        }

        /// <inheritdoc />
        public override long Length
        {
            get
            {
                var length = base.Length - initialPosition;
                if (length > partSize)
                {
                    length = partSize;
                }

                return length;
            }
        }

        /// <inheritdoc />
        public override long Position
        {
            get => base.Position - initialPosition;
            set => base.Position = value;
        }

        /// <summary>
        /// Gets the remaining size.
        /// </summary>
        private long RemainingSize => partSize - Position;

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            long position;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = initialPosition + offset;
                    break;
                case SeekOrigin.Current:
                    position = base.Position + offset;
                    break;
                case SeekOrigin.End:
                    position = base.Position + partSize + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            if (position < initialPosition)
            {
                position = initialPosition;
            }
            else if (position > initialPosition + partSize)
            {
                position = initialPosition + partSize;
            }

            base.Seek(position, SeekOrigin.Begin);
            return Position;
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesToRead = count < RemainingSize ? count : (int)RemainingSize;
            return bytesToRead < 0 ? 0 : base.Read(buffer, offset, bytesToRead);
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}

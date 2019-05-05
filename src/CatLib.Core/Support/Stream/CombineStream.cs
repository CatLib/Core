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
    /// <see cref="CombineStream"/> allow multiple different streams to be combined into one stream.
    /// </summary>
    public class CombineStream : WrapperStream
    {
        /// <summary>
        /// Indicates the position of the current cursor.
        /// </summary>
        private long globalPosition;

        /// <summary>
        /// Indicates the index of the current stream.
        /// </summary>
        private int index;

        /// <summary>
        /// An array of multiple streams.
        /// </summary>
        private readonly Stream[] streams;

        /// <summary>
        /// The length of the <see cref="CombineStream"/>.
        /// </summary>
        private long length;

        /// <summary>
        /// The length of the <see cref="CombineStream"/>.
        /// </summary>
        public override long Length
        {
            get
            {
                if (length >= 0)
                {
                    return length;
                }

                length = 0;
                foreach (var stream in streams)
                {
                    length += stream.Length;
                }
                return length;
            }
        }

        /// <inheritdoc />
        public override bool CanSeek
        {
            get
            {
                foreach (var stream in streams)
                {
                    if (!stream.CanSeek)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <inheritdoc />
        public override long Position
        {
            get => globalPosition;
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <inheritdoc />
        public override bool CanRead
        {
            get
            {
                foreach (var stream in streams)
                {
                    if (!stream.CanRead)
                    {
                        return false;
                    }
                }
                return true;
            }   
        }

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <summary>
        /// Whether to close the sub stream when closing the combined stream
        /// </summary>
        private readonly bool autoClosed;

        /// <summary>
        /// Initialize an new <see cref="CombineStream"/> instance.
        /// </summary>
        /// <param name="left">The left stream.</param>
        /// <param name="right">The right stream.</param>
        /// <param name="closed">Whether to close the sub stream when closing the combined stream.</param>
        public CombineStream(Stream left, Stream right, bool closed = false)
            : this(new[] { left, right }, closed)
        {
        }

        /// <summary>
        /// Initialize an new <see cref="CombineStream"/> instance.
        /// </summary>
        /// <param name="source">An array of the sub stream.</param>
        /// <param name="closed">Whether to close the sub stream when closing the combined stream.</param>
        public CombineStream(Stream[] source, bool closed = false)
        {
            index = 0;
            streams = source;
            length = -1;
            autoClosed = closed;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
            {
                throw new NotSupportedException($"{nameof(CombineStream)} not supported {nameof(Seek)}.");
            }

            long newGloablPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newGloablPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newGloablPosition = globalPosition + offset;
                    break;
                case SeekOrigin.End:
                    newGloablPosition = Length + offset;
                    break;
                default:
                    throw new NotSupportedException($"Not support {nameof(SeekOrigin)}: {origin}");
            }

            if (newGloablPosition < 0 || newGloablPosition > Length)
            {
                throw new ArgumentOutOfRangeException($"{nameof(offset)} must large than zero or small then {nameof(Length)}");
            }

            long localPosition = 0;
            var newIndex = index = CalculatedIndex(newGloablPosition, ref localPosition);

            streams[newIndex].Seek(localPosition, SeekOrigin.Begin);
            while (++newIndex < streams.Length)
            {
                streams[newIndex].Seek(0, SeekOrigin.Begin);
            }

            return globalPosition = newGloablPosition;
        }

        /// <summary>
        /// Calculate sub stream index and relative positions
        /// </summary>
        /// <param name="globalPosition">The position of the current cursor.</param>
        /// <param name="localPosition">The relative position.</param>
        protected int CalculatedIndex(long globalPosition, ref long localPosition)
        {
            long length = 0;
            for (var i = 0; i < streams.Length; i++)
            {
                length += streams[i].Length;
                if (globalPosition > length)
                {
                    continue;
                }

                localPosition = streams[i].Length - (length - globalPosition);
                return i;
            }

            throw new AssertException($"Failed to determine {nameof(localPosition)}");
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            Guard.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(count >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(buffer.Length - offset >= count);

            var result = 0;
            do
            {
                var read = streams[index].Read(buffer, offset, count);
                if (read <= 0 && index < streams.Length - 1)
                {
                    index++;
                    continue;
                }

                if (read <= 0)
                {
                    break;
                }

                count -= read;
                offset += read;
                globalPosition += read;
                result += read;
            } while (count > 0);

            return result;
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException($"{nameof(CombineStream)} not supported {nameof(Write)}.");
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            throw new NotSupportedException($"{nameof(CombineStream)} not supported {nameof(SetLength)}.");
        }

        /// <inheritdoc />
        public override void Flush()
        {
            throw new NotSupportedException($"{nameof(CombineStream)} not supported {nameof(Flush)}.");
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!autoClosed)
                {
                    return;
                }

                foreach (var stream in streams)
                {
                    stream?.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}

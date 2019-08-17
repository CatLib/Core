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

using CatLib.Exception;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CatLib.Support
{
    /// <summary>
    /// Array helper.
    /// </summary>
    public static class Arr
    {
        /// <summary>
        /// Combine multiple specified arrays into one array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <returns>Returns an merged array.</returns>
#pragma warning disable S2368
        public static T[] Merge<T>(params T[][] sources)
#pragma warning restore S2368
        {
            if (sources == null || sources.Length <= 0)
            {
                return Array.Empty<T>();
            }

            var totalSize = 0;
            foreach (var source in sources)
            {
                if (source == null || source.Length <= 0)
                {
                    continue;
                }

                totalSize += source.Length;
            }

            if (totalSize <= 0)
            {
                return Array.Empty<T>();
            }

            var merged = new T[totalSize];
            var length = 0;
            foreach (var source in sources)
            {
                if (source == null || source.Length <= 0)
                {
                    continue;
                }

                Array.Copy(source, 0, merged, length, source.Length);
                length += source.Length;
            }

            return merged;
        }

        /// <summary>
        /// Get a specified number of random values from a specified array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="number">The specified number.</param>
        /// <returns>An array of the random value.</returns>
        public static T[] Rand<T>(T[] sources, int number = 1)
        {
            if (sources == null || sources.Length <= 0)
            {
                return new T[number];
            }

            return Slice(Shuffle(sources), 0, Math.Max(number, 1));
        }

        /// <summary>
        /// Shuffle the elements in the specified array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="seed">The random seed.</param>
        /// <returns>Return the disrupted array.</returns>
        public static T[] Shuffle<T>(T[] sources, int? seed = null)
        {
            if (sources == null || sources.Length <= 0)
            {
                return Array.Empty<T>();
            }

            var requested = new T[sources.Length];
            Array.Copy(sources, requested, sources.Length);

            var random = Helper.MakeRandom(seed);
            for (var i = 0; i < requested.Length; i++)
            {
                var index = random.Next(0, requested.Length - 1);
                if (index == i)
                {
                    continue;
                }

                var temporary = requested[i];
                requested[i] = requested[index];
                requested[index] = temporary;
            }

            return requested;
        }

        /// <summary>
        /// Removes an element of the specified length from the array. If
        /// the <paramref name="replaceSource"/> parameter is given, the new
        /// element is inserted from the <paramref name="start"/> position.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="start">
        /// Delete the start position of the element.
        /// <para>If the value is set to a positive number, delete it from the beginning of the trip.</para>
        /// <para>If the value is set to a negative number, the <paramref name="start"/> absolute value is taken from the back.</para>
        /// </param>
        /// <param name="length">
        /// Number of deleted elements.
        /// <para>If the value is set to a positive number, then the number of elements is returned。.</para>
        /// <para>If the value is set to a negative number, then remove the <paramref name="length"/> absolute position from the back to the front to delete.</para>
        /// <para>If the value is not set, then all elements from the position set by the <paramref name="start"/> parameter to the end of the array are returned.</para>
        /// </param>
        /// <param name="replaceSource">An array inserted at the start position.</param>
        /// <returns>An removed array.</returns>
        public static T[] Splice<T>(ref T[] sources, int start, int? length = null, T[] replaceSource = null)
        {
            if (sources == null || sources.Length <= 0)
            {
                return Array.Empty<T>();
            }

            Helper.NormalizationPosition(sources.Length, ref start, ref length);

            var candidates = new T[length.Value];
            if (length.Value == sources.Length)
            {
                Array.Copy(sources, candidates, sources.Length);
                sources = replaceSource ?? Array.Empty<T>();
                return candidates;
            }

            Array.Copy(sources, start, candidates, 0, length.Value);

            if (replaceSource == null || replaceSource.Length == 0)
            {
                var newSource = new T[sources.Length - length.Value];
                if (start > 0)
                {
                    Array.Copy(sources, 0, newSource, 0, start);
                }

                Array.Copy(sources, start + length.Value, newSource, start, sources.Length - (start + length.Value));
                sources = newSource;
            }
            else
            {
                var newSource = new T[sources.Length - length.Value + replaceSource.Length];
                if (start > 0)
                {
                    Array.Copy(sources, 0, newSource, 0, start);
                }

                Array.Copy(replaceSource, 0, newSource, start, replaceSource.Length);
                Array.Copy(sources, start + length.Value, newSource, start + replaceSource.Length,
                    sources.Length - (start + length.Value));
                sources = newSource;
            }

            return candidates;
        }

        /// <summary>
        /// Crop the array to the desired position.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="position">Crop range, negative numbers are trimmed from back to front.</param>
        public static void Cut<T>(ref T[] source, int position)
        {
            if (source == null || source.Length <= 0 || position == 0)
            {
                return;
            }

            if (Math.Abs(position) >= source.Length)
            {
                if (source.Length > 0)
                {
                    Array.Resize(ref source, 0);
                }

                return;
            }

            if (position > 0)
            {
                var size = source.Length - position;
                Array.Copy(source, position, source, 0, size);
                Array.Resize(ref source, size);
            }
            else
            {
                Array.Resize(ref source, source.Length - Math.Abs(position));
            }
        }

        /// <summary>
        /// Divide an array into new array blocks.
        /// <para>The number of cells in each array is determined by
        /// the <paramref name="size"/> parameter. The number of cells in the last array may be a few.</para>
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="size">The size of the block.</param>
        /// <returns>Return an array of the block.</returns>
        public static T[][] Chunk<T>(T[] sources, int size)
        {
            if (sources == null || sources.Length <= 0)
            {
                return Array.Empty<T[]>();
            }

            size = Math.Max(1, size);
            var requested = new T[(sources.Length / size) + (sources.Length % size == 0 ? 0 : 1)][];

            T[] chunks = null;
            for (var i = 0; i < sources.Length; i++)
            {
                var position = i / size;
                if (i % size == 0)
                {
                    if (chunks != null)
                    {
                        requested[position - 1] = chunks;
                    }

                    chunks = new T[(i + size) <= sources.Length ? size : sources.Length - i];
                }

                if (chunks == null)
                {
                    throw new AssertException("Unexpected exception");
                }

                chunks[i - (position * size)] = sources[i];
            }

            requested[requested.Length - 1] = chunks;
            return requested;
        }

        /// <summary>
        /// Fill the array, if the specified array is passed in, it will be filled based on the specified array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="start">The starting index.</param>
        /// <param name="length">The filling length.</param>
        /// <param name="value">The filling value.</param>
        /// <param name="sources">The specified array.</param>
        /// <returns>Returns an filled array.</returns>
        public static T[] Fill<T>(int start, int length, T value, T[] sources = null)
        {
            Guard.Requires<ArgumentOutOfRangeException>(start >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(length > 0);

            var count = start + length;
            var requested = new T[Math.Max(sources?.Length + length ?? count, count)];

            if (start > 0 && sources != null)
            {
                Array.Copy(sources, requested, Math.Min(sources.Length, start));
            }

            for (var i = start; i < count; i++)
            {
                requested[i] = value;
            }

            if (sources != null && start < sources.Length)
            {
                Array.Copy(sources, start, requested, count, sources.Length - start);
            }

            return requested;
        }

        /// <summary>
        /// Pass each value of the array to the callback function, if the callback
        /// function returns true, remove the corresponding element in the array
        /// and return the removed element.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="predicate">The callback.</param>
        /// <param name="expected">The predicate expected to removed.</param>
        /// <returns>Returns an removed array.</returns>
        public static T[] Remove<T>(ref T[] sources, Predicate<T> predicate, bool expected = true)
        {
            Guard.Requires<ArgumentNullException>(predicate != null, $"Must set a {predicate}.");

            if (sources == null || sources.Length <= 0)
            {
                return Array.Empty<T>();
            }

            var candidateIndex = 0;
            var candidates = new T[sources.Length];
            for (var i = sources.Length - 1; i >= 0; i--)
            {
                if (predicate.Invoke(sources[i]) != expected)
                {
                    continue;
                }

                candidates[candidateIndex++] = sources[i];
                RemoveAt(ref sources, i);
            }

            Array.Reverse(candidates, 0, candidateIndex);
            Array.Resize(ref candidates, candidateIndex);
            return candidates;
        }

        /// <summary>
        /// Each value in the source array is passed to the callback function.
        /// If the callback function is equal to the <paramref name="expected"/>
        /// value, the current value in the input array is added to the result array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="predicate">The callback.</param>
        /// <param name="expected">The expected value.</param>
        /// <returns>Returns an filtered array.</returns>
        public static T[] Filter<T>(IEnumerable<T> sources, Predicate<T> predicate, bool expected = true)
        {
            Guard.Requires<ArgumentNullException>(predicate != null, $"Must set a {predicate}.");

            if (sources == null)
            {
                return Array.Empty<T>();
            }

            var candidates = new LinkedList<T>();
            foreach (var result in sources)
            {
                if (predicate.Invoke(result) == expected)
                {
                    candidates.AddLast(result);
                }
            }

            return candidates.ToArray();
        }

        /// <summary>
        /// Pass the value of the iterator into the callback function, and the
        /// value returned by the custom function as the new array value.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <typeparam name="TReturn">The type of return value.</typeparam>
        /// <param name="source">The source iterator.</param>
        /// <param name="closure">The closure to process.</param>
        /// <returns>Returns an new array.</returns>
        public static TReturn[] Map<T, TReturn>(IEnumerable<T> source, Func<T, TReturn> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null, $"Must set a {closure}.");

            if (source == null)
            {
                return Array.Empty<TReturn>();
            }

            var requested = new List<TReturn>();
            foreach (var value in source)
            {
                requested.Add(closure.Invoke(value));
            }

            return requested.ToArray();
        }

        /// <summary>
        /// Delete the last element in the array and return the deleted element
        /// as the return value.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <returns>Returns removed element.</returns>
        public static T Pop<T>(ref T[] sources)
        {
            Guard.Requires<ArgumentNullException>(sources != null, $"{nameof(sources)} should not be null.");
            Guard.Requires<InvalidOperationException>(sources.Length > 0, $"The number of elements needs to be greater than 0.");

            var candidate = sources[sources.Length - 1];
            Array.Resize(ref sources, sources.Length - 1);
            return candidate;
        }

        /// <summary>
        /// Add one or more elements to the end of the array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="elements">The added elements.</param>
        /// <returns>Returns the length of the new array.</returns>
        public static int Push<T>(ref T[] sources, params T[] elements)
        {
            sources = sources ?? Array.Empty<T>();
            if (elements == null || elements.Length <= 0)
            {
                return sources.Length;
            }

            Array.Resize(ref sources, sources.Length + elements.Length);
            Array.Copy(elements, 0, sources, sources.Length - elements.Length, elements.Length);
            return sources.Length;
        }

        /// <summary>
        /// Pass a value from an array to a callback function and return a string.
        /// <para>The function returns null if the array is empty and the <paramref name="initial"/> parameter is not passed.</para>
        /// <para>If the <paramref name="initial"/> parameter is specified, the parameter will be treated as the first value
        /// in the array, and if the array is empty, it will be the final return value (string).</para>
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="closure">The closure process.</param>
        /// <param name="initial">The initial value.</param>
        /// <returns>Returnd the processed string.</returns>
        public static string Reduce<T>(IEnumerable<T> sources, Func<object, T, string> closure, object initial = null)
        {
            Guard.Requires<ArgumentNullException>(closure != null, $"Must set a {closure}.");

            if (sources == null)
            {
                return initial?.ToString();
            }

            var requested = initial;
            foreach (var segments in sources)
            {
                requested = closure.Invoke(requested, segments);
            }

            return requested?.ToString();
        }

        /// <summary>
        /// Take a value from the array according to the condition and return.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="start">
        /// Remove the starting position of the element.
        /// <para>If the value is set to a positive number, it will be taken from the beginning of the trip.</para>
        /// <para>If the value is set to a negative number, the <paramref name="start"/> absolute value is taken from the back.</para>
        /// </param>
        /// <param name="length">
        /// Returns the length of the array.
        /// <para>If the value is set to a positive number, then the number of elements is returned。.</para>
        /// <para>If the value is set to a negative number, then remove the <paramref name="length"/> absolute position from the back to the front to delete.</para>
        /// <para>If the value is not set, then all elements from the position set by the <paramref name="start"/> parameter to the end of the array are returned.</para>
        /// </param>
        /// <returns>Returns an new array.</returns>
        public static T[] Slice<T>(T[] sources, int start, int? length = null)
        {
            if (sources == null || sources.Length <= 0)
            {
                return Array.Empty<T>();
            }

            Helper.NormalizationPosition(sources.Length, ref start, ref length);

            var requested = new T[length.Value];
            Array.Copy(sources, start, requested, 0, length.Value);
            return requested;
        }

        /// <summary>
        /// Removed the first element in the array and return the value of the removed element.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <returns>Returns the removed value.</returns>
        public static T Shift<T>(ref T[] sources)
        {
            Guard.Requires<ArgumentNullException>(sources != null, $"{nameof(sources)} should not be null.");
            Guard.Requires<InvalidOperationException>(sources.Length > 0, $"The number of elements needs to be greater than 0.");

            var candidate = sources[0];
            var newSource = new T[sources.Length - 1];

            Array.Copy(sources, 1, newSource, 0, sources.Length - 1);
            sources = newSource;
            return candidate;
        }

        /// <summary>
        /// Add a new element at the beginning of the array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="elements">The added element.</param>
        /// <returns>Returns the length of the new array.</returns>
        public static int Unshift<T>(ref T[] sources, params T[] elements)
        {
            sources = sources ?? Array.Empty<T>();
            if (elements == null || elements.Length <= 0)
            {
                return sources.Length;
            }

            var newSources = new T[sources.Length + elements.Length];

            Array.Copy(elements, newSources, elements.Length);
            Array.Copy(sources, 0, newSources, elements.Length, sources.Length);

            sources = newSources;

            return sources.Length;
        }

        /// <summary>
        /// Return arrays in reverse order.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="start">
        /// The starting position of the starting element.
        /// <para>If the value is set to a positive number, it will be taken from the beginning of the trip.</para>
        /// <para>If the value is set to a negative number, the <paramref name="start"/> absolute value is taken from the back.</para></param>
        /// <param name="length">
        /// Returns the length of the array.
        /// <para>If the value is set to a positive number, then the number of elements is returned。.</para>
        /// <para>If the value is set to a negative number, then remove the <paramref name="length"/> absolute position from the back to the front to delete.</para>
        /// <para>If the value is not set, then all elements from the position set by the <paramref name="start"/> parameter to the end of the array are returned.</para>
        /// </param>
        /// <returns>Returns inverted array.</returns>
        public static T[] Reverse<T>(T[] sources, int start = 0, int? length = null)
        {
            if (sources == null || sources.Length <= 0)
            {
                return Array.Empty<T>();
            }

            if (sources.Length == 1)
            {
                return sources;
            }

            Helper.NormalizationPosition(sources.Length, ref start, ref length);
            var temporarySource = new T[sources.Length];
            Array.Copy(sources, temporarySource, sources.Length);
            Array.Reverse(temporarySource, start, length.Value);

            var resquested = new T[length.Value];
            Array.Copy(temporarySource, start, resquested, 0, length.Value);
            return resquested;
        }

        /// <summary>
        /// Retrieve the initial element index that matches all matching values from the array.
        /// If it returns -1, it means that it does not appear.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="match">The value to match, if there are more than one, only all matches will match.</param>
        /// <returns>Returning -1 means that the specified value was not retrieved.</returns>
        public static int IndexOf<T>(T[] source, params T[] match)
        {
            if (match == null || match.Length <= 0
                || source == null || source.Length <= 0)
            {
                return -1;
            }

            for (var sourceIndex = 0; sourceIndex < source.Length; sourceIndex++)
            {
                if (!source[sourceIndex].Equals(match[0]))
                {
                    continue;
                }

                var isFinded = true;

                for (var matchIndex = 0; matchIndex < match.Length; matchIndex++)
                {
                    if ((sourceIndex + matchIndex) < source.Length &&
                          source[sourceIndex + matchIndex].Equals(match[matchIndex]))
                    {
                        continue;
                    }

                    isFinded = false;
                    break;
                }

                if (isFinded)
                {
                    return sourceIndex;
                }
            }

            return -1;
        }

        /// <summary>
        /// Retrieve the subscript of the specified arbitrary matching value from the array.
        /// If it returns -1, it means that it does not appear.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="match">The value to match.Match as long as there is an element match.</param>
        /// <returns>Returning -1 means that the specified value was not retrieved.</returns>
        public static int IndexOfAny<T>(T[] source, params T[] match)
        {
            if (match == null || match.Length <= 0
                || source == null || source.Length <= 0)
            {
                return -1;
            }

            for (var sourceIndex = 0; sourceIndex < source.Length; sourceIndex++)
            {
                for (var matchIndex = 0; matchIndex < match.Length; matchIndex++)
                {
                    if (source[sourceIndex].Equals(match[matchIndex]))
                    {
                        return sourceIndex;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Exclude the specified value in the array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The source array.</param>
        /// <param name="matches">An array of exclude value.</param>
        /// <returns>Returns an array of processed.</returns>
        public static T[] Difference<T>(T[] sources, params T[] matches)
        {
            if (sources == null || sources.Length <= 0 || matches == null || matches.Length <= 0)
            {
                return sources;
            }

            return Filter(sources, (source) =>
            {
                foreach (var match in matches)
                {
                    if (source.Equals(match))
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        /// <summary>
        /// Remove and return the array element of the specified index.
        /// <para>If the index is passed a negative number then it will be removed from the end.</para>
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="index">The index of array.</param>
        /// <param name="defaultValue">Default value if index not found.</param>
        /// <returns>Returns removed element.</returns>
        public static T RemoveAt<T>(ref T[] sources, int index, T defaultValue = default)
        {
            if (sources == null || sources.Length <= 0 || index >= sources.Length)
            {
                return defaultValue;
            }

            var candidates = Splice(ref sources, index, 1);
            return candidates.Length > 0 ? candidates[0] : defaultValue;
        }

        /// <summary>
        /// Pass the specified array to the callback test.
        /// <para>The function returns false only if all elements pass the checker are false.</para>
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="predicate">The callback.</param>
        /// <returns>True if pass the test.</returns>
        public static bool Test<T>(IEnumerable<T> sources, Predicate<T> predicate)
        {
            Guard.Requires<ArgumentNullException>(predicate != null, $"Must set a {predicate}.");

            if (sources == null)
            {
                return false;
            }

            foreach (var source in sources)
            {
                if (predicate(source))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds the specified element in the specified array, and replaces it with a substitute value if found,
        /// otherwise adds a replacement value at the end of the specified array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="sources">The specified array.</param>
        /// <param name="predicate">The callback to find element.</param>
        /// <param name="value">The replacement value.</param>
        public static void Set<T>(ref T[] sources, Predicate<T> predicate, T value)
        {
            Guard.Requires<ArgumentNullException>(predicate != null, $"Must set a {predicate}.");

            sources = sources ?? Array.Empty<T>();

            for (var index = 0; index < sources.Length; index++)
            {
                if (!predicate(sources[index]))
                {
                    continue;
                }

                sources[index] = value;
                return;
            }

            Push(ref sources, value);
        }
    }
}

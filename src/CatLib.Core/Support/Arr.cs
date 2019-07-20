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
            Guard.Requires<ArgumentNullException>(sources != null);
            var length = 0;
            foreach (var source in sources)
            {
                if (source == null || source.Length <= 0)
                {
                    continue;
                }

                length += source.Length;
            }

            if (length <= 0)
            {
                return Array.Empty<T>();
            }

            var merge = new T[length];
            var current = 0;
            foreach (var source in sources)
            {
                if (source == null || source.Length <= 0)
                {
                    continue;
                }

                Array.Copy(source, 0, merge, current, source.Length);
                current += source.Length;
            }

            return merge;
        }

        /// <summary>
        /// Get a specified number of random values from a specified array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="number">The specified number.</param>
        /// <returns>An array of the random value.</returns>
        public static T[] Rand<T>(T[] source, int number = 1)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            number = Math.Max(number, 1);
            source = Shuffle(source);
            var requested = new T[number];
            var i = 0;
            foreach (var result in source)
            {
                if (i >= number)
                {
                    break;
                }

                requested[i++] = result;
            }

            return requested;
        }

        /// <summary>
        /// Disrupt the elements in the specified array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="seed">The random seed.</param>
        /// <returns>Return the disrupted array.</returns>
        public static T[] Shuffle<T>(T[] source, int? seed = null)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            var requested = new T[source.Length];
            Array.Copy(source, requested, source.Length);

            var random = Helper.MakeRandom(seed);
            for (var i = 0; i < requested.Length; i++)
            {
                var index = random.Next(0, requested.Length - 1);
                if (index == i)
                {
                    continue;
                }

                var temp = requested[i];
                requested[i] = requested[index];
                requested[index] = temp;
            }

            return requested;
        }

        /// <summary>
        /// Removes an element of the specified length from the array. If
        /// the <paramref name="replSource"/> parameter is given, the new
        /// element is inserted from the <paramref name="start"/> position.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
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
        /// <param name="replSource">An array inserted at the start position.</param>
        /// <returns>An removed array.</returns>
        public static T[] Splice<T>(ref T[] source, int start, int? length = null, T[] replSource = null)
        {
            Guard.Requires<ArgumentNullException>(source != null);

            Helper.NormalizationPosition(source.Length, ref start, ref length);

            var requested = new T[length.Value];

            if (length.Value == source.Length)
            {
                Array.Copy(source, requested, source.Length);
                source = replSource ?? Array.Empty<T>();
                return requested;
            }

            Array.Copy(source, start, requested, 0, length.Value);

            if (replSource == null || replSource.Length == 0)
            {
                var newSource = new T[source.Length - length.Value];
                if (start > 0)
                {
                    Array.Copy(source, 0, newSource, 0, start);
                }

                Array.Copy(source, start + length.Value, newSource, start, source.Length - (start + length.Value));
                source = newSource;
            }
            else
            {
                var newSource = new T[source.Length - length.Value + replSource.Length];
                if (start > 0)
                {
                    Array.Copy(source, 0, newSource, 0, start);
                }

                Array.Copy(replSource, 0, newSource, start, replSource.Length);
                Array.Copy(source, start + length.Value, newSource, start + replSource.Length,
                    source.Length - (start + length.Value));
                source = newSource;
            }

            return requested;
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
        /// <param name="source">The specified array.</param>
        /// <param name="size">The size of the block.</param>
        /// <returns>Return an array of the block.</returns>
        public static T[][] Chunk<T>(T[] source, int size)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            size = Math.Max(1, size);
            var requested = new T[(source.Length / size) + (source.Length % size == 0 ? 0 : 1)][];

            T[] chunk = null;
            for (var i = 0; i < source.Length; i++)
            {
                var pos = i / size;
                if (i % size == 0)
                {
                    if (chunk != null)
                    {
                        requested[pos - 1] = chunk;
                    }

                    chunk = new T[(i + size) <= source.Length ? size : source.Length - i];
                }

                if (chunk == null)
                {
                    throw new AssertException("Unexpected exception");
                }

                chunk[i - (pos * size)] = source[i];
            }

            requested[requested.Length - 1] = chunk;

            return requested;
        }

        /// <summary>
        /// Fill the array, if the specified array is passed in, it will be filled based on the specified array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="start">The starting index.</param>
        /// <param name="length">The filling length.</param>
        /// <param name="value">The filling value.</param>
        /// <param name="source">The specified array.</param>
        /// <returns>Returns an filled array.</returns>
        public static T[] Fill<T>(int start, int length, T value, T[] source = null)
        {
            Guard.Requires<ArgumentOutOfRangeException>(start >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(length > 0);

            var count = start + length;
            var requested = new T[Math.Max(source?.Length + length ?? count, count)];

            if (start > 0 && source != null)
            {
                Array.Copy(source, requested, Math.Min(source.Length, start));
            }

            for (var i = start; i < count; i++)
            {
                requested[i] = value;
            }

            if (source != null && start < source.Length)
            {
                Array.Copy(source, start, requested, count, source.Length - start);
            }

            return requested;
        }

        /// <summary>
        /// Pass each value of the array to the callback function, if the callback
        /// function returns true, remove the corresponding element in the array
        /// and return the removed element.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="predicate">The callback.</param>
        /// <returns>Returns an removed array.</returns>
        public static T[] Remove<T>(ref T[] source, Predicate<T> predicate)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<ArgumentNullException>(predicate != null);

            if (source.Length <= 0)
            {
                return Array.Empty<T>();
            }

            var results = new T[source.Length];
            var n = 0;
            for (var i = source.Length - 1; i >= 0; i--)
            {
                if (!predicate.Invoke(source[i]))
                {
                    continue;
                }

                results[n++] = source[i];
                RemoveAt(ref source, i);
            }

            Array.Reverse(results, 0, n);
            Array.Resize(ref results, n);
            return results;
        }

        /// <summary>
        /// Each value in the source array is passed to the callback function.
        /// If the callback function is equal to the <paramref name="expected"/>
        /// value, the current value in the input array is added to the result array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="predicate">The callback.</param>
        /// <param name="expected">The expected value.</param>
        /// <returns>Returns an filtered array.</returns>
        public static T[] Filter<T>(T[] source, Predicate<T> predicate, bool expected = true)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<ArgumentNullException>(predicate != null);
            var elements = new T[source.Length];

            var i = 0;
            foreach (var result in source)
            {
                if (predicate.Invoke(result) == expected)
                {
                    elements[i++] = result;
                }
            }

            Array.Resize(ref elements, i);
            return elements;
        }

        /// <summary>
        /// Each value in the source array is passed to the callback function.
        /// If the callback function is equal to the <paramref name="expected"/>
        /// value, the current value in the input array is added to the result array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="predicate">The callback.</param>
        /// <param name="expected">The expected value.</param>
        /// <returns>Returns an filtered array.</returns>
        public static T[] Filter<T>(IEnumerable<T> source, Predicate<T> predicate, bool expected = true)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<ArgumentNullException>(predicate != null);

            var results = new List<T>();
            foreach (var result in source)
            {
                if (predicate.Invoke(result) == expected)
                {
                    results.Add(result);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// Pass the array value into the callback function, the value returned
        /// by the custom function as the new array value.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <typeparam name="TReturn">The type of return value.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Returns an new array.</returns>
        public static TReturn[] Map<T, TReturn>(T[] source, Func<T, TReturn> callback)
        {
            Guard.Requires<ArgumentNullException>(callback != null);

            if (source == null)
            {
                return Array.Empty<TReturn>();
            }

            var requested = new TReturn[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                requested[i] = callback.Invoke(source[i]);
            }

            return requested;
        }

        /// <summary>
        /// Pass the value of the iterator into the callback function, and the
        /// value returned by the custom function as the new array value.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <typeparam name="TReturn">The type of return value.</typeparam>
        /// <param name="source">The source iterator.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Returns an new array.</returns>
        public static TReturn[] Map<T, TReturn>(IEnumerable<T> source, Func<T, TReturn> callback)
        {
            Guard.Requires<ArgumentNullException>(callback != null);

            if (source == null)
            {
                return Array.Empty<TReturn>();
            }

            var requested = new List<TReturn>();
            foreach (var value in source)
            {
                requested.Add(callback.Invoke(value));
            }

            return requested.ToArray();
        }

        /// <summary>
        /// Delete the last element in the array and return the deleted element
        /// as the return value.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <returns>Returns removed element.</returns>
        public static T Pop<T>(ref T[] source)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<InvalidOperationException>(source.Length > 0);

            T result = source[source.Length - 1];
            Array.Resize(ref source, source.Length - 1);
            return result;
        }

        /// <summary>
        /// Add one or more elements to the end of the array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="elements">The added elements.</param>
        /// <returns>Returns the length of the new array.</returns>
        public static int Push<T>(ref T[] source, params T[] elements)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<InvalidOperationException>(elements != null);

            Array.Resize(ref source, source.Length + elements.Length);
            Array.Copy(elements, 0, source, source.Length - elements.Length, elements.Length);

            return source.Length;
        }

        /// <summary>
        /// Pass a value from an array to a callback function and return a string.
        /// <para>The function returns null if the array is empty and the <paramref name="initial"/> parameter is not passed.</para>
        /// <para>If the <paramref name="initial"/> parameter is specified, the parameter will be treated as the first value
        /// in the array, and if the array is empty, it will be the final return value (string).</para>
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="initial">The initial value.</param>
        /// <returns>Returnd the processed string.</returns>
        public static string Reduce<T>(T[] source, Func<object, T, string> callback, object initial = null)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<ArgumentNullException>(callback != null);

            var requested = initial;
            foreach (var segments in source)
            {
                requested = callback.Invoke(requested, segments);
            }

            return requested?.ToString();
        }

        /// <summary>
        /// Take a value from the array according to the condition and return.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
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
        public static T[] Slice<T>(T[] source, int start, int? length = null)
        {
            Guard.Requires<ArgumentNullException>(source != null);

            Helper.NormalizationPosition(source.Length, ref start, ref length);

            var requested = new T[length.Value];
            Array.Copy(source, start, requested, 0, length.Value);

            return requested;
        }

        /// <summary>
        /// Removed the first element in the array and return the value of the removed element.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <returns>Returns the removed value.</returns>
        public static T Shift<T>(ref T[] source)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<InvalidOperationException>(source.Length > 0);

            var requested = source[0];
            var newSource = new T[source.Length - 1];

            Array.Copy(source, 1, newSource, 0, source.Length - 1);
            source = newSource;

            return requested;
        }

        /// <summary>
        /// Add a new element at the beginning of the array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="elements">The added element.</param>
        /// <returns>Returns the length of the new array.</returns>
        public static int Unshift<T>(ref T[] source, params T[] elements)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<ArgumentNullException>(elements != null);

            var newSource = new T[source.Length + elements.Length];

            Array.Copy(elements, newSource, elements.Length);
            Array.Copy(source, 0, newSource, elements.Length, source.Length);

            source = newSource;

            return source.Length;
        }

        /// <summary>
        /// Return arrays in reverse order.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
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
        public static T[] Reverse<T>(T[] source, int start = 0, int? length = null)
        {
            Guard.Requires<ArgumentNullException>(source != null);

            if (source.Length == 1)
            {
                return source;
            }

            Helper.NormalizationPosition(source.Length, ref start, ref length);
            var tmpSource = new T[source.Length];
            Array.Copy(source, tmpSource, source.Length);
            Array.Reverse(tmpSource, start, length.Value);

            var resquested = new T[length.Value];
            Array.Copy(tmpSource, start, resquested, 0, length.Value);
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

            for (var i = 0; i < source.Length; i++)
            {
                if (!source[i].Equals(match[0]))
                {
                    continue;
                }

                var isFinded = true;

                for (var n = 0; n < match.Length; n++)
                {
                    if ((i + n) < source.Length &&
                          source[i + n].Equals(match[n]))
                    {
                        continue;
                    }

                    isFinded = false;
                    break;
                }

                if (isFinded)
                {
                    return i;
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

            for (var i = 0; i < source.Length; i++)
            {
                for (var n = 0; n < match.Length; n++)
                {
                    if (source[i].Equals(match[n]))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Exclude the specified value in the array.
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The source array.</param>
        /// <param name="match">An array of exclude value.</param>
        /// <returns>Returns an array of processed.</returns>
        public static T[] Difference<T>(T[] source, params T[] match)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            if (match == null)
            {
                return source;
            }

            return Filter(source, (val) =>
            {
                foreach (var t in match)
                {
                    if (val.Equals(t))
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
        /// <param name="source">The specified array.</param>
        /// <param name="index">The index of array.</param>
        /// <returns>Returns removed element.</returns>
        public static T RemoveAt<T>(ref T[] source, int index)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<ArgumentException>(index < source.Length);

            var result = Splice(ref source, index, 1);
            return result.Length > 0 ? result[0] : default;
        }

        /// <summary>
        /// Pass the specified array to the callback test.
        /// <para>The function returns false only if all elements pass the checker are false.</para>
        /// </summary>
        /// <typeparam name="T">The type of array.</typeparam>
        /// <param name="source">The specified array.</param>
        /// <param name="predicate">The callback.</param>
        /// <returns>True if pass the test.</returns>
        public static bool Test<T>(T[] source, Predicate<T> predicate)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<ArgumentNullException>(predicate != null);

            foreach (var result in source)
            {
                if (predicate(result))
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
        /// <param name="source">The specified array.</param>
        /// <param name="predicate">The callback to find element.</param>
        /// <param name="value">The replacement value.</param>
        public static void Set<T>(ref T[] source, Predicate<T> predicate, T value)
        {
            Guard.Requires<ArgumentNullException>(predicate != null);

            source = source ?? Array.Empty<T>();

            for (var index = 0; index < source.Length; index++)
            {
                if (!predicate(source[index]))
                {
                    continue;
                }

                source[index] = value;
                return;
            }

            Push(ref source, value);
        }
    }
}

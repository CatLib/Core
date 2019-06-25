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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CatLib.Support
{
    /// <summary>
    /// Represents an ordered set, implemented by a jump list.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    /// <typeparam name="TScore">The score type.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
#pragma warning disable CA1710
    public sealed class SortSet<TElement, TScore> : IEnumerable<TElement>
        where TScore : IComparable<TScore>
#pragma warning restore CA1710
    {
        private readonly int maxLevel;
        private readonly SkipNode header;
        private readonly double probability;
        private readonly System.Random random = new System.Random();
        private readonly IComparer<TScore> comparer;
        private readonly Dictionary<TElement, TScore> elementMapping = new Dictionary<TElement, TScore>();
        private bool forward;
        private int level;
        private SkipNode tail;

        /// <summary>
        /// Initializes a new instance of the <see cref="SortSet{TElement, TScore}"/> class.
        /// </summary>
        /// <param name="probable">Probability coefficient of possible number of level(0-1).</param>
        /// <param name="maxLevel">The max level.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="probable"/>或<paramref name="maxLevel"/>不是有效值时引发.</exception>
        public SortSet(double probable = 0.25, int maxLevel = 32)
        {
            Guard.Requires<ArgumentOutOfRangeException>(maxLevel > 0);
            Guard.Requires<ArgumentOutOfRangeException>(probable < 1);
            Guard.Requires<ArgumentOutOfRangeException>(probable > 0);

            forward = true;
            probability = probable * 0xFFFF;
            this.maxLevel = maxLevel;
            level = 1;
            header = new SkipNode
            {
                Level = new SkipNode.SkipNodeLevel[maxLevel],
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortSet{TElement, TScore}"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <param name="probable">Probability coefficient of possible number of level(0-1).</param>
        /// <param name="maxLevel">The max level.</param>
        public SortSet(IComparer<TScore> comparer, double probable = 0.25, int maxLevel = 32)
            : this(probable, maxLevel)
        {
            Guard.Requires<ArgumentNullException>(comparer != null);
            this.comparer = comparer;
        }

        /// <summary>
        /// Gets the element count.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the sync lock.
        /// </summary>
        public object SyncRoot { get; } = new object();

        /// <summary>
        /// Get the element of the specified ranking.
        /// </summary>
        /// <param name="rank">The ranking(0 is the bottom).</param>
        /// <returns>The element.</returns>
        public TElement this[int rank] => GetElementByRank(rank);

        /// <summary>
        /// Clear the sortset.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < header.Level.Length; ++i)
            {
                header.Level[i].Span = 0;
                header.Level[i].Forward = null;
            }

            tail = null;
            level = 1;
            elementMapping.Clear();
            Count = 0;
        }

        /// <summary>
        /// Reverse traversal order.
        /// </summary>
        public void ReverseIterator()
        {
            ReverseIterator(!forward);
        }

        /// <summary>
        /// Reverse traversal order.
        /// </summary>
        /// <param name="forward">Whether to traverse from the forward.</param>
        public void ReverseIterator(bool forward)
        {
            this.forward = forward;
        }

        /// <summary>
        /// Gets the enumerator classes.
        /// </summary>
        /// <returns>Returns the enumerator classes.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, forward);
        }

        /// <inheritdoc />
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Convert to array.
        /// </summary>
        /// <returns>An element array.</returns>
        public TElement[] ToArray()
        {
            var elements = new TElement[Count];
            var node = header.Level[0];
            var i = 0;
            while (node.Forward != null)
            {
                elements[i++] = node.Forward.Element;
                node = node.Forward.Level[0];
            }

            return elements;
        }

        /// <summary>
        /// Gets the first element.
        /// </summary>
        /// <returns>The first element.</returns>
        public TElement First()
        {
            if (header.Level[0].Forward != null)
            {
                return header.Level[0].Forward.Element;
            }

            throw new InvalidOperationException("SortSet is Null");
        }

        /// <summary>
        /// Gets the last element.
        /// </summary>
        /// <returns>The last element.</returns>
        public TElement Last()
        {
            if (tail != null)
            {
                return tail.Element;
            }

            throw new InvalidOperationException("SortSet is Null");
        }

        /// <summary>
        /// Remove and return the elements at the head.
        /// </summary>
        /// <returns>The first element.</returns>
        public TElement Shift()
        {
            if (!Remove(header.Level[0].Forward, out TElement result))
            {
                throw new InvalidOperationException("SortSet is Null");
            }

            return result;
        }

        /// <summary>
        /// Remove and return the elements at the end.
        /// </summary>
        /// <returns>The last element.</returns>
        public TElement Pop()
        {
            if (!Remove(tail, out TElement result))
            {
                throw new InvalidOperationException("SortSet is Null");
            }

            return result;
        }

        /// <summary>
        /// Add a new record in sortset.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="score">The score.</param>
        public void Add(TElement element, TScore score)
        {
            Guard.Requires<ArgumentNullException>(element != null);
            Guard.Requires<ArgumentNullException>(score != null);

            if (elementMapping.TryGetValue(element, out TScore dictScore))
            {
                Remove(element, dictScore);
            }

            AddElement(element, score);
        }

        /// <summary>
        /// Whether is contains the specided element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>True if contains the specided element.</returns>
        public bool Contains(TElement element)
        {
            Guard.Requires<ArgumentNullException>(element != null);
            return elementMapping.ContainsKey(element);
        }

        /// <summary>
        /// Gets the element's score.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element's score.</returns>
        public TScore GetScore(TElement element)
        {
            Guard.Requires<ArgumentNullException>(element != null);
            if (!elementMapping.TryGetValue(element, out TScore score))
            {
                throw new KeyNotFoundException();
            }

            return score;
        }

        /// <summary>
        /// Get the number of elements in the specified score range.
        /// </summary>
        /// <param name="start">The start score.(contain).</param>
        /// <param name="end">The end score.(contain).</param>
        /// <returns>The number of elements in the score range.</returns>
        public int GetRangeCount(TScore start, TScore end)
        {
            Guard.Requires<ArgumentNullException>(start != null);
            Guard.Requires<ArgumentNullException>(end != null);
            Guard.Requires<ArgumentOutOfRangeException>(Compare(start, end) <= 0);

            int rank = 0, leftRank = 0;
            SkipNode leftCursor = null;

            var isRight = false;
            var cursor = header;

            do
            {
                for (var i = level - 1; i >= 0; --i)
                {
#pragma warning disable S2589
#pragma warning disable S2583
#pragma warning disable S2259
                    while (cursor.Level[i].Forward != null &&
                           ((!isRight && Compare(cursor.Level[i].Forward.Score, start) < 0) ||
                            (isRight && Compare(cursor.Level[i].Forward.Score, end) <= 0)))
#pragma warning disable S2589
#pragma warning disable S2583
#pragma warning disable S2259
                    {
                        rank += cursor.Level[i].Span;
                        cursor = cursor.Level[i].Forward;
                    }

                    if (leftCursor != null)
                    {
                        continue;
                    }

                    // First set the skip cursor and ranking of the top leftmost level.
                    // Than the cursor will start to look down.
                    leftCursor = cursor;
                    leftRank = rank;
                }

                if (isRight)
                {
                    continue;
                }

                cursor = leftCursor;

                var foo = rank;
                rank = leftRank;
                leftRank = foo;

#pragma warning disable S125

                // todo: removed it in new version.
                // leftRank ^= (rank ^= leftRank);
                // rank ^= leftRank;
#pragma warning restore S125
            }
#pragma warning disable S1121
            while (isRight = !isRight);
#pragma warning restore S1121

            return Math.Max(0, rank - leftRank);
        }

        /// <summary>
        /// Remove an element from the sortset, return false if the element does not exist.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Whether is removed the element.</returns>
        public bool Remove(TElement element)
        {
            Guard.Requires<ArgumentNullException>(element != null);

            return elementMapping.TryGetValue(element, out TScore dictScore) && Remove(element, dictScore);
        }

        /// <summary>
        /// Remove elements from the rank range.
        /// </summary>
        /// <param name="startRank">The start rank.(contains, 0 bottom).</param>
        /// <param name="stopRank">The end rank.(contains, 0 bottom).</param>
        /// <returns>Returns the removed elements count.</returns>
        public int RemoveRangeByRank(int startRank, int stopRank)
        {
            startRank = Math.Max(startRank, 0);
            Guard.Requires<ArgumentOutOfRangeException>(startRank <= stopRank);

            int traversed = 0, removed = 0;
            var update = new SkipNode[maxLevel];
            var cursor = header;
            for (var i = level - 1; i >= 0; --i)
            {
                while (cursor.Level[i].Forward != null &&
                       (traversed + cursor.Level[i].Span <= startRank))
                {
                    traversed += cursor.Level[i].Span;
                    cursor = cursor.Level[i].Forward;
                }

                update[i] = cursor;
            }

            cursor = cursor.Level[0].Forward;

            while (cursor != null &&
                    traversed <= stopRank)
            {
                var next = cursor.Level[0].Forward;
                elementMapping.Remove(cursor.Element);
                DeleteNode(cursor, update);
                ++removed;
                ++traversed;
                cursor = next;
            }

            return removed;
        }

        /// <summary>
        /// Remove elements from the score range.
        /// </summary>
        /// <param name="startScore">The start score.（contains）.</param>
        /// <param name="stopScore">The end score.（contains）.</param>
        /// <returns>Returns removed elements count.</returns>
        public int RemoveRangeByScore(TScore startScore, TScore stopScore)
        {
            Guard.Requires<ArgumentNullException>(startScore != null);
            Guard.Requires<ArgumentNullException>(stopScore != null);
            Guard.Requires<ArgumentOutOfRangeException>(Compare(startScore, stopScore) <= 0);

            var removed = 0;
            var update = new SkipNode[maxLevel];
            var cursor = header;
            for (var i = level - 1; i >= 0; --i)
            {
                while (cursor.Level[i].Forward != null &&
                       Compare(cursor.Level[i].Forward.Score, startScore) < 0)
                {
                    cursor = cursor.Level[i].Forward;
                }

                update[i] = cursor;
            }

            cursor = cursor.Level[0].Forward;

            while (cursor != null &&
                   Compare(cursor.Score, stopScore) <= 0)
            {
                var next = cursor.Level[0].Forward;
                elementMapping.Remove(cursor.Element);
                DeleteNode(cursor, update);
                ++removed;
                cursor = next;
            }

            return removed;
        }

        /// <summary>
        /// Get specific element rank.
        /// </summary>
        /// <param name="element">The specific element.</param>
        /// <returns>Returns the element rank(0 bottom) -1 means not found element.</returns>
        public int GetRank(TElement element)
        {
            Guard.Requires<ArgumentNullException>(element != null);
            return elementMapping.TryGetValue(element, out TScore dictScore) ? GetRank(element, dictScore) : -1;
        }

        /// <summary>
        /// Get the reverse ranking of the specified element.
        /// </summary>
        /// <param name="element">The specified element.</param>
        /// <returns>The element's rank(0 bottom)-1 means not found element.</returns>
        public int GetRevRank(TElement element)
        {
            Guard.Requires<ArgumentNullException>(element != null);
            var rank = GetRank(element);
            return rank < 0 ? rank : Count - rank - 1;
        }

        /// <summary>
        /// Get the elements in the ranking range.
        /// </summary>
        /// <param name="startRank">The start rank(contains).</param>
        /// <param name="stopRank">The stop rank(contains).</param>
        /// <returns>An array of the elements.</returns>
        public TElement[] GetElementRangeByRank(int startRank, int stopRank)
        {
            startRank = Math.Max(startRank, 0);
            Guard.Requires<ArgumentOutOfRangeException>(startRank <= stopRank);

            int traversed = 0;
            var cursor = header;
            for (var i = level - 1; i >= 0; --i)
            {
                while (cursor.Level[i].Forward != null &&
                       (traversed + cursor.Level[i].Span <= startRank))
                {
                    traversed += cursor.Level[i].Span;
                    cursor = cursor.Level[i].Forward;
                }
            }

            cursor = cursor.Level[0].Forward;

            var result = new List<TElement>();
            while (cursor != null &&
                   traversed <= stopRank)
            {
                result.Add(cursor.Element);
                ++traversed;
                cursor = cursor.Level[0].Forward;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get the elements in the score range.
        /// </summary>
        /// <param name="startScore">The start score（contains）.</param>
        /// <param name="stopScore">The end score（contains）.</param>
        /// <returns>An array of the elements.</returns>
        public TElement[] GetElementRangeByScore(TScore startScore, TScore stopScore)
        {
            Guard.Requires<ArgumentNullException>(startScore != null);
            Guard.Requires<ArgumentNullException>(stopScore != null);
            Guard.Requires<ArgumentOutOfRangeException>(Compare(startScore, stopScore) <= 0);

            var cursor = header;
            for (var i = level - 1; i >= 0; --i)
            {
                while (cursor.Level[i].Forward != null &&
                       Compare(cursor.Level[i].Forward.Score, startScore) < 0)
                {
                    cursor = cursor.Level[i].Forward;
                }
            }

            cursor = cursor.Level[0].Forward;

            var result = new List<TElement>();
            while (cursor != null &&
                   Compare(cursor.Score, stopScore) <= 0)
            {
                result.Add(cursor.Element);
                cursor = cursor.Level[0].Forward;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get element by rank.
        /// </summary>
        /// <param name="rank">The rank(0 bottom).</param>
        /// <returns>The element.</returns>
        public TElement GetElementByRank(int rank)
        {
            rank = Math.Max(0, rank);
            rank += 1;
            var traversed = 0;
            var cursor = header;
            for (var i = level - 1; i >= 0; i--)
            {
                while (cursor.Level[i].Forward != null &&
                       (traversed + cursor.Level[i].Span) <= rank)
                {
                    traversed += cursor.Level[i].Span;
                    cursor = cursor.Level[i].Forward;
                }

                if (traversed == rank)
                {
                    return cursor.Element;
                }
            }

            if (Count > 0)
            {
                throw new ArgumentOutOfRangeException($"Rank is out of range [{rank}]");
            }

            throw new InvalidOperationException("SortSet is Null");
        }

        /// <summary>
        /// Get element by reverse rank.
        /// </summary>
        /// <param name="rank">The rank.(0 bottom).</param>
        /// <returns>The element.</returns>
        public TElement GetElementByRevRank(int rank)
        {
            return GetElementByRank(Count - rank - 1);
        }

        /// <summary>
        /// Add an element in the sortset.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="score">The score.</param>
        private void AddElement(TElement element, TScore score)
        {
            int i;
            elementMapping.Add(element, score);

            var update = new SkipNode[maxLevel];
            var cursor = header;
            var rank = new int[maxLevel];

            // Find from high to low skip level
            for (i = level - 1; i >= 0; --i)
            {
                // Rank is the starting point of the node of the previous level as the starting point.
                rank[i] = i == (level - 1) ? 0 : rank[i + 1];
                while (cursor.Level[i].Forward != null &&
                        (Compare(cursor.Level[i].Forward.Score, score) < 0))
                {
                    rank[i] += cursor.Level[i].Span;
                    cursor = cursor.Level[i].Forward;
                }

                // Place the last node found in the node that needs to be updated
                update[i] = cursor;
            }

            var newLevel = GetRandomLevel();

            if (newLevel > level)
            {
                for (i = level; i < newLevel; ++i)
                {
                    rank[i] = 0;
                    update[i] = header;
                    update[i].Level[i].Span = Count;
                }

                level = newLevel;
            }

            // Point the cursor to a new skip node
            cursor = new SkipNode
            {
                Element = element,
                Score = score,
                Level = new SkipNode.SkipNodeLevel[newLevel],
            };

            for (i = 0; i < newLevel; ++i)
            {
                cursor.Level[i].Forward = update[i].Level[i].Forward;
                update[i].Level[i].Forward = cursor;
                cursor.Level[i].Span = update[i].Level[i].Span - (rank[0] - rank[i]);
                update[i].Level[i].Span = (rank[0] - rank[i]) + 1;
            }

            for (i = newLevel; i < level; ++i)
            {
                ++update[i].Level[i].Span;
            }

            cursor.Backward = (update[0] == header) ? null : update[0];

            if (cursor.Level[0].Forward != null)
            {
                cursor.Level[0].Forward.Backward = cursor;
            }
            else
            {
                tail = cursor;
            }

            ++Count;
        }

        /// <summary>
        /// Remove the skip node.
        /// </summary>
        /// <param name="node">The skip node.</param>
        /// <param name="element">The removed element.</param>
        /// <returns>Whether is removed the element.</returns>
        private bool Remove(SkipNode node, out TElement element)
        {
            if (node == null)
            {
                element = default(TElement);
                return false;
            }

            var result = node.Element;
            if (!Remove(node.Element, node.Score))
            {
                element = default(TElement);
                return false;
            }

            element = result;
            return true;
        }

        /// <summary>
        /// Remove the skip node.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="score">The score.</param>
        /// <returns>Whether is removed the element.</returns>
        private bool Remove(TElement element, TScore score)
        {
            Guard.Requires<ArgumentNullException>(element != null);
            Guard.Requires<ArgumentNullException>(score != null);

            var update = new SkipNode[maxLevel];
            var cursor = header;

            for (var i = level - 1; i >= 0; --i)
            {
                while (IsFindNext(cursor.Level[i].Forward, element, score, i))
                {
                    cursor = cursor.Level[i].Forward;
                }

                update[i] = cursor;
            }

            cursor = update[0].Level[0].Forward;

            if (cursor == null ||
                Compare(cursor.Score, score) != 0 ||
                        !cursor.Element.Equals(element))
            {
                return false;
            }

            elementMapping.Remove(element);
            DeleteNode(cursor, update);
            return true;
        }

        /// <summary>
        /// Get the element rank.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="score">The element score.</param>
        /// <returns>The element rank.</returns>
        private int GetRank(TElement element, TScore score)
        {
            var rank = 0;
            var cursor = header;
            for (var i = level - 1; i >= 0; --i)
            {
                while (IsFindNext(cursor.Level[i].Forward, element, score, i))
                {
                    rank += cursor.Level[i].Span;
                    cursor = cursor.Level[i].Forward;
                }
            }

            cursor = cursor.Level[0].Forward;

            if (cursor != null && cursor != header &&
                cursor.Element != null &&
                cursor.Element.Equals(element))
            {
                return rank;
            }

            return -1;
        }

        /// <summary>
        /// Determine if you need to find the next node.
        /// </summary>
        /// <param name="node">The skip not.</param>
        /// <param name="element">The element.</param>
        /// <param name="score">The element score.</param>
        /// <param name="level">The level.</param>
        /// <returns>True if find next.</returns>
        private bool IsFindNext(SkipNode node, TElement element, TScore score, int level)
        {
            if (node == null)
            {
                return false;
            }

            var compare = Compare(node.Score, score);
            if (compare < 0 || compare > 0)
            {
                return compare < 0;
            }

            // If the level is greater than 0, it means that it is possible
            // to directly locate element 2, resulting in a bug.
            // So we think that the level is greater than 0, then the value
            // is equal and still returns the pre-order jump node.
            //
            // ----------------------------------------------------
            // |  element 1（score：50）  | element 2 （score：50）
            // ----------------------------------------------------
            if (level > 0)
            {
                return false;
            }

            return !node.Element.Equals(element);
        }

        /// <summary>
        /// Delete the skip node.
        /// </summary>
        /// <param name="cursor">The skip node.</param>
        /// <param name="update">The updated node list.</param>
        private void DeleteNode(SkipNode cursor, SkipNode[] update)
        {
            for (var i = 0; i < level; ++i)
            {
                if (update[i].Level[i].Forward == cursor)
                {
                    update[i].Level[i].Span += cursor.Level[i].Span - 1;
                    update[i].Level[i].Forward = cursor.Level[i].Forward;
                }
                else
                {
                    update[i].Level[i].Span -= 1;
                }
            }

            if (cursor.Level[0].Forward != null)
            {
                cursor.Level[0].Forward.Backward = cursor.Backward;
            }
            else
            {
                tail = cursor.Backward;
            }

            while (level > 1 && header.Level[level - 1].Forward == null)
            {
                --level;
            }

            cursor.IsDeleted = true;
            --Count;
        }

        /// <summary>
        /// Get the rand level.
        /// </summary>
        /// <returns>The rand level.</returns>
        private int GetRandomLevel()
        {
            var newLevel = 1;
            while (random.Next(0, 0xFFFF) < probability)
            {
                ++newLevel;
            }

            return (newLevel < maxLevel) ? newLevel : maxLevel;
        }

        /// <summary>
        /// Compare left and right values.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>Return a value indicating which value is larger.</returns>
        private int Compare(TScore left, TScore right)
        {
            return comparer?.Compare(left, right) ?? left.CompareTo(right);
        }

        /// <summary>
        /// The default enumerator.
        /// </summary>
        public struct Enumerator : IEnumerator<TElement>
        {
            /// <summary>
            /// The sorset instance.
            /// </summary>
            private readonly SortSet<TElement, TScore> sortSet;

            /// <summary>
            /// Whether to traverse from the forward.
            /// </summary>
            private readonly bool forward;

            /// <summary>
            /// The current node.
            /// </summary>
            private SkipNode current;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/> struct.
            /// </summary>
            /// <param name="sortSet">The sortset instnace.</param>
            /// <param name="forward">Whether to traverse from the forward.</param>
            internal Enumerator(SortSet<TElement, TScore> sortSet, bool forward)
            {
                this.sortSet = sortSet;
                this.forward = forward;
                current = forward ? sortSet.header : null;
            }

            /// <inheritdoc />
            public TElement Current
            {
                get
                {
                    return current.Element;
                }
            }

            /// <inheritdoc />
            object IEnumerator.Current
            {
                get
                {
                    return current.Element;
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                // ignore.
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (forward)
                {
                    do
                    {
                        current = current.Level[0].Forward;
                    }
                    while (current != null && current.IsDeleted);
                    return current != null;
                }

                if (current == null)
                {
                    do
                    {
                        current = sortSet.tail;
                    }
                    while (current != null && current.IsDeleted);
                    return current != null;
                }

                do
                {
                    current = current.Backward;
                }
                while (current != null && current.IsDeleted);
                return current != null;
            }

            /// <inheritdoc />
            void IEnumerator.Reset()
            {
                current = forward ? sortSet.header : null;
            }
        }

        /// <summary>
        /// Represents a skip node.
        /// </summary>
        private class SkipNode
        {
            /// <summary>
            /// Gets or sets the element.
            /// </summary>
            public TElement Element { get; set; }

            /// <summary>
            /// Gets or sets the score.
            /// </summary>
            public TScore Score { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether was deleted.
            /// </summary>
            public bool IsDeleted { get; set; }

            /// <summary>
            /// Gets or sets the previous node.
            /// </summary>
            public SkipNode Backward { get; set; }

            /// <summary>
            /// Gets or sets the skip node levels.
            /// </summary>
            public SkipNodeLevel[] Level { get; set; }

            internal struct SkipNodeLevel
            {
                /// <summary>
                /// The next skip node.
                /// </summary>
                internal SkipNode Forward;

                /// <summary>
                /// The number represents how many nodes are crossed with the next node.
                /// </summary>
                internal int Span;
            }
        }
    }
}

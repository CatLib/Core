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
using System.Text;
using System.Text.RegularExpressions;

namespace CatLib.Support
{
    /// <summary>
    /// String helper.
    /// </summary>
    public static class Str
    {
        /// <summary>
        /// The space string.
        /// </summary>
        public const string Space = " ";

        /// <summary>
        /// Fill types.
        /// </summary>
        public enum PadType
        {
            /// <summary>
            /// Fill both sides of the string. If it is not even, the right side gets extra padding.
            /// </summary>
            Both,

            /// <summary>
            /// Fill the left side of the string.
            /// </summary>
            Left,

            /// <summary>
            /// Fill the right side of the string.
            /// </summary>
            Right,
        }

        /// <summary>
        /// Get the method name expressed by the string.
        /// </summary>
        /// <param name="pattern">The string will extract method name.</param>
        /// <returns>The method name.</returns>
        public static string Method(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return string.Empty;
            }

            var chars = new char[pattern.Length];
            var count = 0;
            for (var i = pattern.Length - 1; i >= 0; i--)
            {
                var segment = pattern[i];
                if ((segment >= 'A' && segment <= 'Z')
                    || (segment >= 'a' && segment <= 'z')
                    || (segment >= '0' && segment <= '9')
                    || segment == '_')
                {
                    chars[count++] = segment;
                    continue;
                }

                if (count > 0)
                {
                    break;
                }
            }

            for (var i = count - 1; i >= 0; i--)
            {
                if (chars[i] >= '0' && chars[i] <= '9')
                {
                    count--;
                    continue;
                }

                break;
            }

            Array.Resize(ref chars, count);
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Translate the specified string into an asterisk match expression and test.
        /// </summary>
        /// <param name="pattern">The match pattern.</param>
        /// <param name="value">The match value.</param>
        /// <returns>True if value is matched.</returns>
        public static bool Is(string pattern, string value)
        {
            return pattern == value || Regex.IsMatch(value, "^" + AsteriskWildcard(pattern) + "$");
        }

        /// <summary>
        /// Translate the specified string into an asterisk match expression and test.
        /// <para>Returns true if any match patten matches.</para>
        /// </summary>
        /// <typeparam name="T">The type of source array.</typeparam>
        /// <param name="patterns">The match pattern.</param>
        /// <param name="value">The match value.</param>
        /// <returns>True if matches.</returns>
        public static bool Is<T>(string[] patterns, T value)
        {
            if (patterns == null || value == null)
            {
                return false;
            }

            foreach (var pattern in patterns)
            {
                if (Is(pattern, value.ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Translate the specified string into an asterisk match expression.
        /// </summary>
        /// <param name="pattern">The match pattern.</param>
        /// <returns>Returns processed string.</returns>
        public static string AsteriskWildcard(string pattern)
        {
            pattern = Regex.Escape(pattern);
            return pattern.Replace(@"\*", ".*?");
        }

        /// <summary>
        /// Split a string into an array based on length.
        /// </summary>
        /// <param name="str">The specified string.</param>
        /// <param name="length">Specify the length of each array element.</param>
        /// <returns>Returns an array of the string.</returns>
        public static string[] Split(string str, int length = 1)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Array.Empty<string>();
            }

            length = Math.Max(1, length);
            var ret = new string[(str.Length / length) + (str.Length % length == 0 ? 0 : 1)];
            for (var i = 0; i < str.Length; i += length)
            {
                ret[i / length] = str.Substring(i, Math.Min(str.Length - i, length));
            }

            return ret;
        }

        /// <summary>
        /// Repeat the specified number of times the string.
        /// </summary>
        /// <param name="str">String that needs to be repeated.</param>
        /// <param name="num">Number of repetitions.</param>
        /// <returns>Return the repeated string.</returns>
        public static string Repeat(string str, int num)
        {
            num = Math.Max(0, num);

            if (string.IsNullOrEmpty(str) || num == 0)
            {
                return string.Empty;
            }

            var ret = new StringBuilder();
            for (var i = 0; i < num; i++)
            {
                ret.Append(str);
            }

            return ret.ToString();
        }

        /// <summary>
        /// Disrupt all characters in the string.
        /// </summary>
        /// <param name="str">The specified string.</param>
        /// <param name="seed">The random seed.</param>
        /// <returns>Returns disrupted string.</returns>
        public static string Shuffle(string str, int? seed = null)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            var random = Helper.MakeRandom(seed);
            var ret = new string[str.Length];
            for (var i = 0; i < str.Length; i++)
            {
                var index = random.Next(0, str.Length - 1);

                ret[i] = ret[i] ?? str.Substring(i, 1);
                ret[index] = ret[index] ?? str.Substring(index, 1);

                if (index == i)
                {
                    continue;
                }

#pragma warning disable S4143
                var temporary = ret[i];
                ret[i] = ret[index];
                ret[index] = temporary;
#pragma warning restore S4143
            }

            return Arr.Reduce(ret, (a, b) => a + b, string.Empty);
        }

        /// <summary>
        /// Calculate the number of times a substring appears in a string.
        /// <para>This function does not count overlapping substrings.</para>
        /// </summary>
        /// <param name="str">The specified string.</param>
        /// <param name="substr">The substring.</param>
        /// <param name="start">The starting position.</param>
        /// <param name="length">The length to calculate.</param>
        /// <param name="comparison">The string comparison.</param>
        /// <returns>Returns the number of times a substring appears. -1 means unable to calculate.</returns>
        public static int SubstringCount(string str, string substr, int start = 0, int? length = null, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(substr))
            {
                return 0;
            }

            Helper.NormalizationPosition(str.Length, ref start, ref length);

            var count = 0;
            while (length.Value > 0)
            {
                int index;
                if ((index = str.IndexOf(substr, start, length.Value, comparison)) < 0)
                {
                    break;
                }

                count++;
                length -= index + substr.Length - start;
                start = index + substr.Length;
            }

            return count;
        }

        /// <summary>
        /// Reverse specified string.
        /// </summary>
        /// <param name="str">The specified string.</param>
        /// <returns>Returns reversed string.</returns>
        public static string Reverse(string str)
        {
            var chars = str.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Fill the string with the new length.
        /// </summary>
        /// <param name="length">The new string length. If the value is less than the original length of the string, no action is taken.</param>
        /// <param name="str">The string to be filled.</param>
        /// <param name="padStr">A string to be used for padding. The default is blank.</param>
        /// <param name="type">
        /// Fill in which side of the string.
        /// <para><see cref="PadType.Both"/>Fill both sides of the string. If not even, get extra padding on the right side.</para>
        /// <para><see cref="PadType.Left"/>Fill the left side of the string.</para>
        /// <para><see cref="PadType.Right"/>Fill the right side of the string.</para>
        /// </param>
        /// <returns>Returns filled string.</returns>
        public static string Pad(int length, string str = null, string padStr = null, PadType type = PadType.Right)
        {
            str = str ?? string.Empty;

            var needlePadding = length - str.Length;
            if (needlePadding <= 0)
            {
                return str;
            }

            int rightPadding;
            var leftPadding = rightPadding = 0;

            if (type == PadType.Both)
            {
                leftPadding = needlePadding >> 1;
                rightPadding = (needlePadding >> 1) + (needlePadding % 2 == 0 ? 0 : 1);
            }
            else if (type == PadType.Right)
            {
                rightPadding = needlePadding;
            }
            else
            {
                leftPadding = needlePadding;
            }

            padStr = padStr ?? Space;
            padStr = padStr.Length <= 0 ? Space : padStr;

            var leftPadCount = (leftPadding / padStr.Length) + (leftPadding % padStr.Length == 0 ? 0 : 1);
            var rightPadCount = (rightPadding / padStr.Length) + (rightPadding % padStr.Length == 0 ? 0 : 1);

            return Repeat(padStr, leftPadCount).Substring(0, leftPadding) + str +
                   Repeat(padStr, rightPadCount).Substring(0, rightPadding);
        }

        /// <summary>
        /// Finds the specified value in the string and returns the rest.
        /// <para>If not found, return the specified string itself.</para>
        /// </summary>
        /// <param name="str">The specified string.</param>
        /// <param name="search">The search value.</param>
        /// <returns>The remaining part.</returns>
        public static string After(string str, string search)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(search))
            {
                return str ?? string.Empty;
            }

            var index = str.IndexOf(search, StringComparison.Ordinal);
            return index < 0 ? str : str.Substring(index + search.Length, str.Length - index - search.Length);
        }

        /// <summary>
        /// Determine whether the specified string contains the specified substring.
        /// <para>Substrings are case sensitive.</para>
        /// <para></para>
        /// </summary>
        /// <param name="str">The specified string.</param>
        /// <param name="needles">An array of the specified substring.</param>
        /// <returns>True if contains substring.</returns>
        public static bool Contains(string str, params string[] needles)
        {
            needles = needles ?? Array.Empty<string>();

            foreach (var needle in needles)
            {
                if (str == needle || str.Contains(needle))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Replace the match in the specified string.
        /// </summary>
        /// <param name="matches">An array of the match string.</param>
        /// <param name="replace">The replacement value.</param>
        /// <param name="str">The specified string.</param>
        /// <returns>Returns the replacement string.</returns>
        public static string Replace(string[] matches, string replace, string str)
        {
            matches = matches ?? Array.Empty<string>();
            replace = replace ?? string.Empty;

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            foreach (var match in matches)
            {
                if (match == null)
                {
                    continue;
                }

                str = str.Replace(match, replace);
            }

            return str;
        }

        /// <summary>
        /// Replace the first occurrence of a match in the specified string.
        /// <para>This function is case sensitive.</para>
        /// </summary>
        /// <param name="match">The match string.</param>
        /// <param name="replace">The replacement value.</param>
        /// <param name="str">The specified string.</param>
        /// <returns>Returns the replacement string.</returns>
        public static string ReplaceFirst(string match, string replace, string str)
        {
            if (string.IsNullOrEmpty(match) || string.IsNullOrEmpty(str))
            {
                return str ?? string.Empty;
            }

            replace = replace ?? string.Empty;
            var index = str.IndexOf(match, StringComparison.Ordinal);
            return index < 0 ? str : str.Remove(index, match.Length).Insert(index, replace);
        }

        /// <summary>
        /// Replaces the first occurrence of a match in the specified string from the back to the front.
        /// <para>This function is case sensitive.</para>
        /// </summary>
        /// <param name="match">The match string.</param>
        /// <param name="replace">The replacement value.</param>
        /// <param name="str">The specified string.</param>
        /// <returns>Returns the replacement string.</returns>
        public static string ReplaceLast(string match, string replace, string str)
        {
            if (string.IsNullOrEmpty(match) || string.IsNullOrEmpty(str))
            {
                return str ?? string.Empty;
            }

            replace = replace ?? string.Empty;
            var index = str.LastIndexOf(match, StringComparison.Ordinal);
            return index < 0 ? str : str.Remove(index, match.Length).Insert(index, replace);
        }

        /// <summary>
        /// Generate a random letter (with case), a string of numbers.
        /// </summary>
        /// <param name="length">The length of the generate string.</param>
        /// <param name="seed">The random seed.</param>
        /// <returns>The random string.</returns>
        public static string Random(int length = 16, int? seed = null)
        {
            length = Math.Max(1, length);

            var ret = new StringBuilder();
            var random = Helper.MakeRandom(seed);
            for (int len; (len = ret.Length) < length;)
            {
                var size = length - len;
                var bytes = new byte[size];
                random.NextBytes(bytes);

                var code = Replace(new[] { "/", "+", "=" }, string.Empty, Convert.ToBase64String(bytes));
                ret.Append(code.Substring(0, Math.Min(size, code.Length)));
            }

            return ret.ToString();
        }

        /// <summary>
        /// If the length exceeds the given maximum string length, the string is truncated.
        /// The last character of the truncated string will be replaced with the mission string.
        /// <para>eg: Str.Truncate("hello world , the sun is shine", 15, Str.Space) => hello world...</para>
        /// </summary>
        /// <param name="str">The string to be truncated.</param>
        /// <param name="length">Truncation length (with default character length).</param>
        /// <param name="separator">The adjacent separator, if set, truncates the separator position with the length of the truncation length, and uses a regular match if a regular expression is passed.</param>
        /// <param name="mission">The mission string.</param>
        /// <returns>Reutrns truncated string.</returns>
        public static string Truncate(string str, int length, object separator = null, string mission = null)
        {
            if (str == null || length > str.Length)
            {
                return str;
            }

            mission = mission ?? "...";
            var end = length - mission.Length;

            if (end < 1)
            {
                return mission;
            }

            var ret = str.Substring(0, end);

            if (separator == null)
            {
                return ret + mission;
            }

            var separatorStr = separator.ToString();
            var index = -1;
            if (separator is Regex separatorRegex)
            {
                if (separatorRegex.IsMatch(ret))
                {
                    index = (separatorRegex.RightToLeft
                        ? separatorRegex.Match(ret)
                        : Regex.Match(ret, separatorRegex.ToString(),
                            separatorRegex.Options | RegexOptions.RightToLeft)).Index;
                }
            }
            else if (!string.IsNullOrEmpty(separatorStr) && str.IndexOf(separatorStr, StringComparison.Ordinal) != end)
            {
                index = ret.LastIndexOf(separatorStr, StringComparison.Ordinal);
            }

            if (index > -1)
            {
                ret = ret.Substring(0, index);
            }

            return ret + mission;
        }

        /// <summary>
        /// Calculate Levenshtein distance between two strings.
        /// </summary>
        /// <param name="a">The string 1.</param>
        /// <param name="b">The string 2.</param>
        /// <returns>
        /// This function returns the Levenshtein-Distance between the two argument
        /// strings or -1, if one of the argument strings is longer than the limit
        /// of 255 characters.
        /// </returns>
        public static int Levenshtein(string a, string b)
        {
            if (a == null || b == null)
            {
                return -1;
            }

            var lengthA = a.Length;
            var lengthB = b.Length;

            if (lengthA > 255 || lengthB > 255)
            {
                return -1;
            }

            var pA = new int[lengthB + 1];
            var pB = new int[lengthB + 1];

            for (var i = 0; i <= lengthB; i++)
            {
                pA[i] = i;
            }

            int Min(int num1, int num2, int num3)
            {
                var min = num1;
                if (min > num2)
                {
                    min = num2;
                }

                if (min > num3)
                {
                    min = num3;
                }

                return min;
            }

            for (var i = 0; i < lengthA; i++)
            {
                pB[0] = pA[0] + 1;
                for (var n = 0; n < lengthB; n++)
                {
                    var distance = a[i] == b[n]
                        ? Min(pA[n], pA[n + 1] + 1, pB[n] + 1)
                        : Min(pA[n] + 1, pA[n + 1] + 1, pB[n] + 1);
                    pB[n + 1] = distance;
                }

                var temp = pA;
                pA = pB;
                pB = temp;
            }

            return pA[lengthB];
        }

        /// <summary>
        /// Returns all sequential combination of the given array.
        /// </summary>
        /// <remarks>
        /// v[0] = "hello"
        /// v[1] = "world"
        /// var result = Str.JoinList(v, "/");
        /// result[0] == "hello";
        /// result[1] == "hello/world";.
        /// </remarks>
        /// <param name="sources">The source array.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The sequential combination array.</returns>
        public static string[] JoinList(string[] sources, string separator = null)
        {
            sources = sources ?? Array.Empty<string>();
            var ret = new StringBuilder();
            for (var index = 1; index < sources.Length; index++)
            {
                ret.Append(sources[index - 1]);
                if (!string.IsNullOrEmpty(separator))
                {
                    ret.Append(separator);
                }

                ret.Append(sources[index]);
                sources[index] = ret.ToString();
                ret.Remove(0, sources[index].Length);
            }

            return sources;
        }

        /// <inheritdoc cref="JoinList(string[], char)"/>
        public static string[] JoinList(string[] source, char separator)
        {
            return JoinList(source, separator.ToString());
        }
    }
}

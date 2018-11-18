// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This static class implements an extension for <see cref="string"/> for conversion special purposes.
    /// </summary>
    public static class StringExtension
    {
        private static Regex verseNumberExpression = new Regex(@"^\d+\.$", RegexOptions.Compiled);

        /// <summary>
        /// This method splits a string coming from capella file and containing lyric text and split it by predefined key characters (-, #, $).
        /// </summary>
        /// <param name="text">This parameter is the lyric text to be split.</param>
        /// <returns>The method returns an array of text items.</returns>
        public static string[] SplitForLyrics(this string text)
        {
            var result = new List<string>();
            int position = 0;
            string buffer = string.Empty;
            while (position < text.Length)
            {
                int next = text.IndexOfAny(" -".ToCharArray(), position);
                if (next < 0)
                {
                    result.Add(buffer + text.Substring(position));
                    break;
                }

                string substring = text.Substring(position, next - position + (text[next] == ' ' ? 0 : 1));
                if (verseNumberExpression.IsMatch(substring))
                {
                    // join with next one
                    buffer += substring + " ";
                }
                else
                {
                    result.Add(buffer + substring);

                    // reset
                    buffer = string.Empty;
                }

                position = next + 1;
            }

            return result.ToArray();
        }
    }
}

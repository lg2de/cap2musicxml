// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.Processing
{
    using lg2de.cap2musicxml.musicxml;

    internal class LyricsProcessor
    {
        private readonly string[][] textElements;
        private readonly bool[] isSyllabic;

        private uint currentIndex = 0;

        public LyricsProcessor(string text)
        {
            this.textElements = text.SplitForLyrics();
            this.isSyllabic = new bool[this.textElements.Length];
        }

        public void AddLyrics(note note)
        {
            for (int stave = 0; stave < this.textElements.Length; stave++)
            {
                if (this.currentIndex >= this.textElements[stave].Length)
                {
                    // no (more) text available in stave
                    continue;
                }

                string newText = this.textElements[stave][this.currentIndex];
                var newSyllabic = newText.EndsWith("-");
                if (this.currentIndex + 1 < this.textElements[stave].Length)
                {
                    var nextText = this.textElements[stave][this.currentIndex + 1];
                    newSyllabic |= nextText.StartsWith("#");
                }

                if (newText.StartsWith("#") && this.isSyllabic[stave])
                {
                    // Syllabic is already "active"
                    // Text is Capella syllabic sign which is not needed anymore.
                    continue;
                }

                syllabic? syllabicType = null;
                if (this.isSyllabic[stave] != newSyllabic)
                {
                    // type has changed
                    syllabicType = newSyllabic ? syllabic.begin : syllabic.end;
                }
                else if (this.isSyllabic[stave])
                {
                    // syllabic still "active"
                    syllabicType = syllabic.middle;
                }

                newText = newText.Replace("$", string.Empty).TrimEnd('-');
                var lyric = note.AddLyric(newText, syllabicType);
                lyric.number = (stave + 1).ToString();
                this.isSyllabic[stave] = newSyllabic;
            }

            this.currentIndex++;
        }
    }
}

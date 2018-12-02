// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.Processing
{
    using lg2de.cap2musicxml.musicxml;

    internal class LyricsProcessor
    {
        private readonly string[] textElements;

        private uint currentIndex = 0;
        private bool isSyllabic;

        public LyricsProcessor(string text)
        {
            this.textElements = text.SplitForLyrics();
        }

        public void AddLyrics(note note)
        {
            if (this.currentIndex >= this.textElements.Length)
            {
                // no (more) text available
                return;
            }

            string newText = this.textElements[this.currentIndex];
            this.currentIndex++;
            var newSyllabic = newText.EndsWith("-");
            if (this.currentIndex < this.textElements.Length)
            {
                var nextText = this.textElements[this.currentIndex];
                newSyllabic |= nextText.StartsWith("#");
            }

            if (newText.StartsWith("#") && this.isSyllabic)
            {
                // Syllabic is already "active"
                // Text is Capella syllabic sign which is not needed anymore.
                return;
            }

            bool? syllabicState = null;
            if (this.isSyllabic != newSyllabic)
            {
                syllabicState = newSyllabic;
            }

            newText = newText.Replace("$", string.Empty).TrimEnd('-');
            var lyric = note.AddLyric(newText, syllabicState);
            this.isSyllabic = newSyllabic;
        }
    }
}

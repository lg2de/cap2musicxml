// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using lg2de.cap2musicxml.musicxml;
using lg2de.cap2musicxml.Processing;
using Xunit;

namespace lg2de.cap2musicxml.Tests
{
    public class LyricsProcessorTests
    {
        [Theory]
        [InlineData("Ly-#-rics")]
        [InlineData("Ly # rics")]
        public void AddLyrics_ManyNotesForSingleSyllabic_StartAndEndCorrect(string input)
        {
            var sut = new LyricsProcessor(input);
            note note1 = new note();
            note note2 = new note();
            note note3 = new note();
            sut.AddLyrics(note1);
            sut.AddLyrics(note2);
            sut.AddLyrics(note3);

            using (new AssertionScope())
            {
                note1.lyric.Should().HaveCount(1);
                note1.lyric[0].Items.Should().BeEquivalentTo(
                    syllabic.begin,
                    new { Value = "Ly" });

                note2.lyric.Should().BeNullOrEmpty();

                note3.lyric.Should().HaveCount(1);
                note3.lyric[0].Items.Should().BeEquivalentTo(
                    syllabic.end,
                    new { Value = "rics" });
            }
        }
    }
}

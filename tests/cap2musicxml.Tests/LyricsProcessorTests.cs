// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

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
        public void AddLyrics_Syllabics_StartAndEndCorrect(string input)
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
                note1.lyric.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        number = "1",
                        Items = new dynamic[] { syllabic.begin, new { Value = "Ly" } }
                    }
                });

                note2.lyric.Should().BeNullOrEmpty();

                note3.lyric.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        number = "1",
                        Items = new dynamic[] { syllabic.end, new { Value = "rics" } }
                    }
                });
            }
        }

        [Theory]
        [InlineData("Mu-sic-xml")]
        public void AddLyrics_Syllabics_MiddleCorrect(string input)
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
                note1.lyric.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        number = "1",
                        Items = new dynamic[] { syllabic.begin, new { Value = "Mu" } }
                    }
                });

                note2.lyric.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        number = "1",
                        Items = new dynamic[] { syllabic.middle, new { Value = "sic" } }
                    }
                });

                note3.lyric.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        number = "1",
                        Items = new dynamic[] { syllabic.end, new { Value = "xml" } }
                    }
                });
            }
        }

        [Fact]
        public void AddLyrics_MultipleStaves_TextCorrectlyAssigned()
        {
            var sut = new LyricsProcessor("Ly-#-rics\r\nJust three words");
            note note1 = new note();
            note note2 = new note();
            note note3 = new note();
            sut.AddLyrics(note1);
            sut.AddLyrics(note2);
            sut.AddLyrics(note3);

            using (new AssertionScope())
            {
                note1.lyric.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        number = "1",
                        Items = new dynamic[] { syllabic.begin, new { Value = "Ly" } }
                    },
                    new
                    {
                        number = "2",
                        Items = new dynamic[] { new { Value = "Just" } }
                    }
                });

                note2.lyric.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        number = "2",
                        Items = new dynamic[] { new { Value = "three" } }
                    }
                });

                note3.lyric.Should().BeEquivalentTo(new[]
                {
                    new
                    {
                        number = "1",
                        Items = new dynamic[] { syllabic.end, new { Value = "rics" } }
                    },
                    new
                    {
                        number = "2",
                        Items = new dynamic[] { new { Value = "words" } }
                    }
                });
            }
        }
    }
}

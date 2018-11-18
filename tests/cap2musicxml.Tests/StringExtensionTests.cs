// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.Tests
{
    using FluentAssertions;
    using Xunit;

    public class StringExtensionTests
    {
        [Fact]
        public void SplitForLyrics_Sample_SyllablesExtracted()
        {
            var sut = "My first score with syl-la-bles.";
            sut.SplitForLyrics().Should().Equal(
                "My", "first", "score", "with", "syl-", "la-", "bles.");
        }

        [Fact]
        public void SplitForLyrics_LeadingVerseNumber_NumberCollected()
        {
            var sut = "1. Text";
            sut.SplitForLyrics().Should().Equal("1. Text");
        }
    }
}

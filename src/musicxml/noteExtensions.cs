// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.musicxml
{
    public partial class note
    {
        public void AddPitch(pitch pitch)
        {
            this.Items = ArrayExtensions.ArrayAppend(this.Items, pitch);
            this.ItemsElementName = ArrayExtensions.ArrayAppend(this.ItemsElementName, ItemsChoiceType1.pitch);
        }

        public void AddDuration(decimal duration)
        {
            this.Items = ArrayExtensions.ArrayAppend(this.Items, duration);
            this.ItemsElementName = ArrayExtensions.ArrayAppend(this.ItemsElementName, ItemsChoiceType1.duration);
        }

        public lyric AddLyric(string text, bool? isSyllabic)
        {
            var newLyric = new lyric();
            this.lyric = ArrayExtensions.ArrayAppend(this.lyric, newLyric);

            syllabic? syllabicValue = null;
            if (isSyllabic.HasValue)
            {
                syllabicValue = isSyllabic.Value ? syllabic.begin : syllabic.end;
            }

            if (syllabicValue.HasValue)
            {
                newLyric.Items = new object[2]
                {
                    syllabicValue.Value,
                    new textelementdata { Value = text }
                };
                newLyric.ItemsElementName = new ItemsChoiceType6[2]
                {
                    ItemsChoiceType6.syllabic,
                    ItemsChoiceType6.text
                };
            }
            else
            {
                newLyric.Items = new object[1]
                {
                    new textelementdata { Value = text }
                };
                newLyric.ItemsElementName = new ItemsChoiceType6[1]
                {
                    ItemsChoiceType6.text
                };
            }

            return newLyric;
        }

        public void SetChord()
        {
            this.Items = ArrayExtensions.ArrayAppend(this.Items, new empty());
            this.ItemsElementName = ArrayExtensions.ArrayAppend(this.ItemsElementName, ItemsChoiceType1.chord);
        }

        public rest AddRest()
        {
            var theRest = new rest();
            this.Items = ArrayExtensions.ArrayAppend(this.Items, theRest);
            this.ItemsElementName = ArrayExtensions.ArrayAppend(this.ItemsElementName, ItemsChoiceType1.rest);
            return theRest;
        }

        public void AddTieStart()
        {
            this.Items = ArrayExtensions.ArrayAppend(this.Items, new tie { type = startstop.start });
            this.ItemsElementName = ArrayExtensions.ArrayAppend(this.ItemsElementName, ItemsChoiceType1.tie);
            var theNotation = new notations();
            this.notations = ArrayExtensions.ArrayAppend(this.notations, theNotation);
            theNotation.AddTiedStart();
        }

        public void AddTieStop()
        {
            this.Items = ArrayExtensions.ArrayAppend(this.Items, new tie { type = startstop.stop });
            this.ItemsElementName = ArrayExtensions.ArrayAppend(this.ItemsElementName, ItemsChoiceType1.tie);
            var theNotation = new notations();
            this.notations = ArrayExtensions.ArrayAppend(this.notations, theNotation);
            theNotation.AddTiedStop();
        }
    }
}

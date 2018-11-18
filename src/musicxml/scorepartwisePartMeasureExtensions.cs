// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.musicxml
{
    using System;

    public partial class scorepartwisePartMeasure
    {
        public void AddClef(int keyForm, int keyLine, int keyOctavation)
        {
            attributes att = this.GetAttributes();
            att.clef = new clef[1];

            att.clef[0] = new clef();
            switch (keyForm)
            {
                case 0:
                    att.clef[0].sign = clefsign.G;
                    att.clef[0].line = (5 - keyLine).ToString();
                    break;
                case 1:
                    att.clef[0].sign = clefsign.C;
                    att.clef[0].line = (5 - keyLine).ToString();
                    break;
                case 2:
                    att.clef[0].sign = clefsign.F;
                    att.clef[0].line = (5 - keyLine).ToString();
                    break;
                case 3:
                    att.clef[0].sign = clefsign.percussion;
                    break;
                case 4:
                    att.clef[0].sign = clefsign.none;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (keyOctavation == 0)
            {
                att.clef[0].clefoctavechange = "1";
            }
            else if (keyOctavation == 2)
            {
                att.clef[0].clefoctavechange = "-1";
            }
        }

        public void AddSignature(int signature)
        {
            attributes att = this.GetAttributes();
            var theKey = new key();
            att.key = ArrayExtensions.ArrayAppend(att.key, theKey);

            theKey.Items = ArrayExtensions.ArrayAppend(theKey.Items, (signature - 7).ToString());
            theKey.ItemsElementName = ArrayExtensions.ArrayAppend(theKey.ItemsElementName, ItemsChoiceType8.fifths);
        }

        public void AddNote(note newNote)
        {
            this.Items = ArrayExtensions.ArrayAppend(this.Items, newNote);
        }

        public void AddBackup(decimal duration)
        {
            this.Items = ArrayExtensions.ArrayAppend(this.Items, new backup { duration = duration });
        }

        public attributes GetAttributes()
        {
            if (this.Items != null)
            {
                foreach (object t in this.Items)
                {
                    if (!(t is attributes))
                    {
                        continue;
                    }

                    return t as attributes;
                }
            }

            var att = new attributes();
            this.Items = ArrayExtensions.ArrayAppend(this.Items, att);
            return att;
        }
    }
}

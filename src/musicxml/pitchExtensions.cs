// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.musicxml
{
    using System.Globalization;

    /// <summary>
    /// This class part implements some extensions to the generated part of <see cref="pitch"/>.
    /// </summary>
    public partial class pitch
    {
        private static int[] DurTonleiter = { 0, 2, 4, 5, 7, 9, 11 };

        /// <summary>
        /// This method creates a <see cref="pitch"/> object according parameters with capella logic.
        /// </summary>
        /// <param name="relativePitch">This parameter shall be between x and x.</param>
        /// <param name="alteration">This parameter shall be between -2 and 2.</param>
        /// <param name="keyForm">This parameter gives the current key form.</param>
        /// <param name="keyOctavation">This parameter gives the current key octave.</param>
        /// <returns>The method returns a new <see cref="pitch"/> object.</returns>
        public static pitch CreatePitch(int relativePitch, int alteration, int keyForm, int keyOctavation, int signature)
        {
            var thePitch = new pitch();

            if (keyForm == 1)
            {
                relativePitch -= 7;
            }
            else if (keyForm == 2)
            {
                relativePitch -= 14;
            }

            if (keyOctavation == 0)
            {
                relativePitch += 7;
            }
            else if (keyOctavation == 2)
            {
                relativePitch -= 7;
            }

            int octave = 5;
            while (relativePitch < 0)
            {
                relativePitch += 7;
                octave--;
            }

            while (relativePitch > 6)
            {
                relativePitch -= 7;
                octave++;
            }

            //switch (relativePitch)
            //{
            //    case 0:
            //        thePitch.step = step.C;
            //        break;
            //    case 1:
            //        thePitch.step = step.D;
            //        break;
            //    case 2:
            //        thePitch.step = step.E;
            //        break;
            //    case 3:
            //        thePitch.step = step.F;
            //        break;
            //    case 4:
            //        thePitch.step = step.G;
            //        break;
            //    case 5:
            //        thePitch.step = step.A;
            //        break;
            //    case 6:
            //        thePitch.step = step.B;
            //        break;
            //}

            var cromatic = DurTonleiter[relativePitch];
            var signaturePitch = cromatic + ((((signature - 7) * 7) + 60) % 12);
            while (signaturePitch >= 12)
            {
                signaturePitch -= 12;
                octave++;
            }

            thePitch.octave = octave.ToString(CultureInfo.InvariantCulture);

            switch (signaturePitch)
            {
                case 0:
                    thePitch.step = step.C;
                    break;
                case 1:
                    thePitch.step = step.C;
                    alteration++;
                    break;
                case 2:
                    thePitch.step = step.D;
                    break;
                case 3:
                    thePitch.step = step.D;
                    alteration++;
                    break;
                case 4:
                    thePitch.step = step.E;
                    break;
                case 5:
                    thePitch.step = step.F;
                    break;
                case 6:
                    thePitch.step = step.F;
                    alteration++;
                    break;
                case 7:
                    thePitch.step = step.G;
                    break;
                case 8:
                    thePitch.step = step.G;
                    alteration++;
                    break;
                case 9:
                    thePitch.step = step.A;
                    break;
                case 10:
                    thePitch.step = step.A;
                    alteration++;
                    break;
                case 11:
                    thePitch.step = step.B;
                    break;
            }

            if (alteration != 0)
            {
                thePitch.alter = alteration;
                thePitch.alterSpecified = true;
            }

            return thePitch;
        }
    }
}
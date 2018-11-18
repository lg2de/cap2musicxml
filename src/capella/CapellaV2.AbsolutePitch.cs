// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.capella
{
    /// <content>
    /// This part implements calculation of chromatic pitch according cappella guide.
    /// </content>
    public partial class CapellaV2
    {
        // C2FORMAT.TXT line 485ff
        //function chromatischeTonhoehe (Schluessel_Form, Schluessel_Oktavierung, Tonart,
        //                               relative_diatonische_Hoehe, Alteration: integer)
        //                               : integer;
        //(*
        //  Die ersten drei Parameter ergeben sich aus dem aktuellen Zeilenzustand
        //  (Zeilenattribut bzw. letzter Moduswechsel), 
        //  die beiden letzten aus der Noteninfo (NOTE) 
        //*)
        //const Dur_Tonleiter: array[0..6] of integer = (0,2,4,5,7,9,11);
        //var   d, Oktave, relDiatonic, relChromatic, relTonarthoehe: integer;
        //begin
        //  d := relative_diatonische_Hoehe + 42;
        //  case Schluessel_Form of
        //    1: d := d - 7;   (* C-keyByte eine Oktave tiefer  *)
        //    2: d := d - 14;  (* F-keyByte zwei Oktaven tiefer *)
        //  end;
        //  case Schluessel_Oktavierung of
        //    0: d := d + 7;
        //    2: d := d - 7;
        //  end;
        //  Oktave := d div 7;  
        //  relDiatonic := d mod 7;
        //  relChromatic := 12 * Oktave + Dur_Tonleiter[relDiatonic] + Alteration;
        //  relTonarthoehe := ((Tonart-7) * 7 + 60) mod 12; (* Quintenzirkel *)
        //  chromatischeTonhoehe := relChromatic + relTonarthoehe;
        //end;
        private int AbsolutePitch(int key, int octavation, int signature, int relativePitch, int alteration)
        {
            var major = new[] { 0, 2, 4, 5, 7, 9, 11 };
            int d = relativePitch + 42;
            switch (key)
            {
                case 1: // C
                    d -= 7;
                    break;
                case 2: // F
                    d -= 14;
                    break;
            }

            switch (octavation)
            {
                case 0:
                    d += 7;
                    break;
                case 2:
                    d -= 7;
                    break;
            }

            int oktave = d / 7;
            int relDiatonic = d % 7;
            int relChromatic = (12 * oktave) + major[relDiatonic] + alteration;
            relativePitch = (((signature - 7) * 7) + 60) % 12; // Quintenzirkel

            return relChromatic + relativePitch;
        }
    }
}

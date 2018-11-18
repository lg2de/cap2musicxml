// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.capella
{
    using System;
    using System.Globalization;
    using musicxml;

    /// <content>
    /// This part implements handling for the section of accords.
    /// </content>
    public partial class CapellaV2
    {
        private void ProcessAccords(
            int staffNumber, int measureIndex, int voiceNumber, BYTE[] accords, INT[] accordIndexes, string text)
        {
            Console.WriteLine($"Processing staff {staffNumber}, measure {measureIndex}, voice {voiceNumber}");

            // prepare lyric text
            string[] lyricElemets = text.SplitForLyrics();
            int currentLyricIndex = 0;

            // iterate data
            int index = 0;
            int accordIndex = 0;
            int currentMeasureTicks = 0;
            double tripletRests = 0;

            while (index < accords.Length)
            {
                scorepartwisePartMeasure currentMeasure = this.measures[staffNumber][measureIndex];
                if (accordIndexes[accordIndex] != index)
                {
                    throw new InvalidOperationException(
                        $"The current accord should start at index {accordIndexes[accordIndex]} but is {index}.");
                }

                accordIndex++;
                byte value = accords[index++];

                // C2FORMAT.TXT line 252ff
                //5. Daten der Akkordinformationen
                //--------------------------------
                //
                //  +---+---+---+---+---+---+---+---+
                //  |5              |4  |3  |2  |1  |
                //  +---+---+---+---+---+---+---+---+
                //  (1) BIT      Extras
                //  (2) BIT      fester_Taktstrich_oder_Verschiebung
                //  (3) BIT      Moduswechsel
                //  (4) BIT      Triole_etc
                //  (5) BIT[4]   Anzahl_Noten
                bool hasExtras = (value & 0x01) > 0;
                bool hasMovement = (value & 0x02) > 0;
                bool hasModeChange = (value & 0x04) > 0;
                bool hasTriplet = (value & 0x08) > 0;
                int notes = (value & 0xF0) >> 4;

                if (hasModeChange)
                {
                    // C2FORMAT.TXT line 264ff
                    //  if Moduswechsel
                    //    +---+---+---+---+---+---+---+---+
                    //    |3      |2          |1          |
                    //    +---+---+---+---+---+---+---+---+
                    //    BIT[3] Schluesselform (0=G, 1=C, 2=F, 3=Schlagz. 4=kein, 5=unveraendert)
                    //    BIT[3] keyByte-Linie (0 = oberste Linie, ... , 4 = unterste Linie)
                    //    BIT[2] keyByte-Oktavierung (0=nach oben, 1=keine, 2= nach unten)
                    //    +---+---+---+---+---+---+---+---+
                    //    |2              |1              |
                    //    +---+---+---+---+---+---+---+---+
                    //    (1) BIT[4] Tonart (0=7b ... 7=c-Dur ... 14=7#, 15=unveraendert)
                    //    (2) BIT[4] Taktnenner (0=ganze, 1=halbe, ..., 6=1/64, 15=kein Takt)
                    //    BYTE   Taktzaehler  (255=unveraendert)
                    //  endif
                    byte mb1 = accords[index++];
                    int keyForm = mb1 & 0x07;
                    int keyLine = (mb1 & 0x38) >> 3;
                    int keyOctavation = (mb1 & 0xC0) >> 6;
                    if (keyForm != 5)
                    {
                        // key has changed ("5" is unchanged)
                        this.currentKeyForm[staffNumber] = keyForm;
                        this.currentKeyLine[staffNumber] = keyLine;
                        this.currentKeyOctavation[staffNumber] = keyOctavation;
                        // TODO Anzeigen
                    }

                    byte mb2 = accords[index++];
                    int signature = mb2 & 0x0F;
                    if (signature != 15)
                    {
                        // signature has changed ("15" is unchanged)
                        var currentSignature = this.GetCurrentSignature(staffNumber, voiceNumber);
                        if (currentSignature != signature)
                        {
                            Console.WriteLine($"changing signature from {currentSignature} to {signature}");
                            //this.currentSignature[staffNumber] = signature;
                            this.SetCurrentSignature(staffNumber, voiceNumber, signature);
                            currentMeasure.AddSignature(signature);
                        }
                    }

                    int nenner = (mb2 & 0xF0) >> 4;
                    byte zaehler = accords[index++];
                    if (zaehler != 255)
                    {
                        int newBeats = zaehler;
                        int newBeatType;
                        switch (nenner)
                        {
                            case 0:
                                newBeatType = 1;
                                break;
                            case 1:
                                newBeatType = 2;
                                break;
                            case 2:
                                newBeatType = 4;
                                break;
                            case 3:
                                newBeatType = 8;
                                break;
                            case 4:
                                newBeatType = 16;
                                break;
                            case 5:
                                newBeatType = 32;
                                break;
                            case 6:
                                newBeatType = 64;
                                break;
                            default:
                                throw new NotImplementedException();
                        }

                        // check if beat type is really changed
                        // capella is writing type with every system
                        if (this.currentBeats[staffNumber] != newBeats
                            || this.currentBeatType[staffNumber] != newBeatType)
                        {
                            this.currentBeats[staffNumber] = newBeats;
                            this.currentBeatType[staffNumber] = newBeatType;

                            attributes att = currentMeasure.GetAttributes();
                            var theTime = new time();
                            att.time = ArrayExtensions.ArrayAppend(att.time, theTime);
                            theTime.Items = ArrayExtensions.ArrayAppend(
                                theTime.Items,
                                this.currentBeats[staffNumber].ToString(CultureInfo.InvariantCulture));
                            theTime.ItemsElementName = ArrayExtensions.ArrayAppend(
                                theTime.ItemsElementName,
                                ItemsChoiceType10.beats);
                            theTime.Items = ArrayExtensions.ArrayAppend(theTime.Items, this.currentBeatType[staffNumber].ToString(CultureInfo.InvariantCulture));
                            theTime.ItemsElementName = ArrayExtensions.ArrayAppend(theTime.ItemsElementName, ItemsChoiceType10.beattype);
                        }
                    }
                }

                if (hasMovement)
                {
                    // C2FORMAT.TXT line 281ff
                    //  if fester_Taktstrich_oder_Verschiebung
                    //    +---+---+---+---+---+---+---+---+
                    //    |2              |1              |
                    //    +---+---+---+---+---+---+---+---+
                    //    BIT[4] fester_Taktstrich (0..6=einfach, doppelt, Schluss Wdh-Ende, 
                    //                              Wdh.Anf., Wdh-Ende/Anf)
                    //    BIT[4] Horizontalverschiebung
                    //  endif
                    byte move = accords[index++];
                }

                int tripletCounter = 0;
                bool tripartit;
                if (hasTriplet)
                {
                    // C2FORMAT.TXT line 290ff
                    //  if Triole_etc (Beispiele: Triole / Duole)
                    //    +---+---+---+---+---+---+---+---+
                    //    |3          |2  |1              |
                    //    +---+---+---+---+---+---+---+---+
                    //    (1) BIT[4] Zaehler           ( 3  /  2 )
                    //    (2) BIT[1] tripartit        ( 0  /  1 )
                    //    (3) Bit[3] reserviert (=0)
                    //  endif
                    byte triole = accords[index++];
                    tripletCounter = triole & 0x0F;
                    tripartit = (triole & 0x10) > 0;
                }

                if (hasExtras)
                {
                    // C2FORMAT.TXT line 299ff
                    //  if Extras
                    //    BYTE[3] reserviert
                    //    BYTE    Anzahl_GrafikObjekte
                    //    GRAFIKOBJEKT[Anzahl_GrafikObjekte]
                    //  endif
                    byte r1 = accords[index++];
                    byte r2 = accords[index++];
                    byte r3 = accords[index++];
                    byte graphObjectCount = accords[index++];

                    for (int i = 0; i < graphObjectCount; i++)
                    {
                        // C2FORMAT.TXT line 342ff
                        //GRAFIKOBJEKT
                        //  BYTE Typ
                        //  BYTE Laenge_des_Notenbereichs
                        //  case Typ
                        //    0: BINDEBOGEN     
                        //    1: N_OLENKLAMMER
                        //    2: DE_CRESCENDO   
                        //    3: NOTENLINIEN 
                        //    4: VOLTENKLAMMER
                        //    5: TRILLERSCHLANGE
                        //    6: TEXTOBJEKT
                        //    7: LINIE
                        //    8: RECHTECK
                        //    9: ELLIPSE
                        //  endcase
                        //end
                        byte type = accords[index++];
                        byte length = accords[index++];
                        switch (type)
                        {
                            case 0: // BINDEBOGEN
                                index += 16;
                                break;
                            case 1: // N_OLENKLAMMER
                                index += 10;
                                break;
                            case 2: // DE_CRESCENDO
                                index += 8;
                                break;
                            case 3: // NOTENLINIEN
                                index += 6;
                                break;
                            case 4: // VOLTENKLAMMER
                                index += 7;
                                break;
                            case 5: // TRILLERSCHLANGE
                                index += 7;
                                break;
                            case 6: // TEXTOBJEKT
                                int x = (byte)accords[index] + (byte)accords[index + 1] * 256;
                                index += 2;
                                int y = (byte)accords[index] + (byte)accords[index + 1] * 256;
                                index += 2;
                                byte fontInfo = accords[index++];
                                int fontIndex = fontInfo & 0x07;
                                if (fontIndex == 2)
                                {
                                    index += 56;
                                }

                                while (accords[index] != 0)
                                {
                                    index++;
                                }

                                // terminator
                                index++;
                                break;
                            case 7: // LINIE
                                index += 10;
                                break;
                            case 8: // RECHTECK
                                index += 10;
                                break;
                            case 9: // ELLIPSE
                                index += 10;
                                break;
                        }
                    }
                }

                // C2FORMAT.TXT line 305ff
                //  +---+---+---+---+---+---+---+---+
                //  |4  |3  |2      |1              |
                //  +---+---+---+---+---+---+---+---+
                //  (1) BIT[4] Notenwert
                //  (2) BIT[2] Punktierung (0..3 = ohne, einfach doppelt, Zeilenende)
                //  (3) BIT    halbe_Groesse
                //  (4) BIT    ohne_Wert
                //  +---+---+---+---+---+---+---+---+
                //  |6          |5  |4  |3  |2  |1  |              |
                //  +---+---+---+---+---+---+---+---+
                //  (1) BIT    Balken trennen
                //  (2) BIT    Balken verbinden
                //  (3) BIT    unsichtbar
                //  (4) BIT    Haltebogen
                //  (5) BIT    expl_Vorzeichen
                //  (6) BIT[3] Kopfform
                //  +---+---+---+---+---+---+---+---+
                //  |4  |3  |2      |1              |
                //  +---+---+---+---+---+---+---+---+
                //  (1) BIT[4] Artikulationszeichen
                //  (2) BIT[2] Hals
                //  (3) BIT[1] Pause auch in zweistimmiger Zeile zentriert (ab V2.1)
                //  (4) BIT[1] Atemzeichen (neu in V2.2)
                //  NOTE[Anzahl_Noten]
                byte b1 = accords[index++];
                int noteDuration = b1 & 0x0F;
                int notePunctation = (b1 & 0x30) >> 4;
                bool noteHalfSize = (b1 & 0x40) > 0;
                bool noteNoValue = (b1 & 0x80) > 0;

                byte b2 = accords[index++];
                bool splitBar = (b2 & 0x01) > 0;
                bool combineBar = (b2 & 0x02) > 0;
                bool invisible = (b2 & 0x04) > 0;
                bool connectNext = (b2 & 0x08) > 0;
                bool showSign = (b2 & 0x10) > 0;
                int headFormat = (b2 & 0xE0) >> 5;

                byte b3 = accords[index++];

                if (noteNoValue && invisible)
                {
                    // current note is just a placeholder
                    // skip and continue
                    for (int i = 0; i < notes; i++)
                    {
                        index++;
                    }

                    continue;
                }

                if (notePunctation == 3)
                {
                    // "3" in notePuncation marks end of line
                    // just leave accords loop
                    if (index != accords.Length)
                    {
                        throw new InvalidOperationException(
                            "Current member identifies end of line, but there are pending elements.");
                    }

                    break;
                }

                var noteType = new notetype();
                int noteTicks;
                switch (noteDuration)
                {
                    case 0:
                        noteType.Value = notetypevalue.whole;
                        noteTicks = 128;
                        if (notes == 0)
                        {
                            // whole rest -> whole measure
                            noteTicks = this.currentBeats[staffNumber] * (128 / this.currentBeatType[staffNumber]);
                        }

                        break;
                    case 1:
                        noteType.Value = notetypevalue.half;
                        noteTicks = 64;
                        break;
                    case 2:
                        noteType.Value = notetypevalue.quarter;
                        noteTicks = 32;
                        break;
                    case 3:
                        noteType.Value = notetypevalue.eighth;
                        noteTicks = 16;
                        break;
                    case 4:
                        noteType.Value = notetypevalue.Item16th;
                        noteTicks = 8;
                        break;
                    case 5:
                        noteType.Value = notetypevalue.Item32nd;
                        noteTicks = 4;
                        break;
                    case 6:
                        noteType.Value = notetypevalue.Item64th;
                        noteTicks = 2;
                        break;
                    case 7:
                        noteType.Value = notetypevalue.Item128th;
                        noteTicks = 1;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                emptyplacement[] dots = null;
                if (notePunctation == 1)
                {
                    noteTicks = (noteTicks * 3) / 2;
                    dots = new[] { new emptyplacement() };
                }
                else if (notePunctation == 2)
                {
                    noteTicks = (noteTicks * 5) / 3;
                    dots = new[] { new emptyplacement(), new emptyplacement() };
                }

                if (notes == 0)
                {
                    // rest
                    var newNote = new note();
                    var theRest = newNote.AddRest();

                    // the rest sign will be painted on step A by default in octave 4
                    theRest.displaystep = step.B;
                    int octave = 4;
                    if (voiceNumber == 1)
                    {
                        theRest.displaystep = step.F;
                        octave = 5;
                    }
                    else if (voiceNumber == 2)
                    {
                        theRest.displaystep = step.E;
                    }

                    if (this.currentKeyOctavation[staffNumber] == 0)
                    {
                        // in case of upper octavation we have to increase octave number
                        octave++;
                    }
                    else if (this.currentKeyOctavation[staffNumber] == 2)
                    {
                        // in case of lower octavation we have to decrease octave number
                        octave--;
                    }

                    // in case of F clef we use different octave and step for rest sign
                    if (this.currentKeyForm[staffNumber] == 2)
                    {
                        octave--;
                        theRest.displaystep = step.D;
                        if (voiceNumber == 1)
                        {
                            theRest.displaystep = step.A;
                        }
                        else if (voiceNumber == 2)
                        {
                            theRest.displaystep = step.G;
                            octave--;
                        }
                    }

                    theRest.displayoctave = octave.ToString(CultureInfo.InvariantCulture);

                    newNote.AddDuration(noteTicks);

                    if (noteDuration > 0)
                    {
                        newNote.type = noteType;
                    }

                    newNote.dot = dots;

                    if (voiceNumber > 0)
                    {
                        newNote.voice = voiceNumber.ToString(CultureInfo.InvariantCulture);
                    }

                    if (invisible)
                    {
                        // TODO printobject is yet generated from xsd2code
                        // newNote.printobject = no;
                    }

                    currentMeasure.AddNote(newNote);
                }

                bool isSyllabic = false;
                for (int i = 0; i < notes; i++)
                {
                    // C2FORMAT.TXT line 334ff
                    //NOTE
                    //  +---+---+---+---+---+---+---+---+
                    //  |2      |1                      |
                    //  +---+---+---+---+---+---+---+---+
                    //  (1) BIT[6] relative diatonische Hoehe + 32
                    //  (2) BIT[2] Alteration + 2
                    //end
                    byte noteValue = accords[index++];
                    int relativePitch = (noteValue & 0x3F) - 32;
                    int alteration = ((noteValue & 0xC0) >> 6) - 2;

                    var newNote = new note();

                    if (i > 0)
                    {
                        newNote.SetChord();
                    }

                    var p = pitch.CreatePitch(
                        relativePitch,
                        alteration,
                        this.currentKeyForm[staffNumber],
                        this.currentKeyOctavation[staffNumber],
                        this.GetCurrentSignature(staffNumber, voiceNumber));
                    newNote.AddPitch(p);

                    newNote.type = noteType;
                    newNote.dot = dots;

                    if (voiceNumber > 0)
                    {
                        newNote.voice = voiceNumber.ToString(CultureInfo.InvariantCulture);
                    }

                    // TRIPLETS
                    if (hasTriplet && tripletCounter > 0)
                    {
                        newNote.timemodification = new timemodification
                        {
                            actualnotes = tripletCounter.ToString(CultureInfo.InvariantCulture),
                            normalnotes = "2"
                        };
                        // TODO How to remove normal-type?
                        // TODO : 
                        //newNote.notations = new notations[1]
                        //{
                        //    new notations { Items = new object[1] 
                        //    {
                        //        new tuplet { type = startstop.start } }
                        //    }
                        //};

                        double accurateNoteTicks = noteTicks * 2;
                        accurateNoteTicks /= tripletCounter;
                        noteTicks = (int)accurateNoteTicks;
                        tripletRests += accurateNoteTicks - noteTicks;
                        if (tripletRests > 0.5)
                        {
                            noteTicks += 1;
                            tripletRests -= 1;
                        }
                    }

                    // the duration values corresponds with the divisions value set in readLAYOUT()
                    newNote.AddDuration(noteTicks);

                    if (connectNext && this.notesConnecting[staffNumber] == false)
                    {
                        newNote.AddTieStart();
                    }
                    else if (connectNext == false && this.notesConnecting[staffNumber])
                    {
                        newNote.AddTieStop();
                    }

                    this.notesConnecting[staffNumber] = connectNext;

                    if (lyricElemets.Length > currentLyricIndex)
                    {
                        string newText = lyricElemets[currentLyricIndex];
                        newText = newText.Replace("$", string.Empty);
                        newText = newText.Replace("#", "-");
                        var lyric = newNote.AddLyric(newText, ref isSyllabic);

                        currentLyricIndex++;
                    }

                    currentMeasure.AddNote(newNote);
                }

                currentMeasureTicks += noteTicks;
                if (currentMeasureTicks >= this.currentBeats[staffNumber] * (128 / this.currentBeatType[staffNumber]))
                {
                    currentMeasure.AddBackup(currentMeasureTicks);

                    measureIndex++;
                    if (measureIndex >= this.measures[staffNumber].Count)
                    {
                        var newMeasure = new scorepartwisePartMeasure();
                        this.measures[staffNumber].Add(newMeasure);
                        newMeasure.number = (measureIndex + 1).ToString(CultureInfo.InvariantCulture);
                    }

                    currentMeasureTicks = 0;
                }
            }
        }
    }
}
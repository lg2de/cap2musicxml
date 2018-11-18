// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.capella
{
    using System.Globalization;
    using System.Text;
    using musicxml;

    /// <summary>
    /// This class implements reading files in format of CAPELLA 2.x.
    /// The implementation is based on the file C2FORMAT.TXT provided by WHC many years ago.
    /// </summary>
    public partial class CapellaV2
    {
        // C2FORMAT.TXT line 43ff
        //3. SCORE
        //--------
        //
        //SCORE (capella 2.0/2.1/2.2 - Partitur)
        //  LONG              Dateikennung; moegliche Werte:
        //                      0x74B3E289 = V2.0, 
        //                      0x74B3E28A = V2.1 (oder V2.2 ohne Taktnumerierung)
        //                      0x74B3E28B = V2.2 (mit Taktnumerierung)
        //  PRINTSETTINGS     Druckereinstellungen
        //  UINT              Druckgroesse (7200 * Abstand zweier Notenlinien / Zoll)
        //  BYTE              Druckbilddarstellung
        //  BYTE              Farbdarstellung
        //  FONT              Standard-Text-Font
        //  if Dateikennung >= V2.1
        //    INT             Anfang fuer Seitenzaehlung
        //  endif
        //  if Dateikennung = V2.2 (Taktnumerierung)
        //    BYTE            umrahmte Taktnummern 
        //    BYTE            Abstand der Taktnummern vom linken Rand
        //    BYTE            Abstand der Taktnummern ueber dem System
        //    FONT            Font fuer Taktnummern
        //  endif
        //  TEXTZEILE         Kopfzeile
        //  TEXTZEILE         Fusszeile
        //  LAYOUT            Partiturformat
        //  INT               nSystems
        //  SYSTEM[nSystems]  Beschreibung der Systeme
        //  INT               nMetafiles
        //  METAFILE[nMetafiles]
        //end

        /// <summary>
        /// This method implements reading the capella file from a byte buffer.
        /// </summary>
        /// <param name="buffer">This parameter is the byte buffer.</param>
        /// <returns>The method returns true if reading was successful, otherwise false.</returns>
        public bool ReadCapella(byte[] buffer)
        {
            uint position = 0;
            Read(buffer, ref position, out LONG signature);
            this.version = FileVersion.Undefined;
            if (signature == 0x74B3E289)
            {
                this.version = FileVersion.V20;
            }
            else if (signature == 0x74B3E28A)
            {
                this.version = FileVersion.V21;
            }
            else if (signature == 0x74B3E28B)
            {
                this.version = FileVersion.V22;
            }
            else
            {
                throw new FileFormatException(position);
            }

            this.ReadPRINTSETTINGS(buffer, ref position);
            Read(buffer, ref position, out UINT printSize);
            Read(buffer, ref position, out BYTE printVisualization);
            Read(buffer, ref position, out BYTE colorType);
            this.ReadFONT(buffer, ref position);

            if (this.version >= FileVersion.V21)
            {
                Read(buffer, ref position, out INT firstPage);
            }

            if (this.version == FileVersion.V22)
            {
                Read(buffer, ref position, out BYTE takt1);
                Read(buffer, ref position, out BYTE takt2);
                Read(buffer, ref position, out BYTE takt3);
                this.ReadFONT(buffer, ref position);
            }

            this.ReadTEXTZEILE(buffer, ref position, out this.headLineText);
            this.ReadTEXTZEILE(buffer, ref position, out this.footLineText);
            this.ReadLAYOUT(buffer, ref position);

            Read(buffer, ref position, out INT systemCount);
            for (int i = 0; i < systemCount; i++)
            {
                this.ReadSYSTEM(buffer, ref position);
            }

            Read(buffer, ref position, out INT metafileCount);
            for (int i = 0; i < metafileCount; i++)
            {
                this.ReadMETAFILE(buffer, ref position);
            }

            for (int i = 0; i < this.measures.Length; i++)
            {
                this.Document.part[i].measure = this.measures[i].ToArray();
            }

            return buffer.Length == position;
        }

        // C2FORMAT.TXT line 75ff
        //4. Hilfsformate (alphabetisch)
        //------------------------------
        //
        //FONT (56 Bytes) 
        //  UINT    nScale
        //  LOGFONT logfont
        //  INT     nHeight
        //  UINT    reserviert
        //end
        private void ReadFONT(byte[] buffer, ref uint position)
        {
            Read(buffer, ref position, out UINT scale);
            this.ReadLOGFONT(buffer, ref position);
            Read(buffer, ref position, out INT height);
            Read(buffer, ref position, out UINT reserved);
        }

        // C2FORMAT.TXT line 85ff
        //LAYOUT (Beschreibung der Partitur-Formatvorlage)
        //  if Dateikennung >= V2.1
        //    BYTE    topDist
        //    CHAR    Ersatzzeichen fuer geschuetztes Leerzeichen ('\0' --> '$')
        //    BYTE    interDist
        //    CHAR    Ersatzzeichen fuer geschuetzten Bindestrich ('\0' --> '#')
        //  else
        //    INT     topDist
        //    INT     interDist
        //  endif
        //  BYTE    beamMode
        //  FONT    txtFont
        //  BYTE    txtAlign
        //  BOOL    allaBreve
        //  UINT    tempo
        //  INT     staves
        //  if Dateikennung >= V2.1
        //    BYTE[16] nSound  (Klang fuer Kanal 1 bis 16
        //                      im 1. Byte ist hoechstes Bit gesetzt!)
        //    BYTE[16] nVolume (Lautstaerke fuer Kanal 1 bis 16)
        //  else (Version 2.0)
        //    BYTE[9] nSound   (Klang fuer Kanal 1 bis 9)
        //    BYTE[9] nVolume  (Lautstaerke fuer Kanal 1 bis 9)
        //  endif
        //  STAFF_LAYOUT[staves]
        //end
        private void ReadLAYOUT(byte[] buffer, ref uint position)
        {
            if (this.version >= FileVersion.V21)
            {
                Read(buffer, ref position, out BYTE topDist);
                Read(buffer, ref position, out CHAR spaceReplacement);
                Read(buffer, ref position, out BYTE interDist);
                Read(buffer, ref position, out CHAR dashReplacement);
            }
            else
            {
                Read(buffer, ref position, out INT topDist);
                Read(buffer, ref position, out INT interDist);
            }

            Read(buffer, ref position, out BYTE beamMode);
            this.ReadFONT(buffer, ref position);
            Read(buffer, ref position, out BYTE txtAlign);
            Read(buffer, ref position, out BOOL allaBreve);
            Read(buffer, ref position, out UINT tempo);
            Read(buffer, ref position, out INT staveCount);
            if (this.version >= FileVersion.V21)
            {
                var soundValues = new BYTE[16];
                var volumeValues = new BYTE[16];
                for (int i = 0; i < 16; i++)
                {
                    Read(buffer, ref position, out soundValues[i]);
                }

                for (int i = 0; i < 16; i++)
                {
                    Read(buffer, ref position, out volumeValues[i]);
                }
            }
            else
            {
                var soundValues = new BYTE[9];
                var volumeValues = new BYTE[9];
                for (int i = 0; i < 9; i++)
                {
                    Read(buffer, ref position, out soundValues[i]);
                }

                for (int i = 0; i < 9; i++)
                {
                    Read(buffer, ref position, out volumeValues[i]);
                }
            }

            // TODO hier hatte ich zuerst ein partlist-Array. R# hat eine Co-Varianz-Warnung angezeigt. Daher nun object-Array.
            this.Document.partlist = new partlist { Items = new object[staveCount] };
            this.Document.part = new scorepartwisePart[staveCount];

            this.InitStaves(staveCount);

            for (int i = 0; i < staveCount; i++)
            {
                // initialite partDescription structures for current staff
                var partDescription = new scorepart();
                this.Document.partlist.Items[i] = partDescription;
                partDescription.id = "P" + (i + 1).ToString(CultureInfo.InvariantCulture);
                var partData = new scorepartwisePart { id = partDescription.id };
                this.Document.part[i] = partData;

                var firstMeasure = new scorepartwisePartMeasure();
                this.measures[i].Add(firstMeasure);
                firstMeasure.number = "1";

                attributes att = firstMeasure.GetAttributes();

                // 32 corresponds with duration values in readSTAFF()
                att.divisions = 32;
                att.divisionsSpecified = true;

                this.ReadSTAFFLAYOUT(buffer, ref position, i, partDescription);

                firstMeasure.AddClef(
                    this.currentKeyForm[i],
                    this.currentKeyLine[i],
                    this.currentKeyOctavation[i]);
            }
        }

        // C2FORMAT.TXT line 112
        //LIEDTEXT
        //  INT         Liedtext_Laenge
        //  if Liedtext_Laenge > 0
        //    CHAR[n+1] Liedtext
        //    FONT      (56 Bytes) Liedtext_Font
        //    INT       Abstand_von_Notenzeile
        //    INT       Abstand_zwischen_Strophen
        //  endif
        //end
        private void ReadLIEDTEXT(byte[] buffer, ref uint position, out string result)
        {
            result = string.Empty;

            Read(buffer, ref position, out INT stringLength);
            if (stringLength == 0)
            {
                return;
            }

            if (position + stringLength > buffer.Length)
            {
                throw new FileFormatException(position);
            }

            var enc = Encoding.GetEncoding("iso8859-1");
            result = enc.GetString(buffer, (int)position, stringLength);
            position += (uint)(int)stringLength + 1;

            this.ReadFONT(buffer, ref position);
            Read(buffer, ref position, out INT margin);
            Read(buffer, ref position, out INT distance);
        }

        // C2FORMAT.TXT line 122ff
        //LOGFONT (Windows-Struktur!)
        //  INT     lfHeight;
        //  INT     lfWidth;
        //  INT     lfEscapement;
        //  INT     lfOrientation;
        //  INT     lfWeight;
        //  BYTE    lfItalic;
        //  BYTE    lfUnderline;
        //  BYTE    lfStrikeOut;
        //  BYTE    lfCharSet;
        //  BYTE    lfOutPrecision;
        //  BYTE    lfClipPrecision;
        //  BYTE    lfQuality;
        //  BYTE    lfPitchAndFamily;
        //  CHAR    lfFaceName[32];
        //end
        private void ReadLOGFONT(byte[] buffer, ref uint position)
        {
            Read(buffer, ref position, out INT height);
            Read(buffer, ref position, out INT width);
            Read(buffer, ref position, out INT escapement);
            Read(buffer, ref position, out INT orientation);
            Read(buffer, ref position, out INT weight);
            Read(buffer, ref position, out BYTE italic);
            Read(buffer, ref position, out BYTE underline);
            Read(buffer, ref position, out BYTE strikeOut);
            Read(buffer, ref position, out BYTE charSet);
            Read(buffer, ref position, out BYTE outPrecision);
            Read(buffer, ref position, out BYTE clipPrecision);
            Read(buffer, ref position, out BYTE quality);
            Read(buffer, ref position, out BYTE pitchAndFamily);
            var faceName = new CHAR[32];
            for (int i = 0; i < 32; i++)
            {
                Read(buffer, ref position, out faceName[i]);
            }
        }

        // C2FORMAT.TXT line 139ff
        //METAFILE (importierte WMF-Grafik)
        //  INT           Type
        //  LONG[4]       left, top, right, bottom
        //  LONG          xProp (Soll-Seitenverhaeltnis)
        //  LONG          yProp
        //  INT[4]        reserviert
        //  BOOL          fMF (in capella 2.0 immer TRUE)
        //  if fMF
        //    METAFILEPICT mf
        //    DWORD size
        //    CHAR[size] Daten
        //  endif
        //end
        private void ReadMETAFILE(byte[] buffer, ref uint position)
        {
            Read(buffer, ref position, out INT type);
            var margin = new LONG[4];
            for (int i = 0; i < 4; i++)
            {
                Read(buffer, ref position, out margin[i]);
            }
            Read(buffer, ref position, out LONG propX);
            Read(buffer, ref position, out LONG propY);

            var reserviert = new INT[4];
            for (int i = 0; i < 4; i++)
            {
                Read(buffer, ref position, out reserviert[i]);
            }

            Read(buffer, ref position, out BOOL fMF);

            if (fMF)
            {
                this.ReadMETAFILEPICT(buffer, ref position);
                Read(buffer, ref position, out DWORD size);
                for (int i = 0; i < size; i++)
                {
                    Read(buffer, ref position, out CHAR datenDummy);
                }
            }
        }

        // C2FORMAT.TXT line 153ff
        //METAFILEPICT (Windows-Struktur!)
        //  INT   mm
        //  INT   xExt
        //  INT   yExt
        //  UINT  hMF
        //end
        private void ReadMETAFILEPICT(byte[] buffer, ref uint position)
        {
            Read(buffer, ref position, out INT mm);
            Read(buffer, ref position, out INT extensionX);
            Read(buffer, ref position, out INT extensionY);
            Read(buffer, ref position, out UINT hMF);
        }

        // C2FORMAT.TXT line 160ff
        //PRINTSETTINGS (Teil der Windows-Struktur DEVMODE)
        //  DWORD dmFields;
        //  INT   dmOrientation;
        //  INT   dmPaperSize;
        //  INT   dmPaperLength;
        //  INT   dmPaperWidth;
        //  INT   dmScale;
        //  INT   dmCopies;
        //  INT   dmDefaultSource;
        //  INT   dmPrintQuality;
        //  INT   dmColor;
        //  INT   dmDuplex;
        //  INT   dmYResolution;
        //  INT   dmTTOption;
        //  INT   oberer Seitenrand
        //  INT   unterer Seitenrand
        //  INT   linker Seitenrand
        //  INT   rechter Seitenrand
        //end
        private void ReadPRINTSETTINGS(byte[] buffer, ref uint position)
        {
            Read(buffer, ref position, out DWORD fields);
            Read(buffer, ref position, out INT orientation);
            Read(buffer, ref position, out INT paperSize);
            Read(buffer, ref position, out INT paperLength);
            Read(buffer, ref position, out INT paperWidth);
            Read(buffer, ref position, out INT scale);
            Read(buffer, ref position, out INT copies);
            Read(buffer, ref position, out INT defaultSource);
            Read(buffer, ref position, out INT printQuality);
            Read(buffer, ref position, out INT color);
            Read(buffer, ref position, out INT duplex);
            Read(buffer, ref position, out INT resolutionY);
            Read(buffer, ref position, out INT option);
            Read(buffer, ref position, out INT topMargin);
            Read(buffer, ref position, out INT bottomBargin);
            Read(buffer, ref position, out INT leftMargin);
            Read(buffer, ref position, out INT rightMargin);
        }

        // C2FORMAT.TXT line 180ff
        //STAFF (Notenzeile)
        //  LIEDTEXT
        //  BOOL        fActiveVoice
        //  INT         Stimme  (0 = Einzelzeile, 1 = erste Stimme, 2 = zweite Stimme)
        //  INT         zusaetzlicher Abstand nach oben (in Notenlinien)
        //  INT         zusaetzlicher Abstand nach unten (in Notenlinien)
        //  BYTE        Layout-Zeilennummer
        //  UINT        countAccords
        //  CHAR[countAccords]    Akkordinformationen (siehe Abschnitt 5)
        //  INT         n (Anzahl Akkorde+1)
        //  INT[n+1]    Akkordindizes (Indizes der Akkordbeschreibungen in den Daten.
        //              Beispiel: Akkordindizes[2] = 17 bedeutet, dass die Beschreibung 
        //              des 3. Akkords (Zaehlung ab 0) bei Byte 17 der Akkordinformationen
        //              beginnt.
        //end
        private void ReadSTAFF(byte[] buffer, ref uint position, int measureIndex)
        {
            // read complete STAFF data
            this.ReadLIEDTEXT(buffer, ref position, out string text);
            Read(buffer, ref position, out BOOL activeVoice);
            Read(buffer, ref position, out INT voiceNumber);
            Read(buffer, ref position, out INT topMargin);
            Read(buffer, ref position, out INT bottomMargin);
            Read(buffer, ref position, out BYTE staffNumber);
            Read(buffer, ref position, out UINT countAccords);
            var accords = new BYTE[countAccords];
            for (uint i = 0; i < countAccords; i++)
            {
                Read(buffer, ref position, out accords[i]);
            }

            Read(buffer, ref position, out INT n);
            var indexes = new INT[n + 1];
            for (uint i = 0; i < n + 1; i++)
            {
                Read(buffer, ref position, out indexes[i]);
            }

            // read completed

            // process read data
            this.ProcessAccords(staffNumber, measureIndex, voiceNumber, accords, indexes, text);
        }

        // C2FORMAT.TXT line 197ff
        //STAFF_LAYOUT (Format einer Notenzeile)
        //  INT     Abstand nach oben
        //  INT     Abstand nach unten
        //  BYTE    keyByte
        //  BOOL    fSingleLine
        //  BOOL    irrelevant
        //  INT     baseSound
        //  INT     extSound
        //  INT     transp
        //  CHAR[3] brackets
        //  STRINGZ Beschreibung
        //  STRINGZ Bezeichnung
        //  STRINGZ Abkuerzung
        //end
        private void ReadSTAFFLAYOUT(byte[] buffer, ref uint position, int staffNumber, scorepart partDescription)
        {
            Read(buffer, ref position, out INT topMargin);
            Read(buffer, ref position, out INT bottomMargin);
            Read(buffer, ref position, out BYTE keyByte);

            // structure not explicitely described here
            // taken from section 5:
            //    +---+---+---+---+---+---+---+---+
            //    |3      |2          |1          |
            //    +---+---+---+---+---+---+---+---+
            //    BIT[3] Schluesselform (0=G, 1=C, 2=F, 3=Schlagz. 4=kein, 5=unveraendert)
            //    BIT[3] keyByte-Linie (0 = oberste Linie, ... , 4 = unterste Linie)
            //    BIT[2] keyByte-Oktavierung (0=nach oben, 1=keine, 2= nach unten)
            this.currentKeyForm[staffNumber] = keyByte & 0x07;
            this.currentKeyLine[staffNumber] = (keyByte & 0x38) >> 3;
            this.currentKeyOctavation[staffNumber] = (keyByte & 0xC0) >> 6;
            Read(buffer, ref position, out BOOL singleLine);
            Read(buffer, ref position, out BOOL irrelevant);
            Read(buffer, ref position, out INT baseSound);
            Read(buffer, ref position, out INT extSound);
            Read(buffer, ref position, out INT transp);
            var brackets = new CHAR[3];
            Read(buffer, ref position, out brackets[0]);
            Read(buffer, ref position, out brackets[1]);
            Read(buffer, ref position, out brackets[2]);

            // TODO kein DUMMY!
            this.ReadSTRINGZ(buffer, ref position, out string dummy);
            this.ReadSTRINGZ(buffer, ref position, out dummy);
            partDescription.partname = new partname { Value = dummy };
            this.ReadSTRINGZ(buffer, ref position, out dummy);
            partDescription.partabbreviation = new partname { Value = dummy };
        }

        // C2FORMAT.TXT line 212ff
        //STRINGZ
        //  INT len
        //  CHAR[len+1] Text (Null-terminierter String)
        //end
        private void ReadSTRINGZ(byte[] buffer, ref uint position, out string result)
        {
            Read(buffer, ref position, out INT stringLength);

            if (position + stringLength > buffer.Length)
            {
                throw new FileFormatException(position);
            }

            var enc = Encoding.GetEncoding("iso8859-1");
            int n = stringLength;
            result = enc.GetString(buffer, (int)position, n);
            position += (uint)(int)stringLength + 1;
        }

        // C2FORMAT.TXT line 217ff
        //SYSTEM (Notensystem)
        //  +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
        //  |5                              |4                  |3  |2  |1  |
        //  +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
        //  (1) BIT     fJustification
        //  (2) BIT     fShowMeter
        //  (3) BIT     fFullName
        //  (4) BIT[5]  n1 (Korrektur der Taktzaehlung Teil 1, ab V2.2)
        //  (5) BYTE    nIndent
        //
        //  +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
        //  |4  |3              |2  |1                                      |
        //  +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
        //  (1) BIT[10] staves
        //  (2) BIT     Taktnumerierung auf 1 zuruecksetzen (ab V2.2)
        //  (3) BIT[4]  n2 (Korrektur der Taktzaehlung Teil 2, ab V2.2)
        //              n = n1 + 32*n2
        //              if n > 500 then Korrektur := 500-n else Korrektur := n
        //  (4) BIT     fFormFeed (ab V2.2)
        //
        //  STAFF[staves] (Beschreibung der Notenzeilen)
        //end
        private void ReadSYSTEM(byte[] buffer, ref uint position)
        {
            Read(buffer, ref position, out UINT set1);
            Read(buffer, ref position, out UINT set2);
            var staffs = (uint)(set2 & 0x3ff);

            int measureIndex = this.measures[0].Count - 1;

            for (int i = 0; i < staffs; i++)
            {
                this.ReadSTAFF(buffer, ref position, measureIndex);
            }

            int maxMeasures = 0;
            for (int i = 0; i < this.staffCount; i++)
            {
                if (maxMeasures < this.measures[i].Count)
                {
                    maxMeasures = this.measures[i].Count;
                }
            }

            for (int i = 0; i < this.staffCount; i++)
            {
                for (int j = this.measures[i].Count; j < maxMeasures; j++)
                {
                    var newMeasure = new scorepartwisePartMeasure();
                    this.measures[i].Add(newMeasure);
                    newMeasure.number = (j + 1).ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        // C2FORMAT.TXT line 240ff
        //TEXTZEILE (fuer Kopf-/Fusszeile)
        //  INT             Laenge
        //  if Laenge > 0
        //    CHAR[Laenge+1] Zeile  (Null-terminierter String)
        //    INT           Abstand
        //    FONT          Font
        //    BYTE          Ausrichtung
        //    BYTE          auch auf 1. Seite
        //  endif
        //end
        private void ReadTEXTZEILE(byte[] buffer, ref uint position, out string result)
        {
            Read(buffer, ref position, out INT stringLength);

            result = string.Empty;
            if (stringLength <= 0)
            {
                return;
            }

            if (position + stringLength > buffer.Length)
            {
                throw new FileFormatException(position);
            }

            var enc = Encoding.GetEncoding("iso8859-1");
            result = enc.GetString(buffer, (int)position, stringLength);
            position += (uint)(int)stringLength + 1;

            Read(buffer, ref position, out INT margin);
            this.ReadFONT(buffer, ref position);
            Read(buffer, ref position, out BYTE alignment);
            Read(buffer, ref position, out BYTE firstPage);
        }
    }
}

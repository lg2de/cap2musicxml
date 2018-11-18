// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.capella
{
    using System;
    using System.Collections.Generic;
    using musicxml;

    /// <content>
    /// This part implements some member functionality.
    /// </content>
    public partial class CapellaV2
    {
        private FileVersion version = FileVersion.Undefined;

        private int staffCount;
        private List<scorepartwisePartMeasure>[] measures;
        private int[] currentKeyForm, currentKeyLine, currentKeyOctavation;
        private Dictionary<int, int> currentSignatures = new Dictionary<int, int>();
        private int[] currentBeats, currentBeatType;

        private bool[] notesConnecting;

        private string headLineText, footLineText;

        private enum FileVersion
        {
            Undefined,
            V20,
            V21,
            V22
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public scorepartwise Document { get; } = new scorepartwise();

        private void InitStaves(int staves)
        {
            this.staffCount = staves;
            this.measures = new List<scorepartwisePartMeasure>[staves];
            for (int i = 0; i < staves; i++)
            {
                this.measures[i] = new List<scorepartwisePartMeasure>();
            }

            this.currentKeyForm = new int[staves];
            this.currentKeyLine = new int[staves];
            this.currentKeyOctavation = new int[staves];
            //this.currentSignature = new int[staves];
            this.currentBeats = new int[staves];
            this.currentBeatType = new int[staves];
            this.notesConnecting = new bool[staves];

            // set default values
            for (int i = 0; i < staves; i++)
            {
                this.SetCurrentSignature(i, 0, 7); // C-Dur
                //this.currentSignature[i] = 7; // C-Dur
                this.currentBeats[i] = this.currentBeatType[i] = 4; // 4/4
            }
        }

        private int GetCurrentSignature(int staffNumber, int voiceNumber)
        {
            var key = staffNumber * 10 + voiceNumber;
            if (this.currentSignatures.TryGetValue(key, out int result))
            {
                return result;
            }

            key = staffNumber * 10;
            if (this.currentSignatures.TryGetValue(key, out result))
            {
                return result;
            }

            throw new InvalidOperationException("The signature was not defined.");
        }

        private void SetCurrentSignature(int staffNumber, int voiceNumber, int signature)
        {
            var key = staffNumber * 10 + voiceNumber;
            this.currentSignatures[key] = signature;

            if (voiceNumber == 0)
            {
                // done
                return;
            }

            var singleVoiceKey = staffNumber * 10;

            // set other voice if not yet set
            key = staffNumber * 10 + (3 - voiceNumber);
            if (!this.currentSignatures.ContainsKey(key))
            {
                this.currentSignatures[key] = this.currentSignatures[singleVoiceKey];
            }

            // change single voice signature, too
            this.currentSignatures[singleVoiceKey] = signature;
        }
    }
}

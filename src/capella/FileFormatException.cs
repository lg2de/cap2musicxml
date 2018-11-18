// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.capella
{
    using System;

    /// <summary>
    /// This class is raised from <see cref="CapellaV2.ReadCapella"/> in case a file inconsistency was detected.
    /// </summary>
    public class FileFormatException : Exception
    {
        internal FileFormatException(uint failedPosition)
        {
            this.Message = string.Format("File insonsistent at position {0}", failedPosition);
        }

        /// <summary>
        /// Gets the Exception message.
        /// </summary>
        public new string Message { get; private set; }
    }
}

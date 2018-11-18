// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml
{
    using System;
    using System.IO;
    using lg2de.cap2musicxml.capella;

    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                return 1;
            }

            string fileNameIn = args[0];
            string fileNameOut = fileNameIn + ".xml";
            if (args.Length >= 2)
            {
                fileNameOut = args[1];
            }

            Console.WriteLine($"Reading from {fileNameIn}");

            var reader = new FileStream(fileNameIn, FileMode.Open);
            var bytes = new byte[reader.Length];
            var numBytesToRead = (int)reader.Length;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                int n = reader.Read(bytes, numBytesRead, numBytesToRead);

                // Break when the end of the file is reached.
                if (n == 0)
                {
                    break;
                }

                numBytesRead += n;
                numBytesToRead -= n;
            }

            var c = new CapellaV2();
            if (!c.ReadCapella(bytes))
            {
                Console.WriteLine("Failed to read file.");
                return 1;
            }

            Console.WriteLine($"Writing to {fileNameOut}");
            c.Document.SaveToFile(fileNameOut);
            return 0;
        }
    }
}

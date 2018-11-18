// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.musicxml
{
    using System;

    internal class ArrayExtensions
    {
        public static T[] ArrayResize<T>(T[] array, int newSize)
        {
            if (newSize < 0)
            {
                throw new ArgumentOutOfRangeException("newSize");
            }

            T[] sourceArray = array;
            if (sourceArray == null)
            {
                return new T[newSize];
            }

            if (sourceArray.Length != newSize)
            {
                var destinationArray = new T[newSize];
                for (int i = 0; i < sourceArray.Length && i < newSize; i++)
                {
                    destinationArray[i] = sourceArray[i];
                }

                return destinationArray;
            }

            return array;
        }

        public static T[] ArrayAppend<T>(T[] array, T item)
        {
            T[] newArray = array == null ? ArrayResize<T>(null, 1) : ArrayResize(array, array.Length + 1);

            newArray[newArray.Length - 1] = item;
            return newArray;
        }
    }
}

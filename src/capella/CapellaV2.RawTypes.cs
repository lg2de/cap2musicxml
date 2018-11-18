// <copyright>
//     Copyright (c) Lukas Grützmacher. All rights reserved.
// </copyright>

namespace lg2de.cap2musicxml.capella
{
    using System;
    using System.Diagnostics;
    using System.Text;

    /// <content>
    /// This part implements the raw types used internally for processing capella files.
    /// </content>
    public partial class CapellaV2
    {
        private static void Read<T>(byte[] buffer, ref uint position, out T value) where T : BaseType, new()
        {
            value = new T();

            if (position + value.Data.Length > buffer.Length)
            {
                throw new FileFormatException(position);
            }

            for (int i = 0; i < value.Data.Length; i++)
            {
                value.Data[i] = buffer[position + i];
            }

            position += (uint)value.Data.Length;
        }

        [DebuggerDisplay("{DebuggerDisplay.ToString(),nq}")]
        private class BaseType
        {
            public byte[] Data { get; protected set; }

            protected virtual StringBuilder DebuggerDisplay
            {
                get
                {
                    var result = new StringBuilder("data =");
                    foreach (var item in this.Data)
                    {
                        result.AppendFormat(" {0:X2}", item);
                    }

                    return result;
                }
            }
        }

        // C2FORMAT.TXT line 28ff
        //2. Elementarformate
        //-------------------
        //
        //BIT   einzelne Bits. Treten immer in Achtergruppen (Byte) oder Sechzehner-
        //      gruppen (Wort) auf. Diese Gruppen werden durch Diagramme dargestellt, 
        //      wobei rechts das nidrigwertigste Bit steht.
        //CHAR  1 Byte mit Vorzeichen
        //BYTE  1 Byte ohne Vorzichen
        //INT   2 Bytes mit Vorzeichen
        //BOOL  wie INT
        //UINT  2 Bytes ohne Vorzichen
        //LONG  4 Bytes mit Vorzichen
        //DWORD 4 Bytes ohne Vorzichen
        private class BYTE : BaseType
        {
            public BYTE()
            {
                this.Data = new byte[1];
            }

            protected override StringBuilder DebuggerDisplay
            {
                get
                {
                    StringBuilder result = base.DebuggerDisplay;
                    return result.AppendFormat(", value = {0}, bits = {1}", (byte)this, Convert.ToString(this, 2));
                }
            }

            public static implicit operator byte(BYTE d)
            {
                return d.Data[0];
            }
        }

        private class CHAR : BaseType
        {
            public CHAR()
            {
                this.Data = new byte[1];
            }

            protected override StringBuilder DebuggerDisplay
            {
                get
                {
                    StringBuilder result = base.DebuggerDisplay;
                    return result.AppendFormat(", value = {0}, bits = {1}", (byte)this, Convert.ToString(this, 2));
                }
            }

            public static implicit operator byte(CHAR d)
            {
                return d.Data[0];
            }
        }

        private class UINT : BaseType
        {
            public UINT()
            {
                this.Data = new byte[2];
            }

            protected override StringBuilder DebuggerDisplay
            {
                get
                {
                    StringBuilder result = base.DebuggerDisplay;
                    return result.AppendFormat(", value = {0}", (ushort)this);
                }
            }

            public static implicit operator ushort(UINT d)
            {
                return BitConverter.ToUInt16(d.Data, 0);
            }
        }

        private class INT : BaseType
        {
            public INT()
                : this(0)
            {
            }

            public INT(int value)
            {
                this.Data = value > 0 ? BitConverter.GetBytes(value) : new byte[2];
            }

            protected override StringBuilder DebuggerDisplay
            {
                get
                {
                    StringBuilder result = base.DebuggerDisplay;
                    return result.AppendFormat(", value = {0}", (int)this);
                }
            }

            public static implicit operator int(INT d)
            {
                return BitConverter.ToInt16(d.Data, 0);
            }
        }

        private class BOOL : INT
        {
            protected override StringBuilder DebuggerDisplay
            {
                get
                {
                    StringBuilder result = base.DebuggerDisplay;
                    return result.AppendFormat(", value = {0}", (bool)this);
                }
            }

            public static implicit operator bool(BOOL d)
            {
                return BitConverter.ToBoolean(d.Data, 0);
            }
        }

        private class LONG : BaseType
        {
            public LONG()
            {
                this.Data = new byte[4];
            }

            protected override StringBuilder DebuggerDisplay
            {
                get
                {
                    StringBuilder result = base.DebuggerDisplay;
                    return result.AppendFormat(", value = {0}", (int)this);
                }
            }

            public static implicit operator int(LONG d)
            {
                return BitConverter.ToInt32(d.Data, 0);
            }
        }

        private class DWORD : BaseType
        {
            public DWORD()
            {
                this.Data = new byte[4];
            }

            protected override StringBuilder DebuggerDisplay
            {
                get
                {
                    StringBuilder result = base.DebuggerDisplay;
                    return result.AppendFormat(", value = {0}", (uint)this);
                }
            }

            public static implicit operator uint(DWORD d)
            {
                return BitConverter.ToUInt32(d.Data, 0);
            }
        }
    }
}

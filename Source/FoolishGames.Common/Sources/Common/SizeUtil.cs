using System;
using System.Collections.Generic;
using System.Text;

namespace FoolishGames.Common
{
    public static class SizeUtil
    {
        public static int BoolSize { get; private set; }

        public static int CharSize { get; private set; }


        public static int FloatSize { get; private set; }

        public static int DoubleSize { get; private set; }

        public static int DecimalSize { get; private set; }


        public static int SByteSize { get; private set; }

        public static int ShortSize { get; private set; }

        public static int IntSize { get; private set; }

        public static int LongSize { get; private set; }


        public static int ByteSize { get; private set; }

        public static int UShortSize { get; private set; }

        public static int UIntSize { get; private set; }

        public static int ULongSize { get; private set; }

        static SizeUtil()
        {
            BoolSize = sizeof(bool);
            CharSize = sizeof(char);

            FloatSize = sizeof(float);
            DoubleSize = sizeof(double);
            DecimalSize = sizeof(decimal);

            SByteSize = sizeof(sbyte);
            ShortSize = sizeof(short);
            IntSize = sizeof(int);
            LongSize = sizeof(long);

            ByteSize = sizeof(byte);
            UShortSize = sizeof(ushort);
            UIntSize = sizeof(uint);
            ULongSize = sizeof(ulong);
        }
    }
}

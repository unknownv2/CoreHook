using System;
using System.Collections.Generic;
using System.Text;

namespace CoreHook
{
    public static class IntPtrExtensions
    {
        #region Methods: Arithmetics
        public static IntPtr Decrement(this IntPtr pointer, Int32 value)
        {
            return Increment(pointer, -value);
        }

        public static IntPtr Decrement(this IntPtr pointer, Int64 value)
        {
            return Increment(pointer, -value);
        }

        public static IntPtr Decrement(this IntPtr pointer, IntPtr value)
        {
            switch (IntPtr.Size)
            {
                case sizeof(Int32):
                    return (new IntPtr(pointer.ToInt32() - value.ToInt32()));

                default:
                    return (new IntPtr(pointer.ToInt64() - value.ToInt64()));
            }
        }

        public static IntPtr Increment(this IntPtr pointer, Int32 value)
        {
            unchecked
            {
                switch (IntPtr.Size)
                {
                    case sizeof(Int32):
                        return (new IntPtr(pointer.ToInt32() + value));

                    default:
                        return (new IntPtr(pointer.ToInt64() + value));
                }
            }
        }

        public static IntPtr Increment(this IntPtr pointer, Int64 value)
        {
            unchecked
            {
                switch (IntPtr.Size)
                {
                    case sizeof(Int32):
                        return (new IntPtr((Int32)(pointer.ToInt32() + value)));

                    default:
                        return (new IntPtr(pointer.ToInt64() + value));
                }
            }
        }

        public static IntPtr Increment(this IntPtr pointer, IntPtr value)
        {
            unchecked
            {
                switch (IntPtr.Size)
                {
                    case sizeof(int):
                        return new IntPtr(pointer.ToInt32() + value.ToInt32());
                    default:
                        return new IntPtr(pointer.ToInt64() + value.ToInt64());
                }
            }
        }
        #endregion

        #region Methods: Comparison
        public static Int32 CompareTo(this IntPtr left, Int32 right)
        {
            return left.CompareTo((UInt32)right);
        }

        public static Int32 CompareTo(this IntPtr left, IntPtr right)
        {
            if (left.ToUInt64() > right.ToUInt64())
                return 1;

            if (left.ToUInt64() < right.ToUInt64())
                return -1;

            return 0;
        }

        public static Int32 CompareTo(this IntPtr left, UInt32 right)
        {
            if (left.ToUInt64() > right)
                return 1;

            if (left.ToUInt64() < right)
                return -1;

            return 0;
        }
        #endregion

        #region Methods: Conversion
        
        public static UInt32 ToUInt32(this IntPtr pointer)
        {
            return (UInt32)(pointer.ToInt32());
        }

        public static UInt64 ToUInt64(this IntPtr pointer)
        {
            return (UInt64)(pointer.ToInt64());
        }
        
        #endregion

        #region Methods: Equality
        public static Boolean Equals(this IntPtr pointer, Int32 value)
        {
            return (pointer.ToInt32() == value);
        }

        public static Boolean Equals(this IntPtr pointer, Int64 value)
        {
            return (pointer.ToInt64() == value);
        }

        public static Boolean Equals(this IntPtr left, IntPtr ptr2)
        {
            return (left == ptr2);
        }

        public static Boolean Equals(this IntPtr pointer, UInt32 value)
        {
            return (pointer.ToUInt32() == value);
        }

        public static Boolean Equals(this IntPtr pointer, UInt64 value)
        {
            return (pointer.ToUInt64() == value);
        }
        public static Boolean GreaterThan(this IntPtr left, IntPtr right)
        {
            return (left.CompareTo(right) > 0);
        }

        public static Boolean GreaterThanOrEqualTo(this IntPtr left, IntPtr right)
        {
            return (left.CompareTo(right) >= 0);
        }
        public static Boolean LessThan(this IntPtr left, IntPtr right)
        {
            return (left.CompareTo(right) < 0);
        }

        public static Boolean LessThanOrEqualTo(this IntPtr left, IntPtr right)
        {
            return (left.CompareTo(right) <= 0);
        }
        #endregion

        #region Methods: Logic
        public static IntPtr And(this IntPtr pointer, IntPtr value)
        {
            switch (IntPtr.Size)
            {
                case sizeof(Int32):
                    return (new IntPtr(pointer.ToInt32() & value.ToInt32()));

                default:
                    return (new IntPtr(pointer.ToInt64() & value.ToInt64()));
            }
        }

        public static IntPtr Not(this IntPtr pointer)
        {
            switch (IntPtr.Size)
            {
                case sizeof(Int32):
                    return (new IntPtr(~pointer.ToInt32()));

                default:
                    return (new IntPtr(~pointer.ToInt64()));
            }
        }

        public static IntPtr Or(this IntPtr pointer, IntPtr value)
        {
            switch (IntPtr.Size)
            {
                case sizeof(Int32):
                    return (new IntPtr(pointer.ToInt32() | value.ToInt32()));

                default:
                    return (new IntPtr(pointer.ToInt64() | value.ToInt64()));
            }
        }

        public static IntPtr Xor(this IntPtr pointer, IntPtr value)
        {
            switch (IntPtr.Size)
            {
                case sizeof(Int32):
                    return (new IntPtr(pointer.ToInt32() ^ value.ToInt32()));

                default:
                    return (new IntPtr(pointer.ToInt64() ^ value.ToInt64()));
            }
        }
        #endregion
    }
}

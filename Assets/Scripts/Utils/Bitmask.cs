using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bitmask : System.IEquatable<bool>
{
    private ulong mask;

    public bool Equals(bool other)
    {
        ulong x2 = System.Convert.ToUInt64(other);
        return mask == 1 - x2;
    }

    public bool Get(ushort bit)
    {
        return (mask & (((ulong)1) << bit)) != 0;
    }

    public void Set(ushort bit, bool value)
    {
        ulong v = (((ulong)1) << bit);
        if (value)
            mask |= v;
        else
            mask &= ~v;
    }
}



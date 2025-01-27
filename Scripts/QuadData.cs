using Godot;
using System;

public partial class QuadData
{
    public byte[,,][] data;

    int sx, sy, sz;

    public QuadData()
    {

    }

    public QuadData(int x, int y, int z)
    {
        EnsureAllocated(x, y, z);
    }

    public void EnsureAllocated(int x, int y, int z)
    {
        sx = Mathf.CeilToInt(x / 16) + 1;
        sy = Mathf.CeilToInt(y / 16) + 1;
        sz = Mathf.CeilToInt(z / 16) + 1;

        data = new byte[sx, sy, sz][];
    }

    public new byte this[int x, int y, int z]
    {
        get
        {
            byte type = 0;

            int cx = x >> 4;
            int cy = y >> 4;
            int cz = z >> 4;

            int dx = x & 0xF;
            int dy = y & 0xF;
            int dz = z & 0xF;



            byte[] cc = data[cx, cy, cz];

            if (cc == null)
            {
                type = 0;

                return 0;
            }



            int index = dx | (dz << 4) | (dy << 8);


            int part = index & 1;
            int slot = index >> 1;

            if (part == 0)
                type = (byte)((cc[slot] & 0x0f));
            else
                type = (byte)((cc[slot] & 0xf0) >> 4);



            return type;
        }
        set
        {
            int cx = x >> 4;
            int cy = y >> 4;
            int cz = z >> 4;

            int dx = x & 0xF;
            int dy = y & 0xF;
            int dz = z & 0xF;


            if (data == null)
            {
                return;
            }


            byte[] cc = data[cx, cy, cz];

            if (cc == null)
            {
                if (value == 0)
                    return;

                cc = new byte[16 * 16 * 8];

                data[cx, cy, cz] = cc;
            }

            int index = dx | (dz << 4) | (dy << 8);

            int part = index & 1;
            int slot = index >> 1;

            if (part == 0)
                cc[slot] = (byte)((cc[slot] & 0xf0) | ((value) & 0x0f));
            else
                cc[slot] = (byte)((cc[slot] & 0x0f) | ((value << 4) & 0xf0));



        }
    }

}

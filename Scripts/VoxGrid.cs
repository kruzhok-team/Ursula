using Godot;
using System;
using System.IO;

[Serializable]
class Chunk
{
    public byte[] type;
    public byte[] xtype;
    public byte[] data;

    public int y;
}

[Serializable]
public class VoxTypesGrid
{
    public VoxGrid _grid;

    public VoxTypesGrid(VoxGrid grid)
    {
        _grid = grid;
    }

    public int this[int x, int y, int z]
    {
        get
        {

            return _grid.Get(x, y, z);

        }

        set
        {

            _grid.Set(x, y, z, value);

        }
    }


}

[Serializable]
public class VoxDataGrid
{
    public VoxGrid _grid;

    public VoxDataGrid(VoxGrid grid)
    {
        _grid = grid;
    }

    public byte this[int x, int y, int z]
    {
        get
        {

            return (byte)_grid.Getdata(x, y, z);

        }

        set
        {

            _grid.Setdata(x, y, z, value);

        }
    }


}

[Serializable]
public class VoxGrid
{
    Chunk[,,] chunks;

    int sizeX;
    int sizeY;
    int sizeZ;

    public VoxGrid(int x, int y, int z)
    {
        sizeX = Mathf.CeilToInt((float)x / 16);
        sizeY = Mathf.CeilToInt((float)y / 16);
        sizeZ = Mathf.CeilToInt((float)z / 16);

        chunks = new Chunk[sizeX, sizeY, sizeZ];
    }

    public void Load(MemoryStream ms)
    {
        int nnempty = 0;
        int nnemptydata = 0;

        for (int z = 0; z < sizeZ; z++)
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                {

                    int tt = ms.ReadByte();

                    if (tt == 0)
                        continue;

                    Chunk cc = chunks[x, y, z];

                    cc = new Chunk();

                    cc.y = ms.ReadByte();
                    chunks[x, y, z] = cc;

                    if ((tt & 0x1) == 0x1)
                    {
                        cc.type = new byte[16 * 16 * 16];
                        ms.Read(cc.type, 0, cc.type.Length);
                    }

                    if ((tt & 0x4) == 0x4)
                    {
                        cc.xtype = new byte[16 * 16 * 8];

                        ms.Read(cc.xtype, 0, cc.xtype.Length);
                    }

                    if ((tt & 0x2) == 0x2)
                    {
                        cc.data = new byte[16 * 16 * 16];

                        ms.Read(cc.data, 0, cc.data.Length);
                    }



                }

        GD.Print("empty " + nnempty);
        GD.Print("empty data " + nnemptydata);
    }

    public void Save(MemoryStream ms)
    {
        int nnempty = 0;
        int nnemptydata = 0;

        for (int z = 0; z < sizeZ; z++)
            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                {



                    Chunk cc = chunks[x, y, z];

                    if (cc == null)
                    {
                        ms.WriteByte(0);

                        nnempty++;
                        continue;
                    }
                    byte tt = 0x0;

                    if (cc.type != null)
                        tt = 0x1;

                    if (cc.data != null)
                        tt |= 0x2;

                    if (cc.xtype != null)
                        tt |= 0x4;

                    ms.WriteByte(tt);
                    ms.WriteByte((byte)cc.y);

                    if (cc.type != null)
                        ms.Write(cc.type, 0, cc.type.Length);

                    if (cc.xtype != null)
                        ms.Write(cc.xtype, 0, cc.xtype.Length);

                    if (cc.data != null)
                    {
                        ms.Write(cc.data, 0, cc.data.Length);
                    }
                    else
                    {
                        nnemptydata++;
                    }
                }


        GD.Print("empty " + nnempty);
        GD.Print("empty data " + nnemptydata);
    }


    public void Set(int x, int y, int z, int type)
    {


        int cx = x >> 4;
        int cy = y >> 4;
        int cz = z >> 4;

        int dx = x & 0xF;
        int dy = y & 0xF;
        int dz = z & 0xF;

        if (chunks == null)
        {
            GD.Print("Nothing chank!");
            return;
        }

        Chunk cc = chunks[cx, cy, cz];

        if (cc == null)
        {
            if (type == 0)
                return;

            cc = new Chunk();
            cc.y = cy;

            chunks[cx, cy, cz] = cc;
        }

        if (type != 0 && cc.type == null)
            cc.type = new byte[16 * 16 * 16];

        if (type > 255 && cc.xtype == null)
            cc.xtype = new byte[16 * 16 * 8];



        int index = dx | (dz << 4) | (dy << 8);

        if (cc.type != null)
            cc.type[index] = (byte)type;

        if (cc.xtype != null)
        {
            int part = index & 1;
            int slot = index >> 1;

            if (part == 0)
                cc.xtype[slot] = (byte)((cc.xtype[slot] & 0xf0) | ((type >> 8) & 0x0f));
            else
                cc.xtype[slot] = (byte)((cc.xtype[slot] & 0x0f) | ((type >> 4) & 0xf0));
        }


    }

    public void Set(int x, int y, int z, int type, int data)
    {


        int cx = x >> 4;
        int cy = y >> 4;
        int cz = z >> 4;

        int dx = x & 0xF;
        int dy = y & 0xF;
        int dz = z & 0xF;

        if (chunks == null)
        {
            GD.Print("Nothing chank!");
            return;
        }

        Chunk cc = chunks[cx, cy, cz];

        if (cc == null)
        {
            if (type == 0 && data == 0)
                return;

            cc = new Chunk();
            cc.y = cy;

            chunks[cx, cy, cz] = cc;
        }

        if (type != 0 && cc.type == null)
            cc.type = new byte[16 * 16 * 16];

        if (type > 255 && cc.xtype == null)
            cc.xtype = new byte[16 * 16 * 8];

        if ((data != 0) && cc.data == null)
            cc.data = new byte[16 * 16 * 16];

        int index = dx | (dz << 4) | (dy << 8);

        if (cc.type != null)
            cc.type[index] = (byte)type;

        if (cc.xtype != null)
        {
            int part = index & 1;
            int slot = index >> 1;

            if (part == 0)
                cc.xtype[slot] = (byte)((cc.xtype[slot] & 0xf0) | ((type >> 8) & 0x0f));
            else
                cc.xtype[slot] = (byte)((cc.xtype[slot] & 0x0f) | ((type >> 4) & 0xf0));
        }

        if (cc.data != null)
            cc.data[index] = (byte)data;
    }

    public void Setdata(int x, int y, int z, int data)
    {
        if (chunks == null) return;

        int cx = x >> 4;
        int cy = y >> 4;
        int cz = z >> 4;

        int dx = x & 0xF;
        int dy = y & 0xF;
        int dz = z & 0xF;

        Chunk cc = chunks[cx, cy, cz];

        if (cc == null)
        {
            if (data == 0)
                return;

            cc = new Chunk();
            cc.y = cy;

            chunks[cx, cy, cz] = cc;
        }



        if ((data != 0) && cc.data == null)
            cc.data = new byte[16 * 16 * 16];

        int index = dx | (dz << 4) | (dy << 8);





        if (cc.data != null)
            cc.data[index] = (byte)data;
    }

    public int Get(int x, int y, int z)
    {
        int cx = x >> 4;
        int cy = y >> 4;
        int cz = z >> 4;

        int dx = x & 0xF;
        int dy = y & 0xF;
        int dz = z & 0xF;

        Chunk cc = chunks[cx, cy, cz];

        if (cc == null)
            return 0;

        if (cc.type == null)
            return 0;

        int index = dx | (dz << 4) | (dy << 8);

        if (cc.xtype != null)
        {
            int part = index & 1;
            int slot = index >> 1;

            if (part == 0)
                return ((cc.xtype[slot] & 0x0f) << 8) | cc.type[index];
            else
                return ((cc.xtype[slot] & 0xf0) << 4) | cc.type[index];
        }

        return cc.type[index];
    }

    public byte Getdata(int x, int y, int z)
    {
        int cx = x >> 4;
        int cy = y >> 4;
        int cz = z >> 4;

        int dx = x & 0xF;
        int dy = y & 0xF;
        int dz = z & 0xF;

        Chunk cc = chunks[cx, cy, cz];

        if (cc == null)
            return 0;

        if (cc.data == null)
            return 0;

        int index = dx | (dz << 4) | (dy << 8);

        return cc.data[index];
    }

    public int Get(int x, int y, int z, ref int type, ref int data)
    {
        int cx = x >> 4;
        int cy = y >> 4;
        int cz = z >> 4;

        int dx = x & 0xF;
        int dy = y & 0xF;
        int dz = z & 0xF;

        Chunk cc = chunks[cx, cy, cz];

        if (cc == null)
        {
            type = 0;
            data = 0;

            return 0;
        }

        if (cc.type == null)
            type = 0;

        int index = dx | (dz << 4) | (dy << 8);

        if (cc.xtype != null)
        {
            int part = index & 1;
            int slot = index >> 1;

            if (part == 0)
                type = ((cc.xtype[slot] & 0x0f) << 8) | cc.type[index];
            else
                type = ((cc.xtype[slot] & 0xf0) << 4) | cc.type[index];
        }

        type = cc.type[index];


        if (cc.data == null)
            data = 0;
        else
            data = cc.data[index];

        return type;
    }

}


using UnityEngine;
using System.IO;

public enum CellType : byte
{
    Empty = 0,
    Grass = 1,
    Path  = 2,
    Ramp  = 3,
    Water = 4,
    Lake  = 5
}

[System.Serializable]
public struct CellData
{
    public int height;
    public CellType type;
    public byte bitmask;
    public byte direction; // 0=N, 1=E, 2=S, 3=W

    public static CellData Default()
    {
        return new CellData
        {
            height    = 0,
            type      = CellType.Grass,
            bitmask   = 0,
            direction = 0
        };
    }
}

[CreateAssetMenu(fileName = "NewGridData", menuName = "Terrain/Grid Data")]
public class GridData : ScriptableObject
{
    public int gridWidth  = 16;
    public int gridDepth  = 16;

    [HideInInspector]
    public int[] serializedHeights;
    [HideInInspector]
    public byte[] serializedTypes;
    [HideInInspector]
    public byte[] serializedBitmasks;
    [HideInInspector]
    public byte[] serializedDirections;

    [System.NonSerialized]
    private CellData[] cells;

    // ─── Accessors ──────────────────────────────────────────────

    public void EnsureInitialized()
    {
        int total = gridWidth * gridDepth;
        if (cells != null && cells.Length == total)
            return;

        cells = new CellData[total];

        bool hasData = serializedHeights != null && serializedHeights.Length == total;
        for (int i = 0; i < total; i++)
        {
            if (hasData)
            {
                cells[i] = new CellData
                {
                    height    = serializedHeights[i],
                    type      = (CellType)serializedTypes[i],
                    bitmask   = serializedBitmasks[i],
                    direction = serializedDirections[i]
                };
            }
            else
            {
                cells[i] = CellData.Default();
            }
        }
    }

    public CellData GetCell(int x, int z)
    {
        EnsureInitialized();
        if (x < 0 || x >= gridWidth || z < 0 || z >= gridDepth)
            return CellData.Default();
        return cells[z * gridWidth + x];
    }

    public void SetCell(int x, int z, CellData data)
    {
        EnsureInitialized();
        if (x < 0 || x >= gridWidth || z < 0 || z >= gridDepth)
            return;
        cells[z * gridWidth + x] = data;
    }

    public int GetHeight(int x, int z)
    {
        return GetCell(x, z).height;
    }

    public void SetHeight(int x, int z, int h)
    {
        EnsureInitialized();
        if (x < 0 || x >= gridWidth || z < 0 || z >= gridDepth)
            return;
        int idx = z * gridWidth + x;
        CellData c = cells[idx];
        c.height = h;
        cells[idx] = c;
    }

    public CellType GetCellType(int x, int z)
    {
        return GetCell(x, z).type;
    }

    public void SetCellType(int x, int z, CellType t)
    {
        EnsureInitialized();
        if (x < 0 || x >= gridWidth || z < 0 || z >= gridDepth)
            return;
        int idx = z * gridWidth + x;
        CellData c = cells[idx];
        c.type = t;
        cells[idx] = c;
    }

    public void SetBitmask(int x, int z, byte b)
    {
        EnsureInitialized();
        if (!InBounds(x, z)) return;
        int idx = z * gridWidth + x;
        CellData c = cells[idx];
        c.bitmask = b;
        cells[idx] = c;
    }

    public void SetDirection(int x, int z, byte d)
    {
        EnsureInitialized();
        if (!InBounds(x, z)) return;
        int idx = z * gridWidth + x;
        CellData c = cells[idx];
        c.direction = d;
        cells[idx] = c;
    }

    /// <summary>Returns cardinal neighbor heights [N, E, S, W]. Out-of-bounds returns 0.</summary>
    public int[] GetNeighborHeights(int x, int z)
    {
        return new int[]
        {
            GetHeight(x, z + 1), // N
            GetHeight(x + 1, z), // E
            GetHeight(x, z - 1), // S
            GetHeight(x - 1, z)  // W
        };
    }

    /// <summary>4-bit mask of cardinal neighbors that are higher than this cell (for wall generation).</summary>
    public byte GetWallMask(int x, int z)
    {
        int h = GetHeight(x, z);
        byte mask = 0;
        int[] nh = GetNeighborHeights(x, z);
        for (int i = 0; i < 4; i++)
        {
            // Also generate wall on grid boundary
            int nx = x + new int[]{ 0, 1, 0,-1}[i];
            int nz = z + new int[]{ 1, 0,-1, 0}[i];
            if (!InBounds(nx, nz) || nh[i] < h)
                mask |= (byte)(1 << i);
        }
        return mask;
    }

    /// <summary>4-bit mask of cardinal neighbors that are higher (for lake edge slopes).</summary>
    public byte DetectLakeEdge(int x, int z)
    {
        int h = GetHeight(x, z);
        byte mask = 0;
        int[] nh = GetNeighborHeights(x, z);
        for (int i = 0; i < 4; i++)
        {
            if (nh[i] > h)
                mask |= (byte)(1 << i);
        }
        return mask;
    }

    /// <summary>Detects if a 1x2 ramp can be placed at (x,z) in a given direction. Returns true if valid.</summary>
    public bool CanPlace1x2Ramp(int x, int z, int dir)
    {
        int[] dx = { 0, 1, 0, -1 };
        int[] dz = { 1, 0, -1, 0 };
        int mx = x + dx[dir];
        int mz = z + dz[dir];
        int ex = x + dx[dir] * 2;
        int ez = z + dz[dir] * 2;
        if (!InBounds(mx, mz) || !InBounds(ex, ez)) return false;
        int h0 = GetHeight(x, z);
        int h2 = GetHeight(ex, ez);
        return Mathf.Abs(h0 - h2) == 1;
    }

    // ─── Flush runtime data → serialized arrays (call before saving SO) ─

    public void FlushToSerialized()
    {
        if (cells == null) return;
        int total = cells.Length;
        serializedHeights    = new int[total];
        serializedTypes      = new byte[total];
        serializedBitmasks   = new byte[total];
        serializedDirections = new byte[total];

        for (int i = 0; i < total; i++)
        {
            serializedHeights[i]    = cells[i].height;
            serializedTypes[i]      = (byte)cells[i].type;
            serializedBitmasks[i]   = cells[i].bitmask;
            serializedDirections[i] = cells[i].direction;
        }
    }

    // ─── Bitmask Recalculation ──────────────────────────────────

    public void RecalculateAllBitmasks()
    {
        EnsureInitialized();
        for (int z = 0; z < gridDepth; z++)
        for (int x = 0; x < gridWidth; x++)
        {
            int idx = z * gridWidth + x;
            if (cells[idx].type == CellType.Path || cells[idx].type == CellType.Ramp)
                cells[idx].bitmask = CalculatePathBitmask(x, z);
            else
                cells[idx].bitmask = 0;
        }
    }

    public byte CalculatePathBitmask(int x, int z)
    {
        // 8-bit mask: N NE E SE S SW W NW  (bits 0-7)
        byte mask = 0;
        int[,] offsets = {
            { 0,  1}, // N  - bit 0
            { 1,  1}, // NE - bit 1
            { 1,  0}, // E  - bit 2
            { 1, -1}, // SE - bit 3
            { 0, -1}, // S  - bit 4
            {-1, -1}, // SW - bit 5
            {-1,  0}, // W  - bit 6
            {-1,  1}  // NW - bit 7
        };

        for (int i = 0; i < 8; i++)
        {
            int nx = x + offsets[i, 0];
            int nz = z + offsets[i, 1];
            if (nx >= 0 && nx < gridWidth && nz >= 0 && nz < gridDepth)
            {
                CellType t = cells[nz * gridWidth + nx].type;
                if (t == CellType.Path || t == CellType.Ramp)
                    mask |= (byte)(1 << i);
            }
        }
        return mask;
    }

    // ─── Auto-Ramp Detection ────────────────────────────────────

    // Returns direction (0-3) for a ramp connecting this cell to a neighbor with height diff of 1
    // Works from both the high cell and the low cell
    // Returns -1 if no ramp is possible
    public int DetectRampDirection(int x, int z)
    {
        int h = GetHeight(x, z);
        // Cardinal offsets: N(0,1)=0  E(1,0)=1  S(0,-1)=2  W(-1,0)=3
        int[] dx = { 0, 1, 0, -1 };
        int[] dz = { 1, 0, -1, 0 };

        // First pass: descending (this cell is higher)
        for (int d = 0; d < 4; d++)
        {
            int nx = x + dx[d];
            int nz = z + dz[d];
            int nh = GetHeight(nx, nz);
            if (h - nh == 1)
                return d; // ramp descends toward this direction
        }

        // Second pass: ascending (this cell is lower)
        for (int d = 0; d < 4; d++)
        {
            int nx = x + dx[d];
            int nz = z + dz[d];
            int nh = GetHeight(nx, nz);
            if (nh - h == 1)
                return d; // ramp ascends toward this direction
        }

        return -1;
    }

    // ─── Binary Persistence ─────────────────────────────────────

    private static readonly byte[] MAGIC = { 0x47, 0x52, 0x49, 0x44 }; // "GRID"
    private const byte VERSION = 1;

    public void SaveBinary(string path)
    {
        EnsureInitialized();
        using (BinaryWriter w = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            // Header
            w.Write(MAGIC);
            w.Write(VERSION);
            w.Write(gridWidth);
            w.Write(gridDepth);

            // Cell data (row-major)
            int total = gridWidth * gridDepth;
            for (int i = 0; i < total; i++)
            {
                w.Write(cells[i].height);
                w.Write((byte)cells[i].type);
                w.Write(cells[i].bitmask);
                w.Write(cells[i].direction);
            }
        }
        Debug.Log($"[GridData] Saved {gridWidth}x{gridDepth} grid to: {path}");
    }

    public bool LoadBinary(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"[GridData] File not found: {path}");
            return false;
        }

        using (BinaryReader r = new BinaryReader(File.Open(path, FileMode.Open)))
        {
            // Validate header
            byte[] magic = r.ReadBytes(4);
            for (int i = 0; i < 4; i++)
            {
                if (magic[i] != MAGIC[i])
                {
                    Debug.LogError("[GridData] Invalid file format (bad magic bytes).");
                    return false;
                }
            }

            byte version = r.ReadByte();
            if (version != VERSION)
            {
                Debug.LogError($"[GridData] Unsupported version: {version}");
                return false;
            }

            gridWidth = r.ReadInt32();
            gridDepth = r.ReadInt32();

            int total = gridWidth * gridDepth;
            cells = new CellData[total];

            for (int i = 0; i < total; i++)
            {
                cells[i] = new CellData
                {
                    height    = r.ReadInt32(),
                    type      = (CellType)r.ReadByte(),
                    bitmask   = r.ReadByte(),
                    direction = r.ReadByte()
                };
            }
        }

        FlushToSerialized();
        Debug.Log($"[GridData] Loaded {gridWidth}x{gridDepth} grid from: {path}");
        return true;
    }

    // ─── Utility ────────────────────────────────────────────────

    public void ClearGrid()
    {
        int total = gridWidth * gridDepth;
        cells = new CellData[total];
        for (int i = 0; i < total; i++)
            cells[i] = CellData.Default();
        FlushToSerialized();
    }

    public bool InBounds(int x, int z)
    {
        return x >= 0 && x < gridWidth && z >= 0 && z < gridDepth;
    }
}

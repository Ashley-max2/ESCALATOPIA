using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedural mesh generator for the grid terrain system.
/// Builds all geometry directly into a single vertex buffer (no CombineMeshes).
/// </summary>
public static class MeshGenerator
{
    // ─── Vertex Colors ───────────────────────────────────────────
    static readonly Color COL_GRASS     = new Color(0.35f, 0.65f, 0.20f);
    static readonly Color COL_PATH      = new Color(0.72f, 0.53f, 0.30f);
    static readonly Color COL_WALL      = new Color(0.30f, 0.18f, 0.08f);
    static readonly Color COL_LAKE_BED  = new Color(0.40f, 0.35f, 0.25f);
    static readonly Color COL_LAKE_EDGE = new Color(0.50f, 0.40f, 0.25f);

    // Cell size in world units
    public static float cellSize = 1f;
    public static float heightStep = 1f;

    // Cardinal direction offsets: N(+Z)=0, E(+X)=1, S(-Z)=2, W(-X)=3
    static readonly int[] DX = { 0, 1, 0, -1 };
    static readonly int[] DZ = { 1, 0, -1, 0 };

    // Shared buffers (reused each build)
    static List<Vector3> verts = new List<Vector3>();
    static List<int> tris = new List<int>();
    static List<Color> cols = new List<Color>();

    // ─── Entry Point ─────────────────────────────────────────────

    public static Mesh BuildFullMesh(GridData grid)
    {
        grid.EnsureInitialized();
        grid.RecalculateAllBitmasks();

        verts.Clear();
        tris.Clear();
        cols.Clear();

        for (int z = 0; z < grid.gridDepth; z++)
        {
            for (int x = 0; x < grid.gridWidth; x++)
            {
                CellData cell = grid.GetCell(x, z);
                float wx = x * cellSize;
                float wz = z * cellSize;
                float wy = cell.height * heightStep;

                switch (cell.type)
                {
                    case CellType.Grass:
                    case CellType.Empty:
                        AddQuad(wx, wz, wy, COL_GRASS);
                        AddWalls(grid, x, z);
                        break;

                    case CellType.Path:
                        AddQuad(wx, wz, wy, COL_PATH);
                        AddWalls(grid, x, z);
                        break;

                    case CellType.Ramp:
                        AddRamp(wx, wz, wy, cell.direction, cell.bitmask);
                        AddRampSideWalls(wx, wz, wy, cell.direction);
                        break;

                    case CellType.Lake:
                    case CellType.Water:
                        AddQuad(wx, wz, wy, COL_LAKE_BED);
                        AddLakeEdges(grid, x, z, wx, wz, wy);
                        break;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetColors(cols);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.name = "TerrainGrid";
        return mesh;
    }

    // ─── Flat Quad (top face) ────────────────────────────────────

    static void AddQuad(float x, float z, float y, Color col)
    {
        int i = verts.Count;
        float s = cellSize;

        verts.Add(new Vector3(x,     y, z));
        verts.Add(new Vector3(x + s, y, z));
        verts.Add(new Vector3(x + s, y, z + s));
        verts.Add(new Vector3(x,     y, z + s));

        // CW winding for upward-facing normal
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
        tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);

        cols.Add(col); cols.Add(col); cols.Add(col); cols.Add(col);
    }

    // ─── Walls ───────────────────────────────────────────────────

    static void AddWalls(GridData grid, int gx, int gz)
    {
        int h = grid.GetHeight(gx, gz);
        if (h <= 0) return;

        float cx = gx * cellSize;
        float cz = gz * cellSize;

        for (int dir = 0; dir < 4; dir++)
        {
            int nx = gx + DX[dir];
            int nz = gz + DZ[dir];
            int nh = grid.InBounds(nx, nz) ? grid.GetHeight(nx, nz) : 0;

            if (nh < h)
            {
                float botY = nh * heightStep;
                float topY = h * heightStep;
                AddWallFace(cx, cz, botY, topY, dir);
            }
        }
    }

    static void AddWallFace(float cx, float cz, float bot, float top, int dir)
    {
        int i = verts.Count;
        float s = cellSize;

        // 4 vertices per wall face, wound so normal faces outward
        switch (dir)
        {
            case 0: // North face (+Z)
                verts.Add(new Vector3(cx + s, bot, cz + s)); // 0 BR
                verts.Add(new Vector3(cx,     bot, cz + s)); // 1 BL
                verts.Add(new Vector3(cx,     top, cz + s)); // 2 TL
                verts.Add(new Vector3(cx + s, top, cz + s)); // 3 TR
                break;
            case 1: // East face (+X)
                verts.Add(new Vector3(cx + s, bot, cz));     // 0
                verts.Add(new Vector3(cx + s, bot, cz + s)); // 1
                verts.Add(new Vector3(cx + s, top, cz + s)); // 2
                verts.Add(new Vector3(cx + s, top, cz));     // 3
                break;
            case 2: // South face (-Z)
                verts.Add(new Vector3(cx,     bot, cz));     // 0
                verts.Add(new Vector3(cx + s, bot, cz));     // 1
                verts.Add(new Vector3(cx + s, top, cz));     // 2
                verts.Add(new Vector3(cx,     top, cz));     // 3
                break;
            case 3: // West face (-X)
                verts.Add(new Vector3(cx, bot, cz + s));     // 0
                verts.Add(new Vector3(cx, bot, cz));         // 1
                verts.Add(new Vector3(cx, top, cz));         // 2
                verts.Add(new Vector3(cx, top, cz + s));     // 3
                break;
        }

        // CW winding when viewed from outside = outward normal
        tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
        tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);

        cols.Add(COL_WALL); cols.Add(COL_WALL);
        cols.Add(COL_WALL); cols.Add(COL_WALL);
    }

    // ─── Ramp ────────────────────────────────────────────────────

    /// <summary>
    /// Builds a ramp surface. Uses COL_GRASS by default, COL_PATH if bitmask > 0 (has path neighbors).
    /// direction: 0=N, 1=E, 2=S, 3=W (ramp descends toward that direction)
    /// </summary>
    static void AddRamp(float x, float z, float y, byte direction, byte bitmask)
    {
        Color col = bitmask > 0 ? COL_PATH : COL_GRASS;

        int i = verts.Count;
        float s = cellSize;
        float topY = y;
        float botY = y - heightStep;

        switch (direction)
        {
            case 0: // North: high at south, low at north
                verts.Add(new Vector3(x,     topY, z));
                verts.Add(new Vector3(x + s, topY, z));
                verts.Add(new Vector3(x + s, botY, z + s));
                verts.Add(new Vector3(x,     botY, z + s));
                break;
            case 1: // East: high at west, low at east
                verts.Add(new Vector3(x,     topY, z));
                verts.Add(new Vector3(x,     topY, z + s));
                verts.Add(new Vector3(x + s, botY, z + s));
                verts.Add(new Vector3(x + s, botY, z));
                break;
            case 2: // South: high at north, low at south
                verts.Add(new Vector3(x,     botY, z));
                verts.Add(new Vector3(x + s, botY, z));
                verts.Add(new Vector3(x + s, topY, z + s));
                verts.Add(new Vector3(x,     topY, z + s));
                break;
            case 3: // West: high at east, low at west
                verts.Add(new Vector3(x,     botY, z));
                verts.Add(new Vector3(x,     botY, z + s));
                verts.Add(new Vector3(x + s, topY, z + s));
                verts.Add(new Vector3(x + s, topY, z));
                break;
        }

        tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
        tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);

        cols.Add(col); cols.Add(col); cols.Add(col); cols.Add(col);
    }

    // ─── Ramp Side Walls ─────────────────────────────────────────

    static void AddRampSideWalls(float x, float z, float y, byte direction)
    {
        float s = cellSize;
        float topY = y;
        float botY = y - heightStep;

        // Two triangles — one on each side of the ramp
        switch (direction)
        {
            case 0: // North — sides on West(x) and East(x+s)
                AddTriangle(
                    new Vector3(x, topY, z),
                    new Vector3(x, botY, z + s),
                    new Vector3(x, botY, z), COL_WALL);
                AddTriangle(
                    new Vector3(x + s, topY, z),
                    new Vector3(x + s, botY, z),
                    new Vector3(x + s, botY, z + s), COL_WALL);
                break;
            case 2: // South
                AddTriangle(
                    new Vector3(x, topY, z + s),
                    new Vector3(x, botY, z),
                    new Vector3(x, botY, z + s), COL_WALL);
                AddTriangle(
                    new Vector3(x + s, topY, z + s),
                    new Vector3(x + s, botY, z + s),
                    new Vector3(x + s, botY, z), COL_WALL);
                break;
            case 1: // East — sides on South(z) and North(z+s)
                AddTriangle(
                    new Vector3(x, topY, z),
                    new Vector3(x, botY, z),
                    new Vector3(x + s, botY, z), COL_WALL);
                AddTriangle(
                    new Vector3(x, topY, z + s),
                    new Vector3(x + s, botY, z + s),
                    new Vector3(x, botY, z + s), COL_WALL);
                break;
            case 3: // West
                AddTriangle(
                    new Vector3(x + s, topY, z),
                    new Vector3(x, botY, z),
                    new Vector3(x + s, botY, z), COL_WALL);
                AddTriangle(
                    new Vector3(x + s, topY, z + s),
                    new Vector3(x + s, botY, z + s),
                    new Vector3(x, botY, z + s), COL_WALL);
                break;
        }
    }

    static void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color col)
    {
        int i = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c);
        tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
        cols.Add(col); cols.Add(col); cols.Add(col);
    }

    // ─── Lake Edges ──────────────────────────────────────────────

    static void AddLakeEdges(GridData grid, int gx, int gz, float x, float z, float lakeY)
    {
        byte edgeMask = grid.DetectLakeEdge(gx, gz);
        if (edgeMask == 0) return;

        float s = cellSize;
        float sw = s * 0.4f;

        for (int dir = 0; dir < 4; dir++)
        {
            if ((edgeMask & (1 << dir)) == 0) continue;

            int nx = gx + DX[dir];
            int nz = gz + DZ[dir];
            float nY = grid.GetHeight(nx, nz) * heightStep;

            int i = verts.Count;
            switch (dir)
            {
                case 0: // North
                    verts.Add(new Vector3(x,     lakeY, z + s - sw));
                    verts.Add(new Vector3(x + s, lakeY, z + s - sw));
                    verts.Add(new Vector3(x + s, nY,    z + s));
                    verts.Add(new Vector3(x,     nY,    z + s));
                    break;
                case 1: // East
                    verts.Add(new Vector3(x + s - sw, lakeY, z));
                    verts.Add(new Vector3(x + s - sw, lakeY, z + s));
                    verts.Add(new Vector3(x + s,      nY,    z + s));
                    verts.Add(new Vector3(x + s,      nY,    z));
                    break;
                case 2: // South
                    verts.Add(new Vector3(x + s, lakeY, z + sw));
                    verts.Add(new Vector3(x,     lakeY, z + sw));
                    verts.Add(new Vector3(x,     nY,    z));
                    verts.Add(new Vector3(x + s, nY,    z));
                    break;
                case 3: // West
                    verts.Add(new Vector3(x + sw, lakeY, z + s));
                    verts.Add(new Vector3(x + sw, lakeY, z));
                    verts.Add(new Vector3(x,      nY,    z));
                    verts.Add(new Vector3(x,      nY,    z + s));
                    break;
            }

            tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
            tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);

            cols.Add(COL_LAKE_EDGE); cols.Add(COL_LAKE_EDGE);
            cols.Add(COL_LAKE_EDGE); cols.Add(COL_LAKE_EDGE);
        }
    }
}

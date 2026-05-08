using UnityEngine;
using System.Collections.Generic;

public static class WaterMeshBuilder
{
    struct Edge
    {
        public Vector3 a;
        public Vector3 b;
    }

    static Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>();
    static List<Vector3> vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();

    public static void BuildShoreMesh(
        Terrain terrain,
        float waterHeight,
        int res,
        float width,
        float heightOffset,
        float extrudeDepth)
    {
        vertexMap.Clear();
        vertices.Clear();
        triangles.Clear();

        TerrainData data = terrain.terrainData;
        Vector3 size = data.size;
        Vector3 origin = terrain.transform.position;

        float stepX = size.x / (res - 1);
        float stepZ = size.z / (res - 1);

        List<Edge> edges = new List<Edge>();

        // =========================
        // 1. GENERAR INTERSECCIONES
        // =========================
        for (int x = 0; x < res - 1; x++)
        {
            for (int z = 0; z < res - 1; z++)
            {
                Vector3 p00 = origin + new Vector3(x * stepX, 0, z * stepZ);
                Vector3 p10 = origin + new Vector3((x + 1) * stepX, 0, z * stepZ);
                Vector3 p01 = origin + new Vector3(x * stepX, 0, (z + 1) * stepZ);
                Vector3 p11 = origin + new Vector3((x + 1) * stepX, 0, (z + 1) * stepZ);

                float h00 = terrain.SampleHeight(p00);
                float h10 = terrain.SampleHeight(p10);
                float h01 = terrain.SampleHeight(p01);
                float h11 = terrain.SampleHeight(p11);

                List<Vector3> pts = new List<Vector3>();

                Check(p00, h00, p10, h10, waterHeight, pts);
                Check(p10, h10, p11, h11, waterHeight, pts);
                Check(p11, h11, p01, h01, waterHeight, pts);
                Check(p01, h01, p00, h00, waterHeight, pts);

                if (pts.Count == 2)
                    edges.Add(new Edge { a = pts[0], b = pts[1] });
            }
        }

        if (edges.Count == 0)
        {
            Debug.LogWarning("No hay intersección con el agua");
            return;
        }

        // =========================
        // 2. CONECTAR LÍNEAS
        // =========================
        List<List<Vector3>> lines = BuildLines(edges);

        // =========================
        // 3. GENERAR MESH
        // =========================
        foreach (var line in lines)
        {
            for (int i = 0; i < line.Count - 1; i++)
            {
                Vector3 p0 = line[i];
                Vector3 p1 = line[i + 1];

                p0.y = waterHeight;
                p1.y = waterHeight;

                Vector3 dir = (p1 - p0);
                dir.y = 0;
                dir.Normalize();

                Vector3 normal = Vector3.Cross(dir, Vector3.up);
                normal.y = 0;
                normal.Normalize();

                // 🔥 orientación global estable
                Vector3 center = terrain.transform.position + terrain.terrainData.size * 0.5f;
                Vector3 mid = (p0 + p1) * 0.5f;

                Vector3 outward = (mid - center).normalized;

                if (Vector3.Dot(normal, outward) < 0)
                    normal = -normal;

                // 🔥 SOLO EXPANDE HACIA UN LADO (clave para escala correcta)
                Vector3 v0 = p0;
                Vector3 v1 = p0 + normal * width;
                Vector3 v2 = p1;
                Vector3 v3 = p1 + normal * width;

                // altura
                v0.y = waterHeight + heightOffset;
                v1.y = waterHeight + heightOffset;
                v2.y = waterHeight + heightOffset;
                v3.y = waterHeight + heightOffset;

                // extrude
                Vector3 v0b = v0 + Vector3.down * extrudeDepth;
                Vector3 v1b = v1 + Vector3.down * extrudeDepth;
                Vector3 v2b = v2 + Vector3.down * extrudeDepth;
                Vector3 v3b = v3 + Vector3.down * extrudeDepth;

                int i0 = GetVertex(v0);
                int i1 = GetVertex(v1);
                int i2 = GetVertex(v2);
                int i3 = GetVertex(v3);

                int i0b = GetVertex(v0b);
                int i1b = GetVertex(v1b);
                int i2b = GetVertex(v2b);
                int i3b = GetVertex(v3b);

                // top
                AddQuad(i0, i2, i1, i3);

                // bottom
                AddQuad(i1b, i0b, i3b, i2b);

                // sides
                AddQuad(i0, i1, i0b, i1b);
                AddQuad(i2, i3, i2b, i3b);

                // 🔥 FRONT / BACK CLOSURE
                AddQuad(i0, i2, i0b, i2b);
                AddQuad(i1, i3, i1b, i3b);
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GameObject go = new GameObject("ShoreMesh_FINAL");
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>();
    }

    static int GetVertex(Vector3 v)
    {
        Vector3 key = new Vector3(
            Mathf.Round(v.x * 1000f),
            Mathf.Round(v.y * 1000f),
            Mathf.Round(v.z * 1000f)
        );

        if (vertexMap.TryGetValue(key, out int index))
            return index;

        index = vertices.Count;
        vertices.Add(v);
        vertexMap.Add(key, index);
        return index;
    }

    static List<List<Vector3>> BuildLines(List<Edge> edges)
    {
        List<List<Vector3>> lines = new List<List<Vector3>>();

        while (edges.Count > 0)
        {
            List<Vector3> line = new List<Vector3>();
            Edge e = edges[0];
            edges.RemoveAt(0);

            line.Add(e.a);
            line.Add(e.b);

            bool extended = true;

            while (extended)
            {
                extended = false;

                for (int i = 0; i < edges.Count; i++)
                {
                    if (Vector3.Distance(line[line.Count - 1], edges[i].a) < 0.01f)
                    {
                        line.Add(edges[i].b);
                        edges.RemoveAt(i);
                        extended = true;
                        break;
                    }
                    if (Vector3.Distance(line[line.Count - 1], edges[i].b) < 0.01f)
                    {
                        line.Add(edges[i].a);
                        edges.RemoveAt(i);
                        extended = true;
                        break;
                    }
                }
            }

            lines.Add(line);
        }

        return lines;
    }

    static void AddQuad(int a, int b, int c, int d)
    {
        triangles.Add(a); triangles.Add(b); triangles.Add(c);
        triangles.Add(c); triangles.Add(b); triangles.Add(d);
    }

    static void Check(Vector3 pA, float hA, Vector3 pB, float hB, float waterHeight, List<Vector3> pts)
    {
        if ((hA < waterHeight && hB > waterHeight) ||
            (hA > waterHeight && hB < waterHeight))
        {
            float t = (waterHeight - hA) / (hB - hA);
            Vector3 p = Vector3.Lerp(pA, pB, t);
            p.y = waterHeight;
            pts.Add(p);
        }
    }
}
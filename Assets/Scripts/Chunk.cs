using UnityEngine;
using System;
using System.Collections.Generic;

using UniRx;

public class Chunk
{
    //--Obj Ref--
    public GameObject obj;

    //--Scalar Field--
    public bool performTriangulation;
    public float[,,] field;

    //--Neighbour Chunks--
    public List<Chunk> neighbourChunks;

    //--Planet Ref--
    private Planet planet;

    //--Triangulation--
    private MarchingCubes marchingCubes;

    //--Mesh--
    private List<Vector3> vertBuffer;
    private Mesh m;
    private MeshFilter mf;
    private MeshRenderer mr;
    private MeshCollider mc;

    //--Private Stuff--
    public int n, s, x, y, z;

    //==========Constructor==========

    public Chunk(Planet planet, Transform par, int n, int s, int x, int y, int z, Material mat, int layerMask)
    {
        obj = new GameObject(string.Format("{0} / {1} / {2}", x, y, z), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        obj.transform.position = new Vector3(x * s, y * s, z * s);
        obj.transform.parent = par;
        obj.layer = layerMask;

        neighbourChunks = new List<Chunk>();

        performTriangulation = true;

        this.planet = planet;
        marchingCubes = new MarchingCubes(0.5f, new int[] { 0, 1, 2 });

        vertBuffer = new List<Vector3>();

        m = new Mesh();
        mf = obj.GetComponent<MeshFilter>();
        mr = obj.GetComponent<MeshRenderer>();
        mc = obj.GetComponent<MeshCollider>();

        m.Clear();
        mf.sharedMesh = null;
        mr.material = mat;
        mc.sharedMesh = null;

        this.n = n;
        this.s = s;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    //==========Public Methods==========

    public void SetNeighbourChunks()
    {
        for(int nX = x - 1; nX <= x + 1; nX++)
            for(int nY = y - 1; nY <= y + 1; nY++)
                for(int nZ = z - 1; nZ <= z + 1; nZ++)
                {
                    if(nX > 0 && nX < n)
                        if(nY > 0 && nY < n)
                            if(nZ > 0 && nZ < n)
                            {
                                neighbourChunks.Add(planet.chunks[nX + nY * n + nZ * n * n]);
                            }
                }
    }

    public void SetMarchingCubesNeighbours()
    {
        Chunk neighbourX = null;
        Chunk neighbourY = null;
        Chunk neighbourZ = null;
        Chunk neighbourXY = null;
        Chunk neighbourXZ = null;
        Chunk neighbourZY = null;
        Chunk neighbourXYZ = null;

        if (x + 1 < n) neighbourX = planet.chunks[(x + 1) + y * n + z * n * n];
        if (y + 1 < n) neighbourY = planet.chunks[x + (y + 1) * n + z * n * n];
        if (z + 1 < n) neighbourZ = planet.chunks[x + y * n + (z + 1) * n * n];

        if (x + 1 < n && y + 1 < n) neighbourXY = planet.chunks[(x + 1) + (y + 1) * n + z * n * n];
        if (x + 1 < n && z + 1 < n) neighbourXZ = planet.chunks[(x + 1) + y * n + (z + 1) * n * n];
        if (z + 1 < n && y + 1 < n) neighbourZY = planet.chunks[x + (y + 1) * n + (z + 1) * n * n];

        if (x + 1 < n && y + 1 < n && z + 1 < n) neighbourXYZ = planet.chunks[(x + 1) + (y + 1) * n + (z + 1) * n * n];

        marchingCubes.SetNeighbourChunks(neighbourX, neighbourY, neighbourZ, neighbourXY, neighbourXZ, neighbourZY, neighbourXYZ);
    }

    public Vector3 GetPosition()
    {
        return new Vector3(x * s, y * s, z * s);
    }

    public Vector3 GetCenter()
    {
        return new Vector3(x * s + (s / 2), y * s + s / 2, z * s + s / 2);
    }

    public void Generate()
    {
        field = new float[s, s, s];
        planet.GenerateEmptySurface(this);
    }

    public void Build()
    {
        if (!performTriangulation)
            return;

        marchingCubes.CreateMesh(field);

        vertBuffer.Clear();

        for (int i = 0; i < marchingCubes.mb.vertices.Count; i++)
        {
            TVector3 vert = marchingCubes.mb.vertices[i];
            vertBuffer.Add(new Vector3(vert.X, vert.Y, vert.Z));
        }

        m.Clear();
        m.SetVertices(vertBuffer);
        m.SetTriangles(marchingCubes.mb.triangles, 0);
        m.RecalculateNormals();

        mf.sharedMesh = m;
        mc.sharedMesh = m;
    }

    public void BuildAsync()
    {
        if (!performTriangulation)
            return;

        var msa = Observable.Start(() =>
        {
            marchingCubes.CreateMesh(field);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            return 1;
        });

        Observable.WhenAll(msa).ObserveOnMainThread().Subscribe(x =>
        {
            ConvertMesh();
        });
    }

    public void ConvertMesh()
    {
        vertBuffer.Clear();

        for (int i = 0; i < marchingCubes.mb.vertices.Count; i++)
        {
            TVector3 vert = marchingCubes.mb.vertices[i];
            vertBuffer.Add(new Vector3(vert.X, vert.Y, vert.Z));
        }

        m.Clear();
        m.SetVertices(vertBuffer);
        m.SetTriangles(marchingCubes.mb.triangles, 0);
        m.RecalculateNormals();

        mf.sharedMesh = m;
        mc.sharedMesh = m;
    }

    public void Clear()
    {
        vertBuffer.Clear();
        m.Clear();
    }

    public void Debug()
    {
        Vector3 center = new Vector3(x * s + s / 2, y * s + s / 2, z * s + s / 2);
        Vector3 size = new Vector3(s, s, s);

        if(performTriangulation)
            Gizmos.DrawWireCube(center, size);
    }
}

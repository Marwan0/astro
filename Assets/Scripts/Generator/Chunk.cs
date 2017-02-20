using UnityEngine;
using System;
using System.Collections.Generic;

using UniRx;

public class Chunk : IChunk
{
    //--Obj Ref--
    private GameObject obj;

    //--Scalar Field--
    private float[,,] field;

    //--Neighbour Chunks--
    private List<IChunk> neighbourChunks;

    //--Generator--
    private IGenerator generator;

    //--Triangulation--
    private MarchingCubes marchingCubes;

    //--Mesh--
    private List<Vector3> vertBuffer;
    private Mesh m;
    private MeshFilter mf;
    private MeshRenderer mr;
    private MeshCollider mc;

    //--Private Stuff--
    private ChunkSettings set;

    //==========Public Structs==========

    public struct ChunkSettings
    {
        public int w, h, d, x, y, z, s;

        //==========Constructor==========

        public ChunkSettings(int w, int h, int d, int x, int y, int z, int s)
        {
            this.w = w;
            this.h = h;
            this.d = d;
            this.x = x;
            this.y = y;
            this.z = z;
            this.s = s;
        }
    }

    //==========Constructor==========

    public Chunk(IGenerator generator, Transform par, int w, int h, int d, int x, int y, int z, int s, Material mat, int layerMask)
    {
        obj = new GameObject(string.Format("{0} / {1} / {2}", x, y, z), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        obj.transform.position = new Vector3(x * s, y * s, z * s);
        obj.transform.parent = par;
        obj.layer = layerMask;

        neighbourChunks = new List<IChunk>();

        field = new float[s, s, s];

        this.generator = generator;
        marchingCubes = new MarchingCubes(0.5f, new int[] { 0, 1, 2 });

        vertBuffer = new List<Vector3>();

        m = new Mesh();
        mf = obj.GetComponent<MeshFilter>();
        mr = obj.GetComponent<MeshRenderer>();
        mc = obj.GetComponent<MeshCollider>();

        m.Clear();
        mf.sharedMesh = null;
        mr.material = mat;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        mc.sharedMesh = null;

        set = new ChunkSettings(w, h, d, x, y, z, s);
    }

    //==========Private Methods==========

    private void ConvertMesh()
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

    //==========IChunk Implementation==========

    public void SetChunkNeighbours()
    {
        for(int nX = set.x - 1; nX <= set.x + 1; nX++)
            for(int nY = set.y - 1; nY <= set.y + 1; nY++)
                for(int nZ = set.z - 1; nZ <= set.z + 1; nZ++)
                {
                    if(nX > 0 && nX < set.w)
                        if(nY > 0 && nY < set.h)
                            if(nZ > 0 && nZ < set.d)
                            {
                                neighbourChunks.Add(generator.GetChunks()[nX + nY * set.w + nZ * set.w * set.h]);
                            }
                }
    }

    public void SetMarchingCubesNeighbours()
    {
        IChunk neighbourX = null;
        IChunk neighbourY = null;
        IChunk neighbourZ = null;
        IChunk neighbourXY = null;
        IChunk neighbourXZ = null;
        IChunk neighbourZY = null;
        IChunk neighbourXYZ = null;

        if (set.x + 1 < set.w) neighbourX = generator.GetChunks()[(set.x + 1) + set.y * set.w + set.z * set.w * set.h];
        if (set.y + 1 < set.h) neighbourY = generator.GetChunks()[set.x + (set.y + 1) * set.w + set.z * set.w * set.h];
        if (set.z + 1 < set.d) neighbourZ = generator.GetChunks()[set.x + set.y * set.w + (set.z + 1) * set.w * set.h];

        if (set.x + 1 < set.w && set.y + 1 < set.h) neighbourXY = generator.GetChunks()[(set.x + 1) + (set.y + 1) * set.w + set.z * set.w * set.h];
        if (set.x + 1 < set.w && set.z + 1 < set.d) neighbourXZ = generator.GetChunks()[(set.x + 1) + set.y * set.w + (set.z + 1) * set.w * set.h];
        if (set.z + 1 < set.d && set.y + 1 < set.h) neighbourZY = generator.GetChunks()[set.x + (set.y + 1) * set.w + (set.z + 1) * set.w * set.h];

        if (set.x + 1 < set.w && set.y + 1 < set.h && set.z + 1 < set.d) neighbourXYZ = generator.GetChunks()[(set.x + 1) + (set.y + 1) * set.w + (set.z + 1) * set.w * set.h];

        marchingCubes.SetNeighbourChunks(neighbourX, neighbourY, neighbourZ, neighbourXY, neighbourXZ, neighbourZY, neighbourXYZ);
    }

    public void Generate(IScalar callback)
    {
        field = new float[set.s, set.s, set.s];
        callback.OnScalarGeneration(field, set.w, set.h, set.d, set.x * set.s, set.y * set.s, set.z * set.s, set.s);
    }

    public GameObject GetObject()
    {
        return obj;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(set.x * set.s, set.y * set.s, set.z * set.s);
    }

    ChunkSettings IChunk.GetSettings()
    {
        return set;
    }

    public float[,,] GetField()
    {
        return field;
    }

    public void SetField(float[,,] field)
    {
        this.field = field;
    }

    public List<IChunk> GetNeighbours()
    {
        return neighbourChunks;
    }

    public void Triangulate()
    {
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

    public void TriangulateAsync()
    {
        IObservable<int> msa = Observable.Start(() =>
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

    public void Clear()
    {
        vertBuffer.Clear();
        m.Clear();
    }

    public void Gizmos()
    {
        Vector3 center = new Vector3(set.x * set.w + set.s / 2, set.y * set.h + set.s / 2, set.z * set.d + set.s / 2);
        Vector3 size = new Vector3(set.s, set.s, set.s);

        UnityEngine.Gizmos.DrawWireCube(center, size);
    }
}
using UnityEngine;
using System.Diagnostics;

public class Planet : MonoBehaviour
{
    //--Debug--
    public bool debugMode;
    public bool showField, showChunks;
    public bool debugChunks;

    //--Main Switches--
    public bool enableFog;

    //--Main Stuff--
    public Transform player, fog;
    public Material mat;
    public int layerMask;
    public int num, size;
    public int startHeight;
    private AnimationCurve globalHeightFunction;
    private Vector3 c;

    //--Noise--
    private FastNoise noise;
    public string seed;
    public float frq;
    public int oct;
    public float lac;
    public float gain;
    public float amp;
    public FastNoise.NoiseType noiseType;
    public FastNoise.Interp interp;
    public FastNoise.FractalType fractalType;
    public FastNoise.CellularDistanceFunction cellDistFunc;
    public FastNoise.CellularReturnType cellReturnType;

    //--Gravity--
    public float gravity;

    //--Chunks--
    public Chunk[] chunks;

    //--Diagnostics--
    private Stopwatch watch;

    //==========Unity Internal==========

    void Start()
    {
        int m = (num / 2) * size + (size / 2);
        c = new Vector3(m, m, m);

        if(enableFog)
        {
            fog.position = c;
            fog.localScale = new Vector3(num * size, num * size, num * size);
        }

        chunks = new Chunk[num * num * num];

        //Create Chunks
        for (int x = 0; x < num; x++)
            for (int y = 0; y < num; y++)
                for (int z = 0; z < num; z++)
                    chunks[x + y * num + z * num * num] = new Chunk(this, transform, num, size, x, y, z, mat, layerMask);

        //Setup Chunk Neighbours
        for (int x = 0; x < num; x++)
            for (int y = 0; y < num; y++)
                for (int z = 0; z < num; z++)
                {
                    chunks[x + y * num + z * num * num].SetNeighbourChunks();
                    chunks[x + y * num + z * num * num].SetMarchingCubesNeighbours();
                }

        watch = new Stopwatch();
    }

    void OnDrawGizmos()
    {
        if (!debugMode)
            return;

        if (showField)
        {
            Gizmos.color = Color.yellow;

            Vector3 pF = new Vector3(num * size / 2, num * size / 2, num * size / 2);
            Vector3 sF = new Vector3(num * size, num * size, num * size);

            Gizmos.DrawWireCube(pF, sF);
        }

        if (showChunks)
        {
            Gizmos.color = Color.cyan;

            for (int x = 0; x < num; x++)
                for (int y = 0; y < num; y++)
                    for (int z = 0; z < num; z++)
                    {
                        Vector3 pC = new Vector3(x * size + (size / 2), y * size + (size / 2), z * size + (size / 2));
                        Vector3 sC = new Vector3(size, size, size);

                        Gizmos.DrawWireCube(pC, sC);
                    }
        }

        if (debugChunks && chunks != null)
        {
            Gizmos.color = Color.red;

            foreach (Chunk chunk in chunks)
                chunk.Debug();
        }
    }

    //==========Public Methods==========

    public void Generate()
    {
        noise = new FastNoise(GetSeed());
        noise.SetFrequency(frq);
        noise.SetFractalOctaves(oct);
        noise.SetFractalLacunarity(lac);
        noise.SetFractalGain(gain);
        noise.SetNoiseType(noiseType);
        noise.SetInterp(interp);
        noise.SetFractalType(fractalType);
        noise.SetCellularDistanceFunction(cellDistFunc);
        noise.SetCellularReturnType(cellReturnType);

        watch.Start();

        if(chunks != null)
            foreach (Chunk chunk in chunks)
                chunk.Generate();

        watch.Stop();
        UnityEngine.Debug.Log(string.Format("Noise Generation: {0}ms", watch.ElapsedMilliseconds));
        watch.Reset();
    }

    public void Build()
    {
        watch.Start();

        if (chunks != null)
            foreach (Chunk chunk in chunks)
                chunk.Build();

        watch.Stop();
        UnityEngine.Debug.Log(string.Format("Triangulation: {0}ms", watch.ElapsedMilliseconds));
        watch.Reset();
    }

    public void BuildAsync()
    {
        if (chunks != null)
            foreach (Chunk chunk in chunks)
                chunk.BuildAsync();
    }

    public void Clear()
    {
        if (chunks != null)
            foreach (Chunk chunk in chunks)
                chunk.Clear();
    }

    public void GenerateEmptySurface(Chunk chunk)
    {
        for (int x = 0; x < chunk.field.GetLength(0); x++)
            for (int y = 0; y < chunk.field.GetLength(1); y++)
                for (int z = 0; z < chunk.field.GetLength(2); z++)
                {
                    Vector3 p = chunk.GetPosition() + new Vector3(x, y, z);
                    float d = (c - p).magnitude;
                    float t = 1f - 1f / (1f + d);

                    float n = noise.GetNoise(p.x, p.y, p.z) * amp;
                    globalHeightFunction = AnimationCurve.Linear(0, startHeight + n, 1, 0);
                    float vol = globalHeightFunction.Evaluate(t);
          
                    chunk.field[x, y, z] = vol;
                }
    }

    public void Attract(Rigidbody rb)
    {
        Vector3 gravityUp = (rb.position - c).normalized;
        Vector3 localUp = rb.transform.up;

        rb.AddForce(gravityUp * gravity);
        rb.rotation = Quaternion.FromToRotation(localUp, gravityUp) * rb.rotation;
    }

    public Chunk GetChunkByGameObject(GameObject obj)
    {
        foreach (Chunk chunk in chunks)
            if (chunk.obj.Equals(obj))
                return chunk;

        return null;
    }

    //==========Private Methods==========

    private int GetSeed()
    {
        return (seed == "") ? Random.Range(int.MinValue, int.MaxValue) : seed.GetHashCode();
    }
}
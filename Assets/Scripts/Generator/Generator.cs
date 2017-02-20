using UnityEngine;

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class Generator : IGenerator
{
    //--Constants--
    private static readonly string PATH = "Assets/ScalarData/Generators/";
    private static readonly string EXTENSION = ".gen";

    //--Settings--
    [SerializeField] private Shape shape;
    private Settings settings;

    //--Chunks--
    [NonSerialized] public IChunk[] chunks;
    [SerializeField] private Dictionary<int, float[,,]> chunkFields;

    //==========Public Classes==========

    [Serializable]
    public class Shape
    {
        //--Main Settings--
        public int width, height, depth, size;

        //==========Constructor==========

        public Shape(int w, int h, int d, int s)
        {
            width = w;
            height = h;
            depth = d;
            size = s;
        }
    }

    [Serializable]
    public class Settings
    {
        //--Main Settings--
        [NonSerialized] public Transform parent;
        [NonSerialized] public Material material;
        [NonSerialized] public int mask;

        //==========Constructor==========

        public Settings(Transform parent, Material material, int mask)
        {
            this.parent = parent;
            this.material = material;
            this.mask = mask;
        }
    }

    //==========Constructors==========

    public Generator()
    {
        //Requires empty constructor
    }

    //==========Static Methods==========

    /// <summary>
    /// Load a "WorldGenerator" and it's corresponding fields.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static Generator Load(string fileName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        byte[] worldGeneratorBytes = File.ReadAllBytes(PATH + fileName + EXTENSION);

        using (MemoryStream stream = new MemoryStream())
        {
            stream.Write(worldGeneratorBytes, 0, worldGeneratorBytes.Length);
            stream.Position = 0;

            return (Generator)formatter.Deserialize(stream);
        }
    }

    //===========IGenerator Implementation==========

    public void SetSettings(Shape shape, Settings settings)
    {
        if (shape != null)
            this.shape = shape;

        if(settings != null)
            this.settings = settings;
    }

    public void InitGenerator()
    {
        chunks = new IChunk[shape.width * shape.height * shape.depth];

        for (int x = 0; x < shape.width; x++)
            for (int y = 0; y < shape.height; y++)
                for (int z = 0; z < shape.depth; z++)
                {
                    int idx = x + y * shape.width + z * shape.width * shape.height;

                    chunks[idx] = new Chunk(this, settings.parent, shape.width, shape.height, shape.depth, x, y, z, shape.size, settings.material, settings.mask);
                }

        for (int x = 0; x < shape.width; x++)
            for (int y = 0; y < shape.height; y++)
                for (int z = 0; z < shape.depth; z++)
                {
                    int idx = x + y * shape.width + z * shape.width * shape.height;

                    chunks[idx].SetChunkNeighbours();
                    chunks[idx].SetMarchingCubesNeighbours();
                }
    }

    public void GenerateFields(IScalar callback)
    {
        foreach (IChunk chunk in chunks)
            chunk.Generate(callback);
    }

    public void Triangulate()
    {
        foreach (IChunk chunk in chunks)
            chunk.Triangulate();
    }

    public void TriangulateAsync()
    {
        foreach (IChunk chunk in chunks)
            chunk.TriangulateAsync();
    }

    public void Clear()
    {
        foreach (IChunk chunk in chunks)
            chunk.Clear();
    }

    public void RegisterTools(params ITool[] tools)
    {
        foreach (ITool tool in tools)
            tool.SetGenerator(this);
    }

    public IChunk GetChunkByGameObject(GameObject obj)
    {
        foreach (IChunk chunk in chunks)
            if (chunk.GetObject().Equals(obj))
                return chunk;

        return null;
    }

    public IChunk[] GetChunks()
    {
        return chunks;
    }

    public void Save(string fileName)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        byte[] worldGeneratorBytes = null;

        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, this);
            worldGeneratorBytes = stream.ToArray();
        }

        File.WriteAllBytes(PATH + fileName + EXTENSION, worldGeneratorBytes);
    }

    public void PrepareChunksForSaving()
    {
        chunkFields = new Dictionary<int, float[,,]>();

        foreach (IChunk chunk in chunks)
        {
            int idx = chunk.GetSettings().x + chunk.GetSettings().y * shape.width + chunk.GetSettings().z * shape.width * shape.height;
            chunkFields.Add(idx, chunk.GetField());
        }
    }

    public void PrepareChunksFromLoading()
    {
        if (chunkFields == null)
            return;

        foreach (IChunk chunk in chunks)
        {
            int idx = chunk.GetSettings().x + chunk.GetSettings().y * shape.width + chunk.GetSettings().z * shape.width * shape.height;
            float[,,] field = null;

            if (chunkFields.TryGetValue(idx, out field))
                if (field != null)
                    chunk.SetField(field);
        }
    }

    public void Gizmos(bool showField, bool showChunks)
    {
        float offset = (shape.size % 2 == 0) ? 0f : 0.5f;

        if (showField)
        {
            UnityEngine.Gizmos.color = Color.yellow;

            Vector3 pF = new Vector3(shape.width * shape.size / 2 + offset, shape.height * shape.size / 2 + offset, shape.depth * shape.size / 2 + offset);
            Vector3 sF = new Vector3(shape.width * shape.size, shape.height * shape.size, shape.depth * shape.size);

            UnityEngine.Gizmos.DrawWireCube(pF, sF);
        }

        if (showChunks)
        {
            UnityEngine.Gizmos.color = Color.cyan;

            for (int x = 0; x < shape.width; x++)
                for (int y = 0; y < shape.height; y++)
                    for (int z = 0; z < shape.depth; z++)
                    {
                        Vector3 pC = new Vector3(x * shape.size + (shape.size / 2) + offset, y * shape.size + (shape.size / 2) + offset, z * shape.size + (shape.size / 2) + offset);
                        Vector3 sC = new Vector3(shape.size, shape.size, shape.size);

                        UnityEngine.Gizmos.DrawWireCube(pC, sC);
                    }
        }
    }
}
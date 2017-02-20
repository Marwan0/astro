using UnityEngine;

public class Test : MonoBehaviour, IScalar
{
    //--Main Settings--
    public Material material;
    public Tool tool;

    //--Noise--
    public AnimationCurve brush;
    private FastNoise noise;
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

    //--Generators--
    public IGenerator generator;

    //==========Unity Internal==========

    void Start()
    {
        noise = new FastNoise(Random.Range(int.MinValue, int.MaxValue));
        noise.SetFrequency(frq);
        noise.SetFractalOctaves(oct);
        noise.SetFractalLacunarity(lac);
        noise.SetFractalGain(gain);
        noise.SetNoiseType(noiseType);
        noise.SetInterp(interp);
        noise.SetFractalType(fractalType);
        noise.SetCellularDistanceFunction(cellDistFunc);
        noise.SetCellularReturnType(cellReturnType);

        Generator.Shape cube = new Generator.Shape(10, 10, 10, 16);
        Generator.Shape rect = new Generator.Shape(5, 20, 5, 16);
        Generator.Shape flat = new Generator.Shape(10, 5, 10, 16);

        Generator.Settings settings = new Generator.Settings(transform, material, 8);

        generator = new Generator();
        generator.SetSettings(flat, settings);
        generator.InitGenerator();
        generator.RegisterTools(tool);

        //generator.PrepareChunksFromLoading();

        generator.GenerateFields(this);
        generator.Triangulate();
        //generator.TriangulateAsync();

        //generator.PrepareChunksForSaving();
        //generator.Save("flat");
    }

    void OnDrawGizmos()
    {
        if (generator != null)
            generator.Gizmos(true, false);
    }

    //==========IScalar Implementation============

    public void OnScalarGeneration(float[,,] field, int w, int h, int d, int x, int y, int z, int s)
    {
        for(int currX = 0; currX < s; currX++)
            for(int currY = 0; currY < s; currY++)
                for(int currZ = 0; currZ < s; currZ++)
                {
                    float t = (float)(y + currY) / (h * s);
                    float b = brush.Evaluate(t);
                    float n = Mathf.Abs(noise.GetNoise(x + currX, y + currY, z + currZ)) * amp;

                    field[currX, currY, currZ] = n - b;
                }
    }
}
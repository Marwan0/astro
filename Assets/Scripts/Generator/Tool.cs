using System;
using UnityEngine;

public class Tool : MonoBehaviour, ITool
{
    //--Main Settings--
    public LayerMask mask;
    public float distance;
    public float range;
    public float strenght;
    public AnimationCurve brush;

    //--Generators--
    private IGenerator generator;

    //==========Unity Internal==========

    void Update()
    {
        if (Input.GetMouseButton(0))
            AddTerrain();

        if (Input.GetMouseButton(1))
            SubtractTerrain();
    }

    //==========Private Methods==========

    private void AddTerrain()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance, mask))
        {
            IChunk sourceChunk = generator.GetChunkByGameObject(hit.transform.gameObject);

            if (sourceChunk != null)
            {
                Chunk.ChunkSettings set = sourceChunk.GetSettings();

                foreach (IChunk chunk in sourceChunk.GetNeighbours())
                {
                    bool performTriangulation = false;

                    for (int x = 0; x < set.s; x++)
                        for (int y = 0; y < set.s; y++)
                            for (int z = 0; z < set.s; z++)
                            {
                                float deltaField = ((chunk.GetPosition() + new Vector3(x, y, z)) - hit.point).magnitude;
                                float t = 1f - 1f / (1f + deltaField);
                                float vol = brush.Evaluate(t) * strenght;

                                if (deltaField < range)
                                {
                                    chunk.GetField()[x, y, z] += vol;

                                    if (chunk.GetField()[x, y, z] >= 0.5f)
                                        performTriangulation = true;
                                }
                            }

                    if (performTriangulation)
                        chunk.Triangulate();
                }
            }
        }
    }

    private void SubtractTerrain()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance, mask))
        {
            IChunk sourceChunk = generator.GetChunkByGameObject(hit.transform.gameObject);

            if (sourceChunk != null)
            {
                Chunk.ChunkSettings set = sourceChunk.GetSettings();

                foreach (IChunk chunk in sourceChunk.GetNeighbours())
                {
                    bool performTriangulation = false;

                    for (int x = 0; x < set.s; x++)
                        for (int y = 0; y < set.s; y++)
                            for (int z = 0; z < set.s; z++)
                            {
                                float deltaField = ((chunk.GetPosition() + new Vector3(x, y, z)) - hit.point).magnitude;
                                float t = 1f - 1f / (1f + deltaField);
                                float vol = brush.Evaluate(t) * strenght;

                                if (deltaField < range)
                                {
                                    chunk.GetField()[x, y, z] -= vol;

                                    if (chunk.GetField()[x, y, z] >= 0.5f)
                                        performTriangulation = true;
                                }
                            }

                    if (performTriangulation)
                        chunk.Triangulate();
                }
            }
        }
    }

    //==========ITool Implementation==========

    public void SetGenerator(IGenerator generator)
    {
        this.generator = generator;
    }
}

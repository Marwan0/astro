using UnityEngine;
using System.Collections.Generic;

public class Tool : MonoBehaviour
{
    //--Main Stuff--
    public Planet planet;
    public LayerMask mask;

    //--Tool Settings--
    public float distance;
    public float range;
    public float strenght;
    public AnimationCurve brush;

    //--Job Buffer--
    List<Chunk> jobQueue;

    //--Other Stuff--
    private Chunk sourceChunk;

    //==========Unity Internal==========

    void Start()
    {
        jobQueue = new List<Chunk>();
    }

    void Update()
    {
        ProcessJobs();

        if (Input.GetMouseButton(0))
            AddTerrain();

        if (Input.GetMouseButton(1))
            SubtractTerrain();

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            sourceChunk = null;
    }

    //==========Private Methods==========

    private void ProcessJobs()
    {
        //UnityEngine.Debug.Log(jobQueue.Count);

        if(jobQueue.Count > 0)
        {
            Chunk currentChunk = jobQueue[0];
            jobQueue.RemoveAt(0);

            currentChunk.Build();
        }
    }

    private void AddTerrain()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            sourceChunk = planet.GetChunkByGameObject(hit.transform.gameObject);

            if (sourceChunk != null)
            {
                int size = sourceChunk.field.GetLength(0);

                foreach (Chunk chunk in sourceChunk.neighbourChunks)
                {
                    bool performTriangulation = false;

                    for (int x = 0; x < size; x++)
                        for (int y = 0; y < size; y++)
                            for (int z = 0; z < size; z++)
                            {
                                float deltaField = ((chunk.GetPosition() + new Vector3(x, y, z)) - hit.point).magnitude;
                                float t = 1f - 1f / (1f + deltaField);
                                float vol = brush.Evaluate(t) * strenght;

                                if (deltaField < range)
                                {
                                    chunk.field[x, y, z] += vol;

                                    if (chunk.field[x, y, z] >= 0.5f)
                                        performTriangulation = true;
                                }
                            }

                    chunk.performTriangulation = performTriangulation;
                    chunk.Build();
                }
            }
        }
    }

    private void SubtractTerrain()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
        {
            sourceChunk = planet.GetChunkByGameObject(hit.transform.gameObject);

            if (sourceChunk != null)
            {
                int size = sourceChunk.field.GetLength(0);

                foreach (Chunk chunk in sourceChunk.neighbourChunks)
                {
                    bool performTriangulation = false;

                    for (int x = 0; x < size; x++)
                        for (int y = 0; y < size; y++)
                            for (int z = 0; z < size; z++)
                            {
                                float deltaField = ((chunk.GetPosition() + new Vector3(x, y, z)) - hit.point).magnitude;
                                float t = 1f - 1f / (1f + deltaField);
                                float vol = brush.Evaluate(t) * strenght;

                                if (deltaField < range)
                                {
                                    chunk.field[x, y, z] -= vol;

                                    if (chunk.field[x, y, z] >= 0.5f)
                                        performTriangulation = true;
                                }
                            }

                    chunk.performTriangulation = performTriangulation;
                    chunk.Build();
                }
            }
        }
    }
}

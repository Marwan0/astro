using UnityEngine;
using System.Collections.Generic;

public interface IChunk
{
    /// <summary>
    /// Stores all the neighbour chunks into the current chunk.
    /// </summary>
    void SetChunkNeighbours();

    /// <summary>
    /// Stores all valid marching cubes neighbours.
    /// </summary>
    void SetMarchingCubesNeighbours();

    /// <summary>
    /// Will execute the scall field callback.
    /// </summary>
    /// <param name="callback"></param>
    void Generate(IScalar callback);

    /// <summary>
    /// Get the chunk game object reference.
    /// </summary>
    /// <returns></returns>
    GameObject GetObject();

    /// <summary>
    /// Get the apsolute chunk position.
    /// </summary>
    /// <returns></returns>
    Vector3 GetPosition();

    /// <summary>
    /// Get some basic information of the chunk.
    /// </summary>
    /// <returns></returns>
    Chunk.ChunkSettings GetSettings();

    /// <summary>
    /// Get the chunk field.
    /// </summary>
    /// <returns></returns>
    float[,,] GetField();

    /// <summary>
    /// Set the chunk field.
    /// </summary>
    /// <param name="field"></param>
    void SetField(float[,,] field);

    /// <summary>
    /// Get all chunk neighbours.
    /// </summary>
    /// <returns></returns>
    List<IChunk> GetNeighbours();

    /// <summary>
    /// Performe marching cubes.
    /// </summary>
    void Triangulate();

    /// <summary>
    /// Performe marching cubes async. Requires "MeshBuffer" for vertex convertion.
    /// </summary>
    void TriangulateAsync();

    /// <summary>
    /// Will clea the mesh and the mesh buffer.
    /// </summary>
    void Clear();

    /// <summary>
    /// Will draw the some gizmos.
    /// </summary>
    void Gizmos();
}

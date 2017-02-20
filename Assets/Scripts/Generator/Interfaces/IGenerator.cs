using UnityEngine;

public interface IGenerator
{
    /// <summary>
    /// Will set settings to the world generator.
    /// </summary>
    /// <param name="shape">The current shape. (Can be NULL because it will be serialized)</param>
    /// <param name="settings">The current settings.</param>
    void SetSettings(Generator.Shape shape, Generator.Settings settings);

    /// <summary>
    /// Tell the world generator to initialize their chunks.
    /// </summary>
    void InitGenerator();

    /// <summary>
    /// Tell the chunks to generate their scalar data.
    /// This will override the hold field data.
    /// </summary>
    /// <param name="callback">Define what happens when the scalar field is generating</param>
    void GenerateFields(IScalar callback);

    /// <summary>
    /// Tell the chunks to perform marching cubes.
    /// </summary>
    void Triangulate();

    /// <summary>
    /// Tell the chunks to perform marching cubes asynchronously.
    /// </summary>
    void TriangulateAsync();

    /// <summary>
    /// Tell the chunks to clear their mesh and mesh buffer.
    /// </summary>
    void Clear();

    /// <summary>
    /// Tell the world generator there is a tool that can modify the terrain.
    /// </summary>
    /// <param name="tool"></param>
    void RegisterTools(params ITool[] tools);

    /// <summary>
    /// Finds a specific chunk with the chunk gameobject reference.
    /// </summary>
    /// <param name="obj"></param>
    IChunk GetChunkByGameObject(GameObject obj);

    IChunk[] GetChunks();

    /// <summary>
    /// Will serialize the whole class "WorldGenerator".
    /// </summary>
    /// <param name="fileName">The name of the serialized file.</param>
    void Save(string fileName);

    /// <summary>
    /// Convert the chunk fields into serializeable dictionary.
    /// </summary>
    void PrepareChunksForSaving();

    /// <summary>
    /// Put the chunk fields from the serializeable dictionary into the chunks.
    /// </summary>
    void PrepareChunksFromLoading();

    /// <summary>
    /// Will draw the gizmos in scene view.
    /// </summary>
    /// <param name="showField">Will draw the hole field.</param>
    /// <param name="showChunks">Will draw all chunks.</param>
    void Gizmos(bool showField, bool showChunks);
}

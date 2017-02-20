using UnityEngine;

public interface IScalar
{
    /// <summary>
    /// This will be called when every chunk is generating the scalar field.
    /// </summary>
    /// <param name="field">The current chunk field</param>
    /// <param name="w">World generator width.</param>
    /// <param name="h">World generator height.</param>
    /// <param name="d">World generator depth.</param>
    /// <param name="x">Current chunk x position. (Non real number)</param>
    /// <param name="y">Current chunk y position. (Non real number)</param>
    /// <param name="z">Current chunk z position. (Non real number)</param>
    /// <param name="s">Current chunk size.</param>
    void OnScalarGeneration(float[,,] field, int w, int h, int d, int x, int y, int z, int s);
}
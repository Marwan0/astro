using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Converter : MonoBehaviour
{
    //--Scanned Object--
    public GameObject obj;

    //--Main Settings--
    public bool enableScannerX, enableScannerZ;
    public int width, height, depth;
    public float scannerSpeed;

    //--Scanners--
    public GameObject scannerX, scannerZ;

    //--Scannera Targets--
    private Vector3 scannerXTarget, scannerZTarget;

    //==========Unity Internal==========

	void Start()
    {
        
	}
	
	void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Generate(128);

        if(enableScannerX)
        {
            if (scannerX.transform.position.x <= 0.1f)
                scannerXTarget = new Vector3(width, height / 2, depth / 2);

            if (scannerX.transform.position.x >= width - 0.1f)
                scannerXTarget = new Vector3(0, height / 2, depth / 2);

            scannerX.transform.position = Vector3.Lerp(scannerX.transform.position, scannerXTarget, Time.deltaTime * scannerSpeed);
        }

        if(enableScannerZ)
        {
            if (scannerZ.transform.position.z <= 0.1f)
                scannerZTarget = new Vector3(width / 2, height / 2, depth);

            if (scannerZ.transform.position.z >= depth - 0.1f)
                scannerZTarget = new Vector3(width / 2, height / 2, 0);

            scannerZ.transform.position = Vector3.Lerp(scannerZ.transform.position, scannerZTarget, Time.deltaTime * scannerSpeed);
        }
    }

    void OnDrawGizmos()
    {
        Vector3 center = new Vector3(width / 2, height / 2, depth / 2);
        Vector3 size = new Vector3(width, height, depth);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }

    //==========Private Methods==========

    private void Generate(int size)
    {
        Texture2D tex = new Texture2D(size, size);

        float dThreshold = 7f;
        float zThreshold = 7f;
        float z = scannerZ.transform.position.z;

        Mesh m = obj.GetComponent<MeshFilter>().sharedMesh;

        Vector3[] verticesBuffer = m.vertices;
        int[] indicesBuffer = m.triangles;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                tex.SetPixel(x, y, Color.white);
            }

        for (int x = 0; x < size; x++)
            for(int y = 0; y < size; y++)
            {
                float xN = 1f - 1f / (1f + x);
                float yN = 1f - 1f / (1f + y);

                xN *= 16f;
                yN *= 16f;

                for(int i = 0; i < indicesBuffer.Length; i += 3)
                {
                    Vector3 p0 = obj.transform.position + verticesBuffer[indicesBuffer[i + 0]];
                    Vector3 p1 = obj.transform.position + verticesBuffer[indicesBuffer[i + 1]];
                    Vector3 p2 = obj.transform.position + verticesBuffer[indicesBuffer[i + 2]];

                    Vector3 curr = new Vector3(xN, yN, z);

                    float d0 = (curr - p0).magnitude;
                    float d1 = (curr - p1).magnitude;
                    float d2 = (curr - p2).magnitude;

                    if (d0 < dThreshold || d1 < dThreshold || d2 < dThreshold)
                        tex.SetPixel(x, y, Color.black);

                    /*if (z >= p0.z - zThreshold || z <= p0.z + zThreshold)
                    {
                        float d = (new Vector2(xN, yN) - (Vector2)p0).magnitude;
                        if (d > dThreshold) tex.SetPixel(x, y, Color.black);

                        tex.SetPixel(x, y, Color.black);
                    }

                    if (z >= p1.z - zThreshold || z <= p1.z + zThreshold)
                    {
                        float d = (new Vector2(xN, yN) - (Vector2)p1).magnitude;
                        if (d > dThreshold) tex.SetPixel(x, y, Color.black);

                        tex.SetPixel(x, y, Color.black);
                    }

                    if (z >= p2.z - zThreshold || z <= p2.z + zThreshold)
                    {
                        float d = (new Vector2(xN, yN) - (Vector2)p2).magnitude;
                        if (d > dThreshold) tex.SetPixel(x, y, Color.black);

                        tex.SetPixel(x, y, Color.black);
                    }*/
                }
            }

        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        scannerZ.GetComponent<MeshRenderer>().material.SetTexture(Shader.PropertyToID("_MainTex"), tex);
    }
}

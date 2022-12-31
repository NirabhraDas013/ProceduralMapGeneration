using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProceduralTerrain
{
    [RequireComponent(typeof(NoiseMapGeneration))]
    public class TileGeneration : MonoBehaviour
    {
        [SerializeField]
        private NoiseMapGeneration noiseMapGeneration;
        [SerializeField]
        private MeshRenderer tileMeshRenderer;
        [SerializeField]
        private MeshFilter tileMeshFilter;
        [SerializeField]
        private MeshCollider tileMeshCollider;
        [SerializeField]
        private float levelScale;
        [SerializeField]
        private float heightMultiplier;
        [SerializeField]
        private AnimationCurve heightCurve;

        [Space]

        [SerializeField] [Tooltip("Set The Terrain Types in order of Height. Lowest first")]
        private List<TerrainType> terrainTypes;

        [Space]

        [SerializeField]
        private List<Wave> waves;

        private void Start()
        {
            //Just in case I forget to assign in the inspector
            if (noiseMapGeneration == null) noiseMapGeneration = gameObject.GetComponent<NoiseMapGeneration>();
            if (tileMeshRenderer == null) tileMeshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (tileMeshFilter == null) tileMeshFilter = gameObject.GetComponent<MeshFilter>();
            if (tileMeshCollider == null) tileMeshCollider = gameObject.GetComponent<MeshCollider>();

            GenerateTile();
        }

        private void GenerateTile()
        {
            //Calculate Tile Depth and Width based on Mesh Vertices
            Vector3[] meshVertices = tileMeshFilter.mesh.vertices;
            int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
            int tileWidth = tileDepth;

            //Calculate the offsets based on the tilePosition
            float offsetX = -gameObject.transform.position.x;
            float offsetZ = -gameObject.transform.position.z;

            //Generate Noise using the tile Depth, Width and mapScale
            float[,] noiseMap = noiseMapGeneration.GenerateNoiseMap(tileDepth, tileWidth, levelScale, offsetX, offsetZ, waves);

            //Generate a height Map using noise
            Texture2D tileTexture = BuildTexture(noiseMap);
            tileMeshRenderer.material.mainTexture = tileTexture;

            UpdateMeshVertices(noiseMap);
        }

        private Texture2D BuildTexture(float[,] noiseMap)
        {
            int tileDepth = noiseMap.GetLength(0);
            int tileWidth = noiseMap.GetLength(1);

            //Create a ColorMap for the Tile
            Color[] colorMap = new Color[tileDepth * tileWidth];
            for (int zIndex = 0; zIndex < tileDepth; zIndex++)
            {
                for (int xIndex = 0; xIndex < tileWidth; xIndex++)
                {
                    //transform the 2D mapIndex into an arrayIndex
                    int colorIndex = zIndex * tileWidth + xIndex;
                    float height = noiseMap[zIndex, xIndex];

                    //Choose a Terrain Type according to height value
                    TerrainType terrainType = ChooseTerrainType(height);

                    //Assign color to visualize height value -- Based on terrain according to height value
                    colorMap[colorIndex] = terrainType.color;
                }
            }

            //Create a new texture and set its pixel colors
            Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
            tileTexture.wrapMode = TextureWrapMode.Clamp;
            tileTexture.SetPixels(colorMap);
            tileTexture.Apply();

            return tileTexture;
        }

        private TerrainType ChooseTerrainType(float height)
        {
            //Check each terrain type, check if the heigh passed is below the type specified height
            foreach (var terrainType in terrainTypes)
            {
                //Return the first terrain type whose set height is higher than the passed height
                if (height < terrainType.height) return terrainType;
            }

            // If none matches, return the last one
            return terrainTypes[terrainTypes.Count - 1];
        }

        private void UpdateMeshVertices(float[,] heightMap)
        {
            int tileDepth = heightMap.GetLength(0);
            int tileWidth = heightMap.GetLength(1);

            Vector3[] meshVertices = tileMeshFilter.mesh.vertices;

            //Update vertex index of all heightMap coordinates
            int vertexIndex = 0;
            for (int zIndex = 0; zIndex < tileDepth; zIndex++)
            {
                for (int xIndex = 0; xIndex < tileWidth; xIndex++)
                {
                    float height = heightMap[zIndex, xIndex];

                    Vector3 vertex = meshVertices[vertexIndex];

                    //Adjust the vertex Y coordinate proportional to the height value adjested to multiplier
                    meshVertices[vertexIndex] = new Vector3(vertex.x, heightCurve.Evaluate(height) * heightMultiplier, vertex.z);

                    vertexIndex++;
                }
            }

            //Update and Aplly the changes to the vertices in the mesh
            tileMeshFilter.mesh.vertices = meshVertices;
            tileMeshFilter.mesh.RecalculateBounds();
            tileMeshFilter.mesh.RecalculateNormals();
            //Update the meshCollider ---- *IMPORTANT*
            tileMeshCollider.sharedMesh = tileMeshFilter.mesh;
        }
    }
}

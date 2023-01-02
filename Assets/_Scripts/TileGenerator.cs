using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProceduralTerrain
{
    [RequireComponent(typeof(NoiseMapGeneration))]
    public class TileGenerator : MonoBehaviour
    {
        [SerializeField]
        private NoiseMapGeneration noiseMapGeneration;
        [SerializeField]
        private MeshRenderer tileMeshRenderer;
        [SerializeField]
        private MeshFilter tileMeshFilter;
        [SerializeField]
        private MeshCollider tileMeshCollider;

        private LevelGenerator levelGeneratorInstance;

        private void Awake()
        {
            //Just in case I forget to assign in the inspector
            if (noiseMapGeneration == null) noiseMapGeneration = gameObject.GetComponent<NoiseMapGeneration>();
            if (tileMeshRenderer == null) tileMeshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (tileMeshFilter == null) tileMeshFilter = gameObject.GetComponent<MeshFilter>();
            if (tileMeshCollider == null) tileMeshCollider = gameObject.GetComponent<MeshCollider>();

            //if (LevelGenerator.instance != null)
            //{
                levelGeneratorInstance = LevelGenerator.instance;
            //}
        }

        public void GenerateTile(float centerVertexZ, float maxDistanceZ)
        {
            //Calculate Tile Depth and Width based on Mesh Vertices
            Vector3[] meshVertices = tileMeshFilter.mesh.vertices;
            int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
            int tileWidth = tileDepth;

            //Calculate the offsets based on the tilePosition
            float offsetX = -gameObject.transform.position.x;
            float offsetZ = -gameObject.transform.position.z;

            //Generate Perlin Noise using the tile Depth, Width and mapScale for landmass
            float[,] heightMap = noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, levelGeneratorInstance.LevelScale, offsetX, offsetZ, levelGeneratorInstance.HeightWaves);

            //Calculate the vertex offset based on tile position and distance between vertices
            Vector3 tileDimensions = tileMeshFilter.mesh.bounds.size;
            float distanceBetweenVertices = tileDimensions.z / (float)tileDepth;
            float vertexOffsetZ = gameObject.transform.position.z / distanceBetweenVertices;

            //Generate a NoiseMap using Uniform Noise for using as Heat Map
            float[,] uniformHeatMap = noiseMapGeneration.GenerateUniformNoiseMap(tileDepth, tileWidth, centerVertexZ, maxDistanceZ, vertexOffsetZ);

            //Generate a NoiseMap using Perlin Noise for combining with the Uniform Noise Map
            float[,] perlinHeatMap = noiseMapGeneration.GeneratePerlinNoiseMap(tileDepth, tileWidth, levelGeneratorInstance.LevelScale, offsetX, offsetZ, levelGeneratorInstance.HeatWaves);

            float[,] heatMap = new float[tileDepth, tileWidth];
            for (int zIndex = 0; zIndex < tileDepth; zIndex++)
            {
                for (int xIndex = 0; xIndex < tileWidth; xIndex++)
                {
                    //Mix both heat maps together by multiplying their values
                    heatMap[zIndex, xIndex] = uniformHeatMap[zIndex, xIndex] * perlinHeatMap[zIndex, xIndex];

                    //Make higher regions colder by adding the heigh value to the heat map
                    heatMap[zIndex, xIndex] += levelGeneratorInstance.HeatCurve.Evaluate(heightMap[zIndex, xIndex] * heightMap[zIndex, xIndex]);
                }
            }

            //Generate a Texture2D from the height Map
            Texture2D heightTexture = BuildTexture(heightMap, levelGeneratorInstance.HeightTerrainTypes);
            //Generate a Texture2D from the heat Map
            Texture2D heatTexture = BuildTexture(heatMap, levelGeneratorInstance.HeatTerrainTypes);

            switch (levelGeneratorInstance.VisualizationMode)
            {
                case VisualizationMode.HEIGHT:
                    //Set the level texture to be Height Texture
                    tileMeshRenderer.material.mainTexture = heightTexture;
                    break;
                case VisualizationMode.HEAT:
                    //Set the Level Texture to be Heat Texture
                    tileMeshRenderer.material.mainTexture = heatTexture;
                    break;
                default:
                    break;
            }

            //This one is only with heightMap as heatMap doe not change the landmass
            UpdateMeshVertices(heightMap);
        }

        private Texture2D BuildTexture(float[,] noiseMap, List<TerrainType> terrainTypes)
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
                    TerrainType terrainType = ChooseTerrainType(height, terrainTypes);

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

        private TerrainType ChooseTerrainType(float height, List<TerrainType> terrainTypes)
        {
            //Check each terrain type, check if the heigh passed is below the type specified height
            foreach (var terrainType in terrainTypes)
            {
                //Return the first terrain type whose set height is higher than the passed height
                if (height < terrainType.height) return terrainType;
            }

            // If none matches, return the last one
            return levelGeneratorInstance.HeightTerrainTypes[^1];
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
                    meshVertices[vertexIndex] = new Vector3(vertex.x, levelGeneratorInstance.HeightCurve.Evaluate(height) * levelGeneratorInstance.HeightMultiplier, vertex.z);

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

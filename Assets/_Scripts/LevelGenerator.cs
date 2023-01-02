using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralTerrain
{
    public class LevelGenerator : MonoBehaviour
    {
        public static LevelGenerator instance;

        [SerializeField]
        private VisualizationMode visualizationMode;
        public VisualizationMode VisualizationMode { get => visualizationMode; }
        [SerializeField]
        private int mapWidthInTiles;
        [SerializeField]
        private int mapDepthInTiles;
        [SerializeField]
        private float centerVertexZ;
        [SerializeField]
        private float maxDistanceZ;
        [SerializeField]
        private GameObject tilePrefab;
        [SerializeField]
        private float levelScale;
        public float LevelScale { get => levelScale; }

        [Space]
        [Header("Height")]
        [SerializeField]
        private float heightMultiplier;
        public float HeightMultiplier { get => heightMultiplier; }
        [SerializeField]
        private AnimationCurve heightCurve;
        public AnimationCurve HeightCurve { get => heightCurve; }

        [SerializeField]
        private AnimationCurve heatCurve;
        public AnimationCurve HeatCurve { get => heatCurve; }

        [Space]
        [Header("Terrain Types")]

        [SerializeField]
        [Tooltip("Set The Terrain Types in order of Height. Lowest first")]
        private List<TerrainType> heightTerrainTypes;
        public List<TerrainType> HeightTerrainTypes { get => heightTerrainTypes; }

        [Space]

        [SerializeField]
        [Tooltip("Set The Terrain Types in order of Temperature. Highest first")]
        private List<TerrainType> heatTerrainTypes;
        public List<TerrainType> HeatTerrainTypes { get => heatTerrainTypes; }

        [Space]

        [SerializeField]
        private List<Wave> heightWaves;
        public List<Wave> HeightWaves { get => heightWaves; }

        [Space]

        [SerializeField]
        private List<Wave> heatWaves;
        public List<Wave> HeatWaves { get => heatWaves; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void Start()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            CleanMap();

            //Get the tile dimensions from the prefab
            Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
            int tileWidth = (int)tileSize.x;
            int tileDepth = (int)tileSize.z;

            var levelRootPosition = gameObject.transform.position;

            for (int xTileIndex = 0; xTileIndex < mapWidthInTiles; xTileIndex++)
            {
                for (int zTileIndex = 0; zTileIndex < mapDepthInTiles; zTileIndex++)
                {
                    //Calculate the Tile Position based on X and Z indices
                    Vector3 tilePosition = new Vector3(levelRootPosition.x + xTileIndex * tileWidth, levelRootPosition.y, levelRootPosition.z + zTileIndex * tileDepth);

                    //Instantiate a new Tile
                    GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity, gameObject.transform) as GameObject;
                    //Generate the Tile texture
                    tile.GetComponent<TileGenerator>().GenerateTile(centerVertexZ, maxDistanceZ);
                }
            }
        }

        private void CleanMap()
        {
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
        }
    }
}

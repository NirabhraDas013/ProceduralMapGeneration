using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralTerrain
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField]
        private int mapWidthInTiles;
        [SerializeField]
        private int mapDepthInTiles;
        [SerializeField]
        private GameObject tilePrefab;

        private void Start()
        {
            GenerateMap();
        }

        private void GenerateMap()
        {
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
                }
            }
        }
    }
}

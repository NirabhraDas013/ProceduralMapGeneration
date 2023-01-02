using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralTerrain
{
    public class NoiseMapGeneration : MonoBehaviour
    {
        /// <summary>
        /// Genearate a Matrix representing a Noise Map with noise in each coordinate of the level
        /// </summary>
        /// <param name="mapDepth"></param>
        /// <param name="mapWidth"></param>
        /// <param name="levelScale"></param>
        /// <returns></returns>
        public float[,] GeneratePerlinNoiseMap(int mapDepth, int mapWidth, float levelScale, float offsetX, float offsetZ, List<Wave> waves)
        {
            //Create an empty noise map with mapDepth and mapWidth coordinates
            float[,] noiseMap = new float[mapDepth, mapWidth];

            //Calculate sample indices based on the coordinates and the scale
            for (int zIndex = 0; zIndex < mapDepth; zIndex++)
            {
                for (int xIndex = 0; xIndex < mapWidth; xIndex++)
                {
                    float sampleX = (xIndex + offsetX) / levelScale;
                    float sampleZ = (zIndex + offsetZ) / levelScale;

                    //Iterate through the waves to create the noise in passes rather than one sing noisemap
                    float noise = 0f;
                    float normalization = 0f;
                    foreach (Wave wave in waves)
                    {
                        //Generate noise value for each wave rather than a single pass
                        noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + wave.seed, sampleZ * wave.frequency + wave.seed);
                        normalization += wave.amplitude;
                    }

                    //Set the noise value within 0 and 1
                    noise /= normalization;
                    noiseMap[zIndex, xIndex] = noise;
                }
            }

            return noiseMap;
        }

        /// <summary>
        /// Generate a noise proportional to the distance of each map Coordinate to the center of the map supplied as parameter for the center of the whole level
        /// </summary>
        /// <param name="mapDepth"></param>
        /// <param name="mapWidth"></param>
        /// <param name="centerVertexZ"></param>
        /// <param name="maxDistanceZ">Value passed in nemuber of vertices</param>
        /// <param name="offsetZ">Value passed in number of vertices</param>
        /// <returns>noise Map generated from the center of map Depth</returns>
        public float[,] GenerateUniformNoiseMap(int mapDepth, int mapWidth, float centerVertexZ, float maxDistanceZ, float offsetZ)
        {
            //Create an empty noiseMap with the supplied Depth and Width
            float[,] noiseMap = new float[mapDepth, mapWidth];

            for (int zIndex = 0; zIndex < mapDepth; zIndex++)
            {
                //Calculate the sampleZ by summing the index and offset
                float sampleZ = zIndex + offsetZ;
                //Calculate the noise proportional to the distance of the sample to the center of the level
                float noise = Mathf.Abs(sampleZ - centerVertexZ) / maxDistanceZ;
                //Apply the noise for all points in X direction with this z coordinate
                for (int xIndex = 0; xIndex < mapWidth; xIndex++)
                {
                    noiseMap[mapDepth - zIndex - 1, xIndex] = noise;
                }
            }

            return noiseMap;
        }
    }
}

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
        public float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float levelScale, float offsetX, float offsetZ, List<Wave> waves)
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
    }
}

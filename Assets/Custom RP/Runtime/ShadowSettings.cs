using UnityEngine;

[System.Serializable]
public class ShadowSettings
{


    [Min(0f)]
    public float maxDistance = 100f; // max distance of shadow

    [Range(0.001f, 1f)]
    public float distanceFade = 0.1f; // fade distance

    public enum TextureSize
    {
        _256 = 256, _512 = 512, _1024 = 1024, _2048 = 2048, _4096 = 4096, _8196 = 8196
    }

    [System.Serializable]//this makes a hierarchical setting
    public struct Directional
    {
        public TextureSize atlasSize;
        [Range(1, 4)] public int cascadeCount;
        [Range(0f, 1f)] public float cascadeRatio1, cascadeRatio2, cascadeRatio3;
        public Vector3 CascadeRatios =>
             new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);
    }

    public Directional directional = new Directional
    {
        atlasSize = TextureSize._1024,
        cascadeCount = 4,
        cascadeRatio1 = .1f,
        cascadeRatio2 = .25f,
        cascadeRatio3 = 0.5f
    };


}
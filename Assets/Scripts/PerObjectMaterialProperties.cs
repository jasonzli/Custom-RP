using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{

    //Cuffoff is a per instance property!
    static int baseColorId = Shader.PropertyToID("_BaseColor"),
               cutoffId = Shader.PropertyToID("_Cutoff"),
               metallicId = Shader.PropertyToID("_Metallic"),
               smoothnessId = Shader.PropertyToID("Smoothness");
    static MaterialPropertyBlock block; //we only need one of these for all materials!

    [SerializeField]
    Color baseColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    float cutoff = 0.5f, metallic = 0f, smoothness = 0.5f;

    void Awake()
    {
        OnValidate();
    }

    //Remember that OnValidate only fires when the component is changed
    void OnValidate()
    {
        NewColor();
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetColor(baseColorId, baseColor);
        block.SetFloat(cutoffId, cutoff);
        block.SetFloat(metallicId, metallic);
        block.SetFloat(smoothnessId, smoothness);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }

    void NewColor()
    {
        baseColor = Random.ColorHSV();
    }

}

using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");
    static MaterialPropertyBlock block; //we only need one of these for all materials!
    [SerializeField]

    Color baseColor = Color.white;

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
        GetComponent<Renderer>().SetPropertyBlock(block);
    }

    void NewColor()
    {
        baseColor = Random.ColorHSV();
    }

}

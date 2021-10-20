using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    //Protected-> only RenderPipelineAssets can override this
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomeRenderPipeline();
    }
}

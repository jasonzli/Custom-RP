Shader "Custom RP/Unlit"
{
    Properties
    {
        //No Semicolons here
        _BaseColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1 //self
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", FLoat) = 0 //target
    }
    SubShader
    {
        Pass
        {
            //note how this preceeds the HLSLPROGRAM block
            //Selectable transparency blends!
            // src = SrcAlpha and dst OneMinusSrcAlpha is normal transparency
            Blend [_SrcBlend] [_DstBlend]
            
            HLSLPROGRAM
            
            //generates two versions of the shader: one for instancing and one without.
            #pragma multi_compile_instancing
            
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"
            ENDHLSL
            
        }
    }
}
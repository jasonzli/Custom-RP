Shader "Custom RP/Lit"
{
    Properties
    {
        //No Semicolons here
        _BaseMap ("Texture", 2D) = "white" { }//"white" is the default unity white texture
        _BaseColor ("Color", Color) = (.5, .5, .5, 1.0)
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = .5
        [Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha ("Premultiply Alpha", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1 //self
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", FLoat) = 0 //target
        [Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
    }
    SubShader
    {
        Tags { "LightMode" = "CustomLit" }
        Pass
        {
            //note how this preceeds the HLSLPROGRAM block
            //Selectable transparency blends!
            // src = SrcAlpha and dst OneMinusSrcAlpha is normal transparency
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            
            HLSLPROGRAM
            
            #pragma target 3.5
            //adds clipping as a feature to turn off for the shader
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _PREMULTIPLY_ALPHA
            //generates two versions of the shader: one for instancing and one without.
            #pragma multi_compile_instancing
            
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "LitPass.hlsl"
            ENDHLSL
            
        }
    }
    
    CustomEditor "CustomShaderGUI"
}
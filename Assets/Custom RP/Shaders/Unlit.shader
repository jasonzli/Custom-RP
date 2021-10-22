Shader "Custom RP/Unlit"
{
    Properties
    {
        //No Semicolons here
        _BaseColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Pass
        {
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
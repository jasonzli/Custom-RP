#ifndef CUSTOM_UNLIT_PASS_INCLUDED
    #define CUSTOM_UNLIT_PASS_INCLUDED
    
    #include "../ShaderLibrary/Common.hlsl"
    
    float4 UnlitPassVertex(float3 positionOS: POSITION): SV_POSITION
    {
        //float4's guidance: 0.0 for direction, 1.0 for point
        float3 positionWS = TransformObjectToWorld(positionOS.xyz);
        return TransformWorldToHClip(positionWS);
    }
    
    float4 UnlitPassFragment(): SV_TARGET
    {
        return float4(1.0, 1.0, 0.0, 1.0);
    }
    
#endif
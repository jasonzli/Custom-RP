#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
    #define CUSTOM_SHADOW_CASTER_PASS_INCLUDED
    
    #include "../ShaderLibrary/Common.hlsl"
    
    
    TEXTURE2D(_BaseMap);
    SAMPLER(sampler_BaseMap);
    
    //these must be accessed via UNITY_ACCESS_INSTANCED_PROP
    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST) //_S(cale)T(ranslation) variables
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
    
    struct Attributes
    {
        float3 positionOS: POSITION;
        float2 baseUV: TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        //Instancing adds in this to take the instance parameter
    };
    
    struct Varyings //instancing requires a struct as a vertex parameter
    {
        float4 positionCS: SV_POSITION;
        float2 baseUV: VAR_BASE_UV; //this is our own semantic
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    //These changes allow isntancing to work
    Varyings ShadowCasterPassVertex(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input)
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        //float4's guidance: 0.0 for direction, 1.0 for point
        float3 positionWS = TransformObjectToWorld(input.positionOS);
        output.positionCS = TransformWorldToHClip(positionWS);
        
        #if UNITY_REVERSED_Z
            output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
        #else
            output.positionCS.z = min(output.posiitonCS.z, output.posiitonCS.w * UNITY_NEAR_CLIP_VALUE);
        #endif
        
        float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
        //apply scaling and translation in vertex step so the coords are scaled in fragment
        //xy is scale, zw is translation
        output.baseUV = input.baseUV * baseST.xy + baseST.zw;
        return output;
    }
    
    void ShadowCasterPassFragment(Varyings input)
    {
        UNITY_SETUP_INSTANCE_ID(input);
        float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
        float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
        float4 base = baseMap * baseColor;
        
        #if defined(_SHADOWS_CLIP)
            clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
        #endif
    }
    
#endif
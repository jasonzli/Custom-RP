#ifndef CUSTOM_SHADOWS_INCLUDED
    #define CUSTOM_SHADOWS_INCLUDED
    
    #define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
    #define MAX_CASCADE_COUNT 4
    
    //shadow atlas is not a regular texture so use _SHADOW version
    TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
    #define SHADOW_SAMPLER sampler_linear_clamp_compare
    SAMPLER_CMP(SHADOW_SAMPLER); //CMP is a modified sampler that does not filter
    
    CBUFFER_START(_CustomShadows)
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
    CBUFFER_END
    
    struct DirectionalShadowData
    {
        float strength;
        int tileIndex;
    };
    
    float SampleDirectionalShadowAtlas(float3 positionSTS)
    {
        return SAMPLE_TEXTURE2D_SHADOW(
            _DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS //S.hadow T.exture S.pace
        );
    }
    
    //The attentuation is a factor of how much light is being received by the surface.
    //This modifies the light
    float GetDirectionalShadowAttenuation(DirectionalShadowData data, Surface surfaceWS)
    {
        if (data.strength <= 0.0)
        {
            return 1.0;
        }
        //conversion from position to shadow space
        float3 positionSTS = mul(
            _DirectionalShadowMatrices[data.tileIndex],
            float4(surfaceWS.position, 1.0)
        ).xyz;
        float shadow = SampleDirectionalShadowAtlas(positionSTS);
        
        //when strength is zero then it should be 1.0 (no attentuation of light)
        //strength is how much of the shadow we read, and not the amount of shadow
        //this is an artistic indirection
        return lerp(1.0, shadow, data.strength);
    }
#endif
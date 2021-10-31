#ifndef CUSTOM_SHADOWS_INCLUDED
    #define CUSTOM_SHADOWS_INCLUDED
    
    #define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
    #define MAX_CASCADE_COUNT 4
    
    //shadow atlas is not a regular texture so use _SHADOW version
    TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
    #define SHADOW_SAMPLER sampler_linear_clamp_compare
    SAMPLER_CMP(SHADOW_SAMPLER); //CMP is a modified sampler that does not filter
    
    CBUFFER_START(_CustomShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
    float4 _ShadowDistanceFade;
    CBUFFER_END
    
    struct ShadowData
    {
        int cascadeIndex;
        float strength;
    };
    
    //Faded shadows give the sense that light is accumulating at the distance
    float FadedShadowStrength(float distance, float scale, float fade)
    {
        return saturate((1.0 - distance * scale) * fade); //fade by distance, this is linear
    }
    
    //this must be determined on a per-fragment basis, calculated by surface world space position
    ShadowData GetShadowData(Surface surfaceWS)
    {
        ShadowData data;
        //we have the surface position depth, so we can choose to set it 1 or 0 depending on if we're in the cascades
        //calculate the fade by the set values
        data.strength = FadedShadowStrength(
            surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y
        );
        int i;
        for (i = 0; i < _CascadeCount; i++)
        {
            float4 sphere = _CascadeCullingSpheres[i];
            float distanceSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
            if (distanceSqr < sphere.w) //sphere.w is square radius because we do that in Shadows.cs
            {
                //determine which cascade the fragment is in and return that index
                break;
            }
        }
        if (i == _CascadeCount)
        {
            //we might want to play with this
            data.strength = 0.0; //if we're out of the cascades then no shadow strenght at all
        }
        data.cascadeIndex = i;
        return data;
    }
    
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
#ifndef CUSTOM_SHADOWS_INCLUDED
    #define CUSTOM_SHADOWS_INCLUDED
    
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
    
    //directional filter setup is a special sampler for a tent filter
    #if defined(_DIRECTIONAL_PCF3)
        #define DIRECTIONAL_FILTER_SAMPLES 4
        #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
    #elif defined(_DIRECTIONAL_PCF5)
        #define DIRECTIONAL_FILTER_SAMPLES 9
        #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
    #elif defined(_DIRECTIONAL_PCF7)
        #define DIRECTIONAL_FILTER_SAMPLES 16
        #define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
    #endif
    
    #define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
    #define MAX_CASCADE_COUNT 4
    
    //shadow atlas is not a regular texture so use _SHADOW version
    TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
    #define SHADOW_SAMPLER sampler_linear_clamp_compare
    SAMPLER_CMP(SHADOW_SAMPLER); //CMP is a modified sampler that does not filter
    
    CBUFFER_START(_CustomShadows)
    int _CascadeCount;
    float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
    float4 _CascadeData[MAX_CASCADE_COUNT];
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT * MAX_CASCADE_COUNT];
    float4 _ShadowAtlasSize;
    float4 _ShadowDistanceFade;
    CBUFFER_END
    
    struct ShadowData
    {
        int cascadeIndex;
        float cascadeBlend;
        float strength;
    };
    
    struct DirectionalShadowData
    {
        float strength;
        int tileIndex;
        float normalBias; //multiply this value against the texel normal to scale the offset
    };
    
    //Faded shadows give the sense that light is accumulating at the distance
    float FadedShadowStrength(float distance, float scale, float fade)
    {
        return saturate((1.0 - distance * scale) * fade); //fade by distance, this is linear
    }
    
    //this must be determined on a per-fragment basis, calculated by surface world space position
    //Conversion into which cascade it belongs to
    ShadowData GetShadowData(Surface surfaceWS)
    {
        ShadowData data;
        data.cascadeBlend = 1.0;
        //we have the surface position depth, so we can choose to set it 1 or 0 depending on if we're in the cascades
        //calculate the fade by the set values
        data.strength = FadedShadowStrength(
            surfaceWS.depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y
        );
        
        //determine which cascade the fragment is in and return that index
        int i;
        for (i = 0; i < _CascadeCount; i++)
        {
            float4 sphere = _CascadeCullingSpheres[i];
            float distanceSqr = DistanceSquared(surfaceWS.position, sphere.xyz);
            if (distanceSqr < sphere.w) //sphere.w is square radius because we do that in Shadows.cs
            {
                //calcaulate fade between blends
                float fade = FadedShadowStrength(
                    distanceSqr, _CascadeData[i].x, _ShadowDistanceFade.z
                );
                //check if we're in the last cascade and fade as necessary with square fade
                if (i == _CascadeCount - 1)
                {
                    data.strength *= fade;
                }
                else
                {
                    data.cascadeBlend *= fade;
                }
                break;
            }
        }
        if (i == _CascadeCount)
        {
            //we might want to play with this
            data.strength = 0.0; //if we're out of the cascades then no shadow strenght at all
        }
        //if dither is defined, and blend the cascade if we're les than the dither
        #if defined(_CASCADE_BLEND_DITHER)
            else if (data.cascadeBlend < surfaceWS.dither)
            {
                i += 1;
            }
        #endif
        //if soft is not defined, then do not blend
        #if !defined(_CASCADE_BLEND_SOFT)
            data.cascadeBlend = 1.0;
        #endif
        data.cascadeIndex = i; //the cascade that the shadow is in
        return data;
    }
    
    
    float SampleDirectionalShadowAtlas(float3 positionSTS)
    {
        return SAMPLE_TEXTURE2D_SHADOW(
            _DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS //S.hadow T.exture S.pace
        );
    }
    
    float FilterDirectionalShadow(float3 positionSTS)
    {
        #if defined(DIRECTIONAL_FILTER_SETUP)
            float weights[DIRECTIONAL_FILTER_SAMPLES];
            float2 positions[DIRECTIONAL_FILTER_SAMPLES];
            float4 size = _ShadowAtlasSize.yyxx;
            DIRECTIONAL_FILTER_SETUP(size, positionSTS.xy, weights, positions);
            float shadow = 0;
            for (int i = 0; i < DIRECTIONAL_FILTER_SAMPLES; i++)
            {
                shadow += weights[i] * SampleDirectionalShadowAtlas(
                    float3(positions[i].xy, positionSTS.z)
                );
            }
            return shadow;
        #else
            return SampleDirectionalShadowAtlas(positionSTS);
        #endif
    }
    
    //The attentuation is a factor of how much light is being received by the surface.
    //This modifies the light
    float GetDirectionalShadowAttenuation(
        DirectionalShadowData directional, ShadowData global, Surface surfaceWS
    )
    {
        if (directional.strength <= 0.0)
        {
            return 1.0;
        }
        float3 normalBias = surfaceWS.normal *
        //directional normalBias multiplied into the texel offset
        (directional.normalBias * _CascadeData[global.cascadeIndex].y); //move normal by texelSize
        
        //conversion from position to shadow space
        float3 positionSTS = mul(
            _DirectionalShadowMatrices[directional.tileIndex],
            float4(surfaceWS.position + normalBias, 1.0)
        ).xyz;
        //sample the shadow with a filter.
        float shadow = FilterDirectionalShadow(positionSTS); // direct sample is SampleDirectionalShadowAtlas(positionSTS);
        
        //sample between cascades
        if (global.cascadeBlend < 1.0)
        {
            normalBias = surfaceWS.normal *
            (directional.normalBias * _CascadeData[global.cascadeIndex + 1].y); //sample the next cascade
            positionSTS = mul(
                _DirectionalShadowMatrices[directional.tileIndex + 1], //sample next tile up in the cascade
                float4(surfaceWS.position + normalBias, 1.0)
            ).xyz;
            shadow = lerp(
                FilterDirectionalShadow(positionSTS), shadow, global.cascadeBlend
            );
        }
        
        //when strength is zero then it should be 1.0 (no attentuation of light)
        //strength is how much of the shadow we read, and not the amount of shadow
        //this is an artistic indirection
        return lerp(1.0, shadow, directional.strength);
    }
    
    
#endif
#ifndef CUSTOM_SHADOWS_INCLUDED
    #define CUSTOM_SHADOWS_INCLUDED
    
    #define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
    
    //shadow atlas is not a regular texture so use _SHADOW version
    TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
    #define SHADOW_SAMPLER sampler_linear_clamp_compare
    SAMPLER_CMP(sampler_DirectionalShadowAtlas); //CMP is a modified sampler that does not filter
    
    CBUFFER_START(_CustomShadows)
    float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
    CBUFFER_END
#endif
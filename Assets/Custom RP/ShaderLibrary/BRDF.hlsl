#ifndef CUSTOM_BRDF_INCLUDED
    #define CUSTOM_BRDF_INCLUDED
    
    struct BRDF
    {
        float3 diffuse;
        float3 specular;
        float roughness;
    };
    
    #define MIN_REFLECTIVITY 0.04
    
    float OneMinusReflectivity(float metallic)
    {
        float range = 1.0 - MIN_REFLECTIVITY;
        return range - metallic * range;
    }
    
    BRDF GetBRDF(Surface surface)
    {
        BRDF brdf;
        
        float oneMinusReflectivity = OneMinusReflectivity(surface.metallic); //metallics reflect via specular
        brdf.diffuse = surface.color * oneMinusReflectivity;
        //surface.color - brdf.diffuse;
        //reflected light cannot be more than the diffuse
        //but metals take on surface color, so thje more metallic, the more *the specular* becomes the surface color
        brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
        
        float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
        brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
        
        return brdf;
    }
#endif
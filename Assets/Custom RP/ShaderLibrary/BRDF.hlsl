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
        float oneMinusReflectivity = OneMinusReflectivity(surface.metallic) //metallics reflect via specular
        brdf.diffuse = surface.color * oneMinusReflectivity;
        brdf.specular = 0.0;
        brdf.roughness = 1.0;
        return brdf;
    }
#endif
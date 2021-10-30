#ifndef CUSTOM_BRDF_INCLUDED
    #define CUSTOM_BRDF_INCLUDED
    
    struct BRDF
    {
        float3 diffuse;
        float3 specular;
        float roughness;
    };
    
    BRDF GetBRDF(Surface surface)
    {
        BRDF brdf;
        float oneMinusReflectivity = 1.0 - surface.metallic; //metallics reflect via specular
        brdf.diffuse = surface.color * oneMinusReflectivity;
        brdf.specular = 0.0;
        brdf.roughness = 1.0;
        return brdf;
    }
#endif
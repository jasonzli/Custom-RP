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
    
    /////////////
    //
    // BRDF Calculation
    //
    ////////////////
    
    //minimalist CookTorrance BRDF
    float SpecularStrength(Surface surface, BRDF brdf, Light light)
    {
        //The half vector is H = L+V, then normalized, essentially a mixed vector between light and view
        // d is (N dot H)^2 times (r^2 - 1) + 1.0001, where r is the roughness.
        //normalization term is defined as roughness times 4 plus 2.0.
        //That's cookTorrance specularity.
        float3 h = SafeNormalize(light.direction + surface.viewDirection);
        float nh2 = Square(saturate(dot(surface.normal, h)));
        float lh2 = Square(saturate(dot(light.direction, h)));
        float r2 = Square(brdf.roughness);
        float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
        float normalization = brdf.roughness * 4.0 + 2.0;
        return r2 / (d2 * max(0.1, lh2) * normalization);
    }
    
    float3 DirectBRDF(Surface surface, BRDF brdf, Light light)
    {
        return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
    }
    
#endif
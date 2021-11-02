#ifndef CUSTOM_LIGHTING_INCLUDED
    #define CUSTOM_LIGHTING_INCLUDED
    
    float3 IncomingLight(Surface surface, Light light)
    {
        return saturate(dot(surface.normal, light.direction) * light.attentuation) * light.color;
    }
    
    float3 GetLighting(Surface surface, BRDF brdf, Light light)
    {
        return IncomingLight(surface, light) * DirectBRDF(surface, brdf, light);
    }
    
    float3 GetLighting(Surface surfaceWS, BRDF brdf)
    {
        ShadowData shadowData = GetShadowData(surfaceWS);
        float3 color = 0.0;
        for (int i = 0; i < GetDirectionalLightCount(); i++)
        {
            Light light = GetDirectionalLight(i, surfaceWS, shadowData); //get the attenuated light value
            color += GetLighting(surfaceWS, brdf, light); //calculate the lit color of the fragment
        }
        return color;
    }
    
    
#endif
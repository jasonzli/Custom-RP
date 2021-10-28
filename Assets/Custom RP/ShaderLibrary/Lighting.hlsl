#ifndef CUSTOM_LIGHTING_INCLUDED
    #define CUSTOM_LIGHTING_INCLUDED
    
    float3 IncomingLight(Surface surface, Light light)
    {
        return saturate(dot(surface.normal, light.direction)) * light.color;
    }
    
    float3 GetLighting(Surface surface, Light light)
    {
        return IncomingLight(surface, light) * surface.color; //factor albedo into the lighting
    }
    
    float3 GetLighting(Surface surface)
    {
        //Invoke GetDirectionalLight() from the Light.hlsl file
        return GetLighting(surface, GetDirectionalLight());
    }
    
    
#endif
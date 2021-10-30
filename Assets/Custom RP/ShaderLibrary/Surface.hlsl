#ifndef CUSTOM_SURFACE_INCLUDED
    #define CUSTOM_SURFACE_INCLUDED
    
    struct Surface
    {
        float3 normal; //could be defined in any space because we don't care... from the POV of the surface
        float3 color;
        float alpha;
        float metallic;
        float smoothness;
    };
#endif
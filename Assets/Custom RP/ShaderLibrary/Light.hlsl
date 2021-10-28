#ifndef CUSTOM_LIGHT_INCLUDED
    #define CUSTOM_LIGHT_INCLUDED
    
    //a cbuffer called _CustomLight that contains directional light direction and color
    CBUFFER_START(_CustomLight)
    float3 _DirectionalLightColor;
    float3 _DirectionalLightDirection;
    CBUFFER_END
    
    struct Light
    {
        float3 color;
        float3 direction;
    };
    
    //a function GetDirectionalLight() that returns a Light with color and direction
    Light GetDirectionalLight()
    {
        Light light;
        light.color = _DirectionalLightColor;
        light.direction = _DirectionalLightDirection;
        return light;
    }
    
    
#endif
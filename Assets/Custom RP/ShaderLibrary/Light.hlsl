#ifndef CUSTOM_LIGHT_INCLUDED
    #define CUSTOM_LIGHT_INCLUDED
    
    struct Light
    {
        float3 color;
        float3 direction;
    };
    
    //a function GetDirectionalLight() that returns a Light with color and direction
    Light GetDirectionalLight()
    {
        Light light;
        light.color = 1.0;
        light.direction = float3(0.0f, 1.0f, 0.0f);
        return light;
    }
    
    
#endif
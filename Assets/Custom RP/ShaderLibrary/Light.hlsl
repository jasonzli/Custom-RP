#ifndef CUSTOM_LIGHT_INCLUDED
    #define CUSTOM_LIGHT_INCLUDED
    
    #define MAX_DIRECTIONAL_LIGHT_COUNT 4
    
    //a cbuffer called _CustomLight that contains directional light direction and color
    CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
    CBUFFER_END
    
    struct Light
    {
        float3 color;
        float3 direction;
        float attentuation;
    };
    
    int GetDirectionalLightCount()
    {
        return _DirectionalLightCount;
    }
    
    DirectionalShadowData GetDirectionalShadowData(int lightIndex)
    {
        DirectionalShadowData data;
        data.strength = _DirectionalLightShadowData[lightIndex].x;
        data.tileIndex = _DirectionalLightShadowData[lightIndex].y;
        
        return data;
    }
    
    //a function GetDirectionalLight() that returns a Light with color and direction
    Light GetDirectionalLight(int index, Surface surfaceWS)
    {
        Light light;
        light.color = _DirectionalLightColors[index].rgb;
        light.direction = _DirectionalLightDirections[index].xyz;
        DirectionalShadowData shadowData = GetDirectionalShadowData(index);
        light.attentuation = GetDirectionalShadowAttenuation(shadowData, surfaceWS);
        return light;
    }
    
    
    
#endif
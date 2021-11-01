#ifndef CUSTOM_LIGHT_INCLUDED
    #define CUSTOM_LIGHT_INCLUDED
    
    #define MAX_DIRECTIONAL_LIGHT_COUNT 4
    
    //a cbuffer called _CustomLight that contains directional light direction and color
    //all constant buffers are set in c#
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
    
    DirectionalShadowData GetDirectionalShadowData(
        int lightIndex, ShadowData shadowData
    )
    {
        DirectionalShadowData data;
        data.strength = _DirectionalLightShadowData[lightIndex].x * shadowData.strength;
        data.tileIndex = _DirectionalLightShadowData[lightIndex].y + shadowData.cascadeIndex; //index corrections
        data.normalBias = _DirectionalLightShadowData[lightIndex].z;
        return data;
    }
    
    //a function GetDirectionalLight() that returns a Light with color and direction
    Light GetDirectionalLight(int index, Surface surfaceWS, ShadowData shadowData)
    {
        Light light;
        light.color = _DirectionalLightColors[index].rgb;
        light.direction = _DirectionalLightDirections[index].xyz;
        DirectionalShadowData dirShadowData = GetDirectionalShadowData(index, shadowData);
        light.attentuation = GetDirectionalShadowAttenuation(dirShadowData, shadowData, surfaceWS);
        
        //light.attentuation = shadowData.cascadeIndex * .25; //use this to view the cascades in camera
        return light;
    }
    
    
    
#endif
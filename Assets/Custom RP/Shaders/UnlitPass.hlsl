#ifndef CUSTOM_UNLIT_PASS_INCLUDED
    #define CUSTOM_UNLIT_PASS_INCLUDED
    
    #include "../ShaderLibrary/Common.hlsl"
    
    //CBUFFER macro is replaced with the instancing buffer macro (which does both)
    /* formerly
    CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor
    CBUFFER_END
    */
    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
    
    struct Attributes
    {
        float3 positionOS: POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        //Instancing adds in this to take the instance parameter
    };
    
    struct Varyings //instancing requires a struct as a vertex parameter
    {
        float4 positionCS: SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    //These changes allow isntancing to work
    Varyings UnlitPassVertex(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input)
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        //float4's guidance: 0.0 for direction, 1.0 for point
        float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
        output.positionCS = TransformObjectToHClip(positionWS);
        return output;
    }
    
    float4 UnlitPassFragment(Varyings input): SV_TARGET
    {
        UNITY_SETUP_INSTANCE_ID(input);
        return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    }
    
#endif
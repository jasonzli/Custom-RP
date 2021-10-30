#ifndef CUSTOM_COMMON_INCLUDED
    #define CUSTOM_COMMON_INCLUDED
    
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
    #include "UnityInput.hlsl"
    
    //this is how unity expects these matrices to be defined
    #define UNITY_MATRIX_M unity_ObjectToWorld
    #define UNITY_MATRIX_I_M unity_WorldToObject
    #define UNITY_MATRIX_V unity_MatrixV
    #define UNITY_MATRIX_VP unity_MatrixVP
    #define UNITY_MATRIX_P glstate_matrix_projection
    
    //For instancing... obviously
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
    
    //This package requires a UNITY_MATIRX_M macro
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
    
    //some easy functions
    float Square(float v)
    {
        return v * v;
    }
#endif
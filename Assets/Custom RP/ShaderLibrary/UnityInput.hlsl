#ifndef CUSTOM_UNITY_INPUT_INCLUDED
    #define CUSTOM_UNITY_INPUT_INCLUDED
    
    //these are just things that unity will fill in I guess.
    
    float4x4 unity_ObjectToWorld; //object to world place
    float4x4 unity_WorldToObject;
    real4 unity_WorldTransformParams; //real4 is a macro itself
    
    float4x4 unity_MatrixVP; //view projection, convert world to homogeneous clip space
    float4x4 unity_MatrixV;
    float4x4 glstate_matrix_projection;
#endif
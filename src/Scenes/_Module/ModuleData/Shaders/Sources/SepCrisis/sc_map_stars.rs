// Needs: Shader -> Flags -> uses_geometry_pass
// Untick: Is Generated

// topology_shader.rs
// system_postfx_bokeh_draw.rs

#include "../shader_configuration.h"

#include "hlsl_to_pssl.rsh"
// #ifdef USE_GNM
// #endif

#include "definitions.rsh"

VS_OUTPUT main_vs(RGL_VS_INPUT In)
{
    VS_OUTPUT Out;
    Out.position = In.Position;
    return Out;
}
// #if VERTEX_SHADER
// #endif // VS

// Generates 4 vertices to from a quad and then uses to view_matrix so that it faces the camera for billboarding
[maxvertexcount(4)]
void main_gs(triangle VS_OUTPUT In[1], inout TriangleStream<VS_OUTPUT> Out)
{
    float3 center = In[0].position;
    float half_size = 0.5f;
    float3 camera_right = float3(g_view._m00, g_view._m10, g_view._m20);
    float3 camera_up    = float3(g_view._m01, g_view._m11, g_view._m21);

    // 4 corners of the quad relative to the camera view
    float3 positions[4];
    positions[0] = center + (camera_right * -half_size) + (camera_up * half_size); // Top-Left
    positions[1] = center + (camera_right * half_size) + (camera_up * half_size); // Top-Right
    positions[2] = center + (camera_right * -half_size) + (camera_up * -half_size); // Bottom-Left
    positions[3] = center + (camera_right * half_size) + (camera_up * -half_size); // Bottom-Right

    float2 tex_coords[4] = 
    {
        float2(0.0f, 0.0f),
        float2(1.0f, 0.0f),
        float2(0.0f, 1.0f),
        float2(1.0f, 1.0f)
    };

    VS_OUTPUT vertex;
    [unroll]
    for (int i = 0; i < 4; i++)
    {
        // Transform the billboard corner to clip space
        float4 world_pos = float4(positions[i], 1.0f);
        float4 view_pos = mul(world_pos, g_view);
        vertex.position = mul(view_pos, g_proj);
        
        vertex.tex_coord = tex_coords[i];
        
        Out.Append(vertex);
    }
    
    Out.RestartStrip();
}

PS_OUTPUT_TO_USE main_ps(VS_OUTPUT_STANDART In)
{
    PS_OUTPUT_TO_USE Output = (PS_OUTPUT_TO_USE)0;

	// // Convert texture coordinates from [0, 1] to [-1, 1] to find the center (0,0)
    // float2 uv = In.tex_coord * 2.0f - 1.0f;
    
    // // Calculate squared distance from center
    // float dist_sq = dot(uv, uv);
    
    // // If outside the radius of 1.0, discard the pixel (makes it a circle)
    // if (dist_sq > 1.0f)
    // {
    //     discard;
    // }
    
    // // Soft radial intensity drop-off (bright center, soft edges)
    // float intensity = exp(-dist_sq * 5.0f); 

    // Output.RGBColor.rgba = float4(1.0f, 1.0f, 1.0f, intensity);

    Output.RGBColor.rgba = float4(1, 1, 1, 1);
}
// #if PIXEL_SHADER
// #endif // PS
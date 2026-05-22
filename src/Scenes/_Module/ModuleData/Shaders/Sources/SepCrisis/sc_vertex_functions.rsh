#ifndef SC_VERTEX_FUNCTIONS_RSH
#define SC_VERTEX_FUNCTIONS_RSH

#include "../shader_configuration.h"
#include "definitions.rsh"

#if VERTEX_SHADER

void sc_pristine_grid_world_centered(inout RGL_VS_INPUT In, inout Per_vertex_modifiable_variables pv_modifiable, inout VS_OUTPUT_STANDART output)
{
    // output.tex_coord = output.world_position.xy;

    float4 camera_pos = mul(g_root_camera_position, output.world_position);
    output.tex_coord = camera_pos.yz;
}

#endif // VERTEX_SHADER

#endif // INCLUDE_SC_VERTEX_FUNCTIONS
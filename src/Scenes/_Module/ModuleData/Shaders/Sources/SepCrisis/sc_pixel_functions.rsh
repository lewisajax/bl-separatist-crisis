// Pristine grid from The Best Darn Grid Shader (yet)
// https://bgolus.medium.com/the-best-darn-grid-shader-yet-727f9278b9d8
float sc_pristine_grid( in float2 uv, float2 lineWidth)
{
    float2 dx = ddx(uv);
    float2 dy = ddy(uv);

    float2 uvDeriv = float2(length(float2(dx.x, dy.x)), length(float2(dx.y, dy.y)));
    bool2 invertLine = bool2(lineWidth.x > 0.5, lineWidth.y > 0.5);

    float2 targetWidth = float2(
      invertLine.x ? 1.0 - lineWidth.x : lineWidth.x,
      invertLine.y ? 1.0 - lineWidth.y : lineWidth.y
      );

    float2 drawWidth = clamp(targetWidth, uvDeriv, float2(0.5, 0.5));
    float2 lineAA = uvDeriv * 1.5;

    float2 gridUV = abs(frac(uv) * 2.0 - 1.0);
    gridUV.x = invertLine.x ? gridUV.x : 1.0 - gridUV.x;
    gridUV.y = invertLine.y ? gridUV.y : 1.0 - gridUV.y;

    float2 grid2 = smoothstep(drawWidth + lineAA, drawWidth - lineAA, gridUV);
    grid2 *= clamp(targetWidth / drawWidth, 0.0, 1.0);
    grid2 = lerp(grid2, targetWidth, clamp(uvDeriv * 2.0 - 1.0, 0.0, 1.0));
    grid2.x = invertLine.x ? 1.0 - grid2.x : grid2.x;
    grid2.y = invertLine.y ? 1.0 - grid2.y : grid2.y;
    return lerp(grid2.x, 1.0, grid2.y);
}

void sc_pristine_grid_output(inout Pixel_shader_input_type In, in Per_pixel_static_variables pp_static, inout Per_pixel_modifiable_variables pp_modifiable, inout PS_OUTPUT Output)
{
    float line_width_x = g_mesh_vector_argument.x;
    float line_width_y = g_mesh_vector_argument.y;
    half4 line_color = g_mesh_factor_color.rgba;
    half4 base_color = g_mesh_factor2_color.rgba;

    // float grid_scale = g_mesh_vector_argument.z;
    // float grid_scale = g_root_camera_position.z;
    float grid_scale = g_root_camera_position.z;

    float grid = sc_pristine_grid(In.tex_coord.xy * grid_scale, float2(line_width_x, line_width_y));
    Output.RGBColor.rgba = lerp(base_color, line_color, grid * line_color.a);
}
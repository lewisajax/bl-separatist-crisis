//WARNING : This is a generated file
//WARNING : Do not change this file.

// The game will sometimes generate related source files (_gbuffer, overdraw etc) when they're not present. 
// But it also overwrites the #define Diffuse2 Texture line wrong in already created files. 
// It writes Diffuse 2 instead but your game won't catch it until u delete the .sacx files and recompile the shaders.

#define DiffuseMap	texture0
#define Diffuse2Map	texture1
#define NormalMap	texture2
#define DetailNormalMap	texture3
#define SpecularMap	texture4
#define Heightmap	texture6
#define DecalDiffuseMap	texture7
#define DecalNormalMap	texture8
#define DecalSpecularMap	texture9

// If there's already a compiled shader .sacx file under ProgramData/Bannerlord/Shader... Then it won't read from a missing or corrupted .rs file.
// We might be able to just copy the .sacx file and put that in the ProgramData Shader folder but it's most likely generated for that user's device.

#define Vertex_shader_output_type VS_OUTPUT_STANDART

#if USE_TESSELATION
 #define Pixel_shader_input_type VS_OUTPUT_STANDART
#else
 #define Pixel_shader_input_type VS_OUTPUT_STANDART
#endif

#define my_material_id MATERIAL_ID_STANDART

#define Per_pixel_modifiable_variables pbr_shading_values

#define Per_pixel_auxiliary_variables standart_auxiliary_values

// We can have nested folders in the Shaders/Sources dir but inside the actual source file, 
// our imports should use relative paths as if our source files are under Shader/Sources instead of Shader/Sources/SepCrisis.

// Inside our Shader in the resource browser, the file name will use the nested dir i.e. SepCrisis/source_file

#include "../shader_configuration.h" 
#include "definitions.rsh" 
#include "modular_struct_definitions.rsh" 
#include "shared_functions.rsh" 
#include "motion_vector.rsh" 
#include "generated_definitions.rsh" 

#include "standart.rsh" 

#include "pbr_shading_functions.rsh" 

#include "pbr_standart_functions.rsh" 

// If we include header files that are also in the shared nested folder, the relative import path for it in the source folder, should use the nested dir

#include "./SepCrisis/sc_pixel_functions.rsh"
#include "./SepCrisis/sc_vertex_functions.rsh"

#if VERTEX_SHADER
Vertex_shader_output_type main_vs(RGL_VS_INPUT In)
{
	INITIALIZE_OUTPUT(Vertex_shader_output_type, Out);
	Per_vertex_modifiable_variables pv_modifiable = (Per_vertex_modifiable_variables)0;

	calculate_object_space_values_standart(In , pv_modifiable, Out );
	calculate_world_space_values_standart(In , pv_modifiable, Out );
	calculate_render_related_values_standart(In , pv_modifiable, Out );
	sc_pristine_grid_world_centered(In , pv_modifiable, Out );
#ifdef SYSTEM_SHOW_VERTEX_COLORS
	vs_output_vertex_color(Out, In);
#endif
	return Out;
}

#endif
#if USE_TESSELATION
	#define Constant_hs_output_type HS_CONSTANT_DATA_OUTPUT_STANDART

	#define Hull_shader_output_type VS_OUTPUT_STANDART

	#define Domain_shader_output_type VS_OUTPUT_STANDART

	#define Generated_hull_shader_func_name standart_hs

	#define Generated_domain_shader_func_name standart_ds

#if HULL_SHADER
	[domain("tri")]
	[partitioning("fractional_odd")]
	[outputtopology("triangle_cw")]
	[outputcontrolpoints(3)]
	[patchconstantfunc("constants_hs")]
	[maxtessfactor(16.0)]
	Hull_shader_output_type main_hs(InputPatch<VS_OUTPUT_STANDART, 3> inputPatch, uint control_point_id : SV_OutputControlPointID)
	{
		return Generated_hull_shader_func_name(inputPatch, control_point_id);           
	}
#endif
#if DOMAIN_SHADER
	[domain("tri")]
	Domain_shader_output_type main_ds(Constant_hs_output_type input, float3 barycentric_coords : SV_DomainLocation, const OutputPatch<Hull_shader_output_type, 3> triangle_patch)
	{
		return Generated_domain_shader_func_name(input, barycentric_coords, triangle_patch);            
	}
#endif
#endif

#if PIXEL_SHADER
#if !ALPHA_TEST && !USE_SMOOTH_FADE_OUT
[earlydepthstencil]
#endif
PS_OUTPUT_TO_USE main_ps(Pixel_shader_input_type In)
{
	PS_OUTPUT_TO_USE Output = (PS_OUTPUT_TO_USE)0;
	Per_pixel_static_variables pp_static = (Per_pixel_static_variables)0;
	Per_pixel_modifiable_variables pp_modifiable = (Per_pixel_modifiable_variables)0;

	Per_pixel_auxiliary_variables pp_aux = (Per_pixel_auxiliary_variables)0;

	calculate_per_pixel_static_variables(In, pp_static);

	sample_textures_standart(In , pp_static , pp_modifiable, pp_aux);
	calculate_alpha_standart(In , pp_static , pp_modifiable, pp_aux);
	apply_alpha_test(In, pp_modifiable.early_alpha_value);
	calculate_normal_standart(In , pp_static , pp_modifiable, pp_aux);
	calculate_albedo_standart(In , pp_static , pp_modifiable, pp_aux);
	calculate_specularity_standart(In , pp_static , pp_modifiable, pp_aux);
	calculate_diffuse_ao_factor_standart_forward(In , pp_static , pp_modifiable, pp_aux);
	sc_pristine_grid_output(In , pp_static , pp_modifiable, Output);
	accumulate_light_contributions(In , pp_static , pp_modifiable, Output);
#ifdef SYSTEM_SHOW_VERTEX_COLORS
	#if (MATERIAL_ID_TERRAIN != my_material_id) && (MATERIAL_ID_DEFERRED != my_material_id) && (MATERIAL_ID_GRASS != my_material_id)
		Output.RGBColor.rgba = get_masked_vertex_color(In.vertex_color.rgba);
	#endif
#endif
	return Output;
}
#endif
#ifndef UNIVERSAL_SIMPLE_LIT_PASS_INCLUDED
#define UNIVERSAL_SIMPLE_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "section_clipping_CS.cginc"

float  _MapScale;

static const float3x3 projMatrix = float3x3(float3(1, 0, 0), float3(0, 1, 0), float3(0, 0, 1));

struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    float2 lightmapUV    : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    float3 posWS                    : TEXCOORD2;    // xyz: posWS

#ifdef _NORMALMAP
    half4 normal                    : TEXCOORD3;    // xyz: normal, w: viewDir.x
    half4 tangent                   : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    half4 bitangent                 : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    half3  normal                   : TEXCOORD3;
    half3 viewDir                   : TEXCOORD4;
#endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#ifdef _MAIN_LIGHT_SHADOWS
    float4 shadowCoord              : TEXCOORD7;
#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData.positionWS = input.posWS;

#ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
    inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
#else
    half3 viewDirWS = input.viewDir;
    inputData.normalWS = input.normal;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
	inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
}
///////////////////////////////////////////////////////////////////////////////
//                            LIBRARY FUNCTIONS                              //
inline float3 ObjSpaceViewDir( in float4 v )
{
    float3 objSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
    return objSpaceCameraPos - v.xyz;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput;
	
	#if PLANE_CLIPPING_ENABLED 

	if(_retractBackfaces==1)
	{
		float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
		float frontface = dot(input.normalOS, ObjSpaceViewDir(input.positionOS));
		if(frontface<0) 
		{
			float3 worldNorm = TransformObjectToWorldNormal(input.normalOS);
			worldPos -= worldNorm * _BackfaceExtrusion;
			vertexInput = GetVertexPositionInputs(mul(unity_WorldToObject, float4(worldPos, 1)).xyz);
		}
		else
		{
			vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
		}
	}
	else
	{
	#endif
		vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
	#if PLANE_CLIPPING_ENABLED
	}
	#endif

    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

#ifdef _NORMALMAP
    output.normal = half4(normalInput.normalWS, viewDirWS.x);
    output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDir = viewDirWS;
#endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    return output;
}

// Used for StandardSimpleLighting shader
half4 LitPassFragmentSimple(Varyings input
#if PLANE_CLIPPING_ENABLED
, bool isFrontFace : SV_IsFrontFace
#endif
) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = input.uv;
    half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

    half alpha = diffuseAlpha.a * _BaseColor.a;
    AlphaDiscard(alpha, _Cutoff);
#ifdef _ALPHAPREMULTIPLY_ON
    diffuse *= alpha;
#endif

    half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    half3 emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
    half4 specular = SampleSpecularSmoothness(uv, alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    half smoothness = specular.a;

    InputData inputData;
    InitializeInputData(input, normalTS, inputData);

	PLANE_CLIP(inputData.positionWS);

	half4 color;

    color = UniversalFragmentBlinnPhong(inputData, diffuse, specular, smoothness, emission, alpha);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    #if PLANE_CLIPPING_ENABLED
	if(!isFrontFace)
	{
		color = _SectionColor;
	}
	#endif
    return color;
};

half4 LitPassFragmentSimpleTriplanar(Varyings input) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	float2 uv = input.uv;
	//half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
	//half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

	half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));

	InputData inputData;
	InitializeInputData(input, normalTS, inputData);

	//triplanar diffuse mapping
	float3 projPos = mul(projMatrix, inputData.positionWS);
	float3 projNorm = mul(projMatrix, inputData.normalWS);
	half2 UV;
	UV = projPos.zy * _MapScale; // side
	half4 cx = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, float2(UV.x*_BaseMap_ST.x, UV.y*_BaseMap_ST.y))*abs(projNorm.x); // use WALLSIDE texture
	UV = projPos.xy * _MapScale; // front
	half4 cz = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, float2(UV.x*_BaseMap_ST.x, UV.y*_BaseMap_ST.y))*abs(projNorm.z); // use WALL texture
	UV = projPos.xz * _MapScale; // top
	half4 cy = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, float2(UV.x*_BaseMap_ST.x, UV.y*_BaseMap_ST.y))*abs(projNorm.y); // use FLR texture

	half4 diffuseAlpha = cx + cy + cz;
	half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

	half alpha = diffuseAlpha.a * _BaseColor.a;
	AlphaDiscard(alpha, _Cutoff);
#ifdef _ALPHAPREMULTIPLY_ON
	diffuse *= alpha;
#endif
	half3 emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
	half4 specular = SampleSpecularSmoothness(uv, alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
	half smoothness = specular.a;

	half4 color = UniversalFragmentBlinnPhong(inputData, diffuse, specular, smoothness, emission, alpha);
	color.rgb = MixFog(color.rgb, inputData.fogCoord);
	return color;
};

#endif

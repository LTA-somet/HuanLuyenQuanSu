// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt) /URP/Blit

Shader "Hidden/CrossSectionURP/FaceSideMask"
{
	Properties
	{
		_ColorFront("ColorFront", Color) = (1, 0, 0, 1)
		_ColorBack("ColorBack", Color) = (0, 1, 0, 1)
	}
	SubShader
	{
		Tags { "RenderType" = "Clipping" "RenderPipeline" = "UniversalPipeline"}
		LOD 100

		Pass
		{
			Name "Mask"
			//ZTest Always
			ZWrite On
			Cull Off

			HLSLPROGRAM
		// Required to compile gles 2.0 with standard srp library
		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x
		#pragma vertex Vertex
		#pragma fragment Fragment
		#pragma multi_compile __ CLIP_PLANE CLIP_TWO_PLANES CLIP_SPHERE CLIP_CUBE CLIP_CORNER CLIP_TUBES
		#include "section_clipping_CS.cginc"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		inline float DecodeFloatRG(float2 enc)
		{
			float2 kDecodeDot = float2(1.0, 1 / 255.0);
			return dot(enc, kDecodeDot);
		}

		struct Attributes
		{
			float4 positionOS   : POSITION;
			float2 uv           : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID//
		};

		struct Varyings
		{
			half4 positionCS    : SV_POSITION;
			half2 uv            : TEXCOORD0;
			float linearDepth : TEXCOORD1;
			float4 screenPos : TEXCOORD2;
#if PLANE_CLIPPING_ENABLED
    float3 positionWS               : TEXCOORD3;
#endif
			UNITY_VERTEX_INPUT_INSTANCE_ID//
			UNITY_VERTEX_OUTPUT_STEREO//
		};

		Varyings Vertex(Attributes input)
		{
			Varyings output;

			UNITY_SETUP_INSTANCE_ID(input);//
			UNITY_TRANSFER_INSTANCE_ID(input, output);//
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);//

			output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
			output.uv = input.uv;
			output.screenPos = ComputeScreenPos(output.positionCS);
			output.linearDepth = -(TransformWorldToView(input.positionOS.xyz).z * _ProjectionParams.w);
#if PLANE_CLIPPING_ENABLED 
			float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
			output.positionWS = worldPos;
#endif
			return output;
		}

		half4 _ColorFront;
		half4 _ColorBack;
		TEXTURE2D(_CameraDepthTexture);
		SAMPLER(sampler_CameraDepthTexture);

		half4 Fragment(Varyings input
#if PLANE_CLIPPING_ENABLED
			, bool isFrontFace : SV_IsFrontFace
#endif
		) : SV_Target
		{
			UNITY_SETUP_INSTANCE_ID(input);//
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);//
			PLANE_CLIP(input.positionWS);
			//half4 col = SAMPLE_TEXTURE2D(_BackfaceMaskTexture, sampler_BackfaceMaskTexture, input.uv);
			half4 col = float4(0, 0, 0, 1);
			// decode depth texture info
			float2 uv = input.screenPos.xy / input.screenPos.w; // normalized screen-space pos
			float camDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
			//float camDepth = DecodeFloatRG(enc.zw);
			camDepth = Linear01Depth(camDepth, _ZBufferParams);
			float diff = saturate(input.linearDepth - camDepth);
			if (diff < 0.004)
			{

	col = _ColorFront;
	#if PLANE_CLIPPING_ENABLED
				if (!isFrontFace)
				{
					col = _ColorBack;
				}
	#endif
			}
			return col;

		}
		ENDHLSL
		}
	}
	Fallback Off
}

Shader "CrossSectionURP/Unlit"
{
    // Keep properties of StandardSpecular shader for upgrade reasons.
    Properties
    {
        _BaseColor("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
		_Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5

        // BlendMode
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        
        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
        
        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (0.5, 0.5, 0.5, 1)
		[HideInInspector] _SampleGI("SampleGI", float) = 0.0 // needed from bakedlit

		// CrossSection properties
		_SectionColor ("Section Color", Color) = (1,0,0,1)
		[Toggle(INVERSE)] _inverse("inverse", Float) = 0
		[Toggle(RETRACT_BACKFACES)] _retractBackfaces("retractBackfaces", Float) = 0
		[Enum(None,0,Alpha,1,Red,8,Green,4,Blue,2,RGB,14,RGBA,15)] _ColorMask("Color Mask", Int) = 0

		//[HideInInspector] _SectionPoint("_SectionPoint", Vector) = (0,0,0,1)	//expose as local properties
		//[HideInInspector] _SectionPlane("_SectionPlane", Vector) = (1,0,0,1)	//expose as local properties
		//[HideInInspector] _SectionPlane2("_SectionPlane2", Vector) = (0,1,0,1)	//expose as local properties
		//[HideInInspector] _Radius("_Radius", Vector) = (0,1,0,1)	//expose as local properties

		//[HideInInspector] _SectionScale("_SectionScale", Vector) = (0,0,1,1)	//expose as local properties
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        LOD 100

		Blend[_SrcBlend][_DstBlend]
        ZWrite[_ZWrite]
        Cull[_Cull]
		ColorMask[_ColorMask]

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            // -------------------------------------
			// CrossSection Keywords
			#pragma multi_compile __ CLIP_PLANE CLIP_TWO_PLANES CLIP_SPHERE CLIP_CUBE CLIP_TUBES CLIP_BOX CLIP_CORNER
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_TWO_PLANES CLIP_SPHERE CLIP_CUBE CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.
			#pragma shader_feature RETRACT_BACKFACES
			#pragma shader_feature INVERSE
		
			// -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile_fog
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
			#include "section_clipping_CS.cginc"

            struct Attributes
            {
                float4 positionOS       : POSITION;
				float3 normalOS			: NORMAL;
                float2 uv               : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv        : TEXCOORD0;
                float fogCoord  : TEXCOORD1;
#if PLANE_CLIPPING_ENABLED
				float3 positionWS : TEXCOORD2;
#endif
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
			///////////////////////////////////////////////////////////////////////////////
			//                            LIBRARY FUNCTIONS                              //
			inline float3 ObjSpaceViewDir(in float4 v)
			{
				float3 objSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
				return objSpaceCameraPos - v.xyz;
			}

			///////////////////////////////////////////////////////////////////////////////
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				VertexPositionInputs vertexInput;

#if PLANE_CLIPPING_ENABLED 
				float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
				output.positionWS = worldPos;
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

                output.vertex = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }


            half4 frag(Varyings input
#if PLANE_CLIPPING_ENABLED
			, bool isFrontFace : SV_IsFrontFace
#endif
			) : SV_Target
			{
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				PLANE_CLIP(input.positionWS);
                half2 uv = input.uv;
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                half3 color = texColor.rgb * _BaseColor.rgb;
                half alpha = texColor.a * _BaseColor.a;
                AlphaDiscard(alpha, _Cutoff);

#ifdef _ALPHAPREMULTIPLY_ON
                color *= alpha;
#endif
                color = MixFog(color, input.fogCoord);
				half4 _color = half4(color, alpha);

#if PLANE_CLIPPING_ENABLED
				if(!isFrontFace)
				{
					_color = _SectionColor;
				}
#endif
				return _color;
            }

            ENDHLSL
        }


        Pass
        {
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

			//--------------------------------------
			// CrossSection Keywords
			#pragma multi_compile __ CLIP_PLANE CLIP_TWO_PLANES CLIP_SPHERE CLIP_CUBE CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_TWO_PLANES CLIP_SPHERE CLIP_CUBE CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.
			#pragma shader_feature INVERSE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}

            Cull Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaUnlit

            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitMetaPass.hlsl"

            ENDHLSL
        }
    }
    Fallback "Hidden/InternalErrorShader"
    CustomEditor "UnityEditor.CrossSection.URP.ShaderGUI.UnlitShader"
}

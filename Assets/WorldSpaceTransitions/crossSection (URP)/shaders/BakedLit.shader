Shader "CrossSectionURP/Baked Lit"
{
    // Keep properties of StandardSpecular shader for upgrade reasons.
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (1, 1, 1, 1)
        _Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5
        _BumpMap("Normal Map", 2D) = "bump" {}

        // BlendMode
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("Src", Float) = 1.0
        [HideInInspector] _DstBlend("Dst", Float) = 0.0
        [HideInInspector] _ZWrite("ZWrite", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        
        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
        
        // CrossSection Properties
		_SectionColor ("Section Color", Color) = (1,0,0,1)
		[Toggle(INVERSE)] _inverse("inverse", Float) = 0
		[Toggle(RETRACT_BACKFACES)] _retractBackfaces("retractBackfaces", Float) = 0

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

        Pass
        {
            Name "BakedLit"
            Tags{ "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            // -------------------------------------
            // Material Keywords
			#pragma shader_feature _ _NORMALMAP
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            // -------------------------------------
			// CrossSection Keywords
			#pragma multi_compile __ CLIP_PLANE CLIP_TWO_PLANES CLIP_SPHERE CLIP_CUBE CLIP_TUBES CLIP_BOX
			//#pragma multi_compile_local __ CLIP_PLANE CLIP_TWO_PLANES CLIP_SPHERE CLIP_CUBE CLIP_TUBES CLIP_BOX // to get enumerated keywords as local.
			#pragma shader_feature RETRACT_BACKFACES
			#pragma shader_feature INVERSE
		
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag

            // Lighting include is needed because of GI
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/BakedLitInput.hlsl"

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                float2 lightmapUV       : TEXCOORD1;
                float3 normalOS         : NORMAL;
                float4 tangentOS        : TANGENT;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float3 uv0AndFogCoord           : TEXCOORD0; // xy: uv0, z: fogCoord
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
                half3 normalWS                    : TEXCOORD2;
#if defined(_NORMALMAP)
                half4 tangentWS                   : TEXCOORD3;
#endif
#if PLANE_CLIPPING_ENABLED
				float3 positionWS : TEXCOORD4;
#endif
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
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

                output.vertex = vertexInput.positionCS;
                output.uv0AndFogCoord.xy = TRANSFORM_TEX(input.uv, _BaseMap);
                output.uv0AndFogCoord.z = ComputeFogFactor(vertexInput.positionCS.z);

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
    #if defined(_NORMALMAP)
				real sign = input.tangentOS.w * GetOddNegativeScale();
				output.tangentWS = half4(normalInput.tangentWS.xyz, sign);
    #endif
                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS, output.vertexSH);
                
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

                half2 uv = input.uv0AndFogCoord.xy;
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                half3 color = texColor.rgb * _BaseColor.rgb;
                half alpha = texColor.a * _BaseColor.a;
                AlphaDiscard(alpha, _Cutoff);

#ifdef _ALPHAPREMULTIPLY_ON
                color *= alpha;
#endif
#if defined(_NORMALMAP)
				half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap)).xyz;
				float sgn = input.tangentWS.w;      // should be either +1 or -1
				float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
				half3 normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent, input.normalWS));
#else
				half3 normalWS = input.normalWS;
#endif
				normalWS = NormalizeNormalPerPixel(normalWS);
				color *= SAMPLE_GI(input.lightmapUV, input.vertexSH, normalWS);
				color = MixFog(color, input.uv0AndFogCoord.z);
				alpha = OutputAlpha(alpha);

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

            #include "Packages/com.unity.render-pipelines.universal/Shaders/BakedLitInput.hlsl"
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
            #pragma fragment UniversalFragmentMetaBakedLit

            #include "Packages/com.unity.render-pipelines.universal/Shaders/BakedLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/BakedLitMetaPass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Universal2D"
            Tags{ "LightMode" = "Universal2D" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/BakedLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
    Fallback "Hidden/InternalErrorShader"
    CustomEditor "UnityEditor.CrossSection.URP.ShaderGUI.BakedLitShader"
}
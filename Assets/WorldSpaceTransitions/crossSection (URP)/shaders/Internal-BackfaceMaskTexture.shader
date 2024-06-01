// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/Internal-BackfaceMaskTexture" {
Properties {
	_MainTex ("", 2D) = "white" {}
	_Cutoff ("", Float) = 0.5
	_Color ("", Color) = (1,1,1,1)
}



SubShader {
	Tags { "RenderType"="Clipping" }
	
	Pass {
	Cull Off
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile __ CLIP_PLANE CLIP_TWO_PLANES CLIP_SPHERE CLIP_CUBE CLIP_TUBES
#include "section_clipping_CS.cginc"
#include "UnityCG.cginc"
struct v2f {
    float4 pos : SV_POSITION;
    float4 nz : TEXCOORD0;
	float4 wpos : TEXCOORD1;
	UNITY_VERTEX_OUTPUT_STEREO
};
v2f vert( appdata_base v ) {
    v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
	o.wpos = float4(worldPos.xyz, 1);
    o.pos = UnityObjectToClipPos(v.vertex);
	//ComputeScreenPos(o.pos);
    o.nz.xyz = COMPUTE_VIEW_NORMAL;
    o.nz.w = COMPUTE_DEPTH_01;
    return o;
}
fixed4 frag(v2f i, float face : VFACE) : SV_Target{
	//PLANE_CLIP(i.wpos);
	//i.nz.xyz = face >= 0 ? half3(0,0,0) : half3(1,1,1);
//fixed4 frag(v2f i, bool isFrontFace : SV_IsFrontFace) : SV_Target {
	PLANE_CLIP(i.wpos);
	//i.nz.xyz = isFrontFace ? half3(0,0,0): half3(1,1,1);
	i.nz.xyz = face >= 0 ? half3(-1, -1, -1) : half3(1, 1, 1);
	return EncodeDepthNormal (i.nz.w , i.nz.xyz);
}
ENDCG
	}
}


SubShader{
	Tags { "RenderType" = "Opaque" }
	Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct v2f {
	float4 pos : SV_POSITION;
	float4 nz : TEXCOORD0;
	UNITY_VERTEX_OUTPUT_STEREO
};
v2f vert(appdata_base v) {
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	o.pos = UnityObjectToClipPos(v.vertex);
	o.nz.xyz = COMPUTE_VIEW_NORMAL;
	o.nz.w = COMPUTE_DEPTH_01;
	return o;
}
fixed4 frag(v2f i) : SV_Target {
	return EncodeDepthNormal(i.nz.w, i.nz.xyz);
}
ENDCG
	}
}


Fallback Off
}

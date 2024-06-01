// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/Internal-DepthVfaceTexture" {
Properties {
	//_MainTex ("", 2D) = "white" {}
	//_Cutoff ("", Float) = 0.5
	//_Color ("", Color) = (1,1,1,1)
}



SubShader {
	Tags { "RenderType"="Clipping" }
	
	Pass {
	Cull Off
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile __ CLIP_PLANE CLIP_TWO_PLANES CLIP_SPHERE CLIP_CUBE CLIP_CORNER CLIP_TUBES
#include "section_clipping_CS.cginc"
#include "UnityCG.cginc"
struct v2f {
    float4 pos : SV_POSITION;
	float4 wpos : TEXCOORD1;
	UNITY_VERTEX_OUTPUT_STEREO
};
v2f vert( appdata_base v ) {
    v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex)).xyz;
	o.wpos = float4(worldPos.xyz, 1);
    o.pos = UnityObjectToClipPos(v.vertex);
    return o;
}
fixed4 frag(v2f i, float face : VFACE) : SV_Target{
	PLANE_CLIP(i.wpos);
	half4 col = face >= 0 ? half4(0, 0, 0, 1) : half4(0, 1, 0, 1);
	return col;
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
	UNITY_VERTEX_OUTPUT_STEREO
};
v2f vert(appdata_base v) {
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	o.pos = UnityObjectToClipPos(v.vertex);
	return o;
}
fixed4 frag(v2f i) : SV_Target {
	return half4(0, 0, 0, 1);
}
ENDCG
	}
}


Fallback Off
}

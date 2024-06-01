//section_clipping_CS.cginc

#ifndef PLANE_CLIPPING_INCLUDED
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
//#pragma exclude_renderers d3d11 gles
#define PLANE_CLIPPING_INCLUDED


uniform half _BackfaceExtrusion = 0;
//Plane clipping definitions

#if INVERSE
uniform float _inverse = 1;
#else
uniform float _inverse = 0;
#endif

#if RETRACT_BACKFACES
uniform float _retractBackfaces = 1;
#else
uniform float _retractBackfaces = 0;
#endif

uniform half4 _SectionColor;

#if CLIP_PLANE || CLIP_TWO_PLANES || CLIP_SPHERE || CLIP_CUBE || CLIP_TUBES || CLIP_BOX || CLIP_CORNER || FADE_PLANE || FADE_SPHERE || CLIP_SPHERE_OUT
	//PLANE_CLIPPING_ENABLED will be defined.
	//This makes it easier to check if this feature is available or not.


	#define PLANE_CLIPPING_ENABLED 1


	float randFromTexture(float3 co, sampler2D _noise, float _noiseScale)
	 {
		 co *= _noiseScale;
		 float x = frac(co.x);
		 float y = frac(co.y);
		 float z = frac(co.z);
		 float zy = 8*frac(8*z) - frac(8*frac(8*z));
		 float zx = 8*z - frac(8*z);
		 half4 col = tex2D(_noise, float2((zx + x)/8,(zy + y)/8));
		 return col.r;
	 }

#if CLIP_PLANE || CLIP_TWO_PLANES || CLIP_CUBE || CLIP_SPHERE || FADE_PLANE || FADE_SPHERE || CLIP_SPHERE_OUT
	uniform float _SectionOffset = 0;
	uniform float3 _SectionPlane;
	uniform float3 _SectionPoint;

	#if CLIP_TWO_PLANES || CLIP_CUBE
	uniform float3 _SectionPlane2;
	#endif
	#if CLIP_SPHERE || CLIP_CUBE || FADE_SPHERE || FADE_PLANE || CLIP_SPHERE_OUT
	uniform float _Radius = 0;
	#endif

	#if CLIP_CUBE
	static const float3 _SectionPlane3 = normalize(cross(_SectionPlane, _SectionPlane2));
	#endif
#endif

#if CLIP_PLANE || CLIP_CUBE || CLIP_SPHERE || FADE_PLANE || FADE_SPHERE || CLIP_BOX || CLIP_TUBES || CLIP_SPHERE_OUT
	
#endif

#if FADE_SPHERE || FADE_PLANE 
	uniform sampler2D _TransitionGradient;
	uniform fixed _spread = 1;

	#if SCREENDISSOLVE || SCREENDISSOLVE_GLOW
		uniform sampler2D _ScreenNoise;
		float4 _ScreenNoise_TexelSize;
		uniform float _ScreenNoiseScale;
	#endif
	#if DISSOLVE || DISSOLVE_GLOW
		uniform float _Noise3dScale;
		uniform sampler2DArray _NoiseArray;
		uniform float _NoiseArraySliceRange;
	#endif
#endif

#if CLIP_TUBES
		uniform float4 _AxisDirs[64];
#endif
#if CLIP_TUBES || CLIP_SPHERES
		uniform float4 _centerPoints[64];
		uniform float _Radiuses[64];
		uniform int _centerCount = 0;
#endif

#if CLIP_BOX ||  CLIP_CORNER
	uniform float4x4 _WorldToBoxMatrix;

	#if CLIP_BOX
	float4 _SectionScale;

	// txx - world-to-box transformation
	// ro is the ray origin in world space
	// rd is the ray direction in world space
	// txx is the world-to-box transformation
	// rad is the half-length of the box
	bool clipBoxCone(in float3 ro, in float3 rd, in float4x4 txx, in float3 rad)
	{
		float3 rdd = (mul(txx, float4(rd, 0.0))).xyz;
		float3 roo = (mul(txx, float4(ro, 1.0))).xyz;

		float3 m = 1.0 / rdd;
		float3 n = m * roo;
		float3 k = abs(m)*rad;

		float3 t1 = -n - k;
		float3 t2 = -n + k;

		float tN = max(max(t1.x, t1.y), t1.z);
		float tF = min(min(t2.x, t2.y), t2.z);
		//if (tN > tF || (tF < 0.0 || tN>0.0)) return -1.0;
		if (tN > tF || tF < 0.0) return true;
		//return !(tN > 0.0);
		return false;
	}

	// txx - world-to-box transformation
	// po - point in world space
	// poo - point in box object space
	bool clipBox(in float3 po, in float4x4 txx, in float3 rad)
	{
		float3 poo = (mul(txx, float4(po, 1.0))).xyz;
		return (abs(poo.x) - rad.x) > 0 || (abs(poo.y) - rad.y) > 0 || (abs(poo.z) - rad.z) > 0;
	}

	#endif
	#if CLIP_CORNER
		bool clipCorner(in float3 po, in float4x4 txx)
		{
			float3 poo = (mul(txx, float4(po, 1.0))).xyz;
			return (poo.x > 0 && poo.y > 0 && poo.z > 0);
		}
	#endif
#endif

#if	CLIP_SPHERE_OUT
		static const float sphereDist = length(_WorldSpaceCameraPos - _SectionPoint);
		static const float hideRadius = sqrt(sphereDist*sphereDist -_Radius* _Radius);
		static const float3 coneDir = normalize(_WorldSpaceCameraPos - _SectionPoint);
#endif

	//discard drawing of a point in the world if it is behind any one of the planes.
	void PlaneClip(float3 posWorld) {
		bool _clip = false;
		#if CLIP_TWO_PLANES
		float3 vcross = cross(_SectionPlane,_SectionPlane2);
		if(vcross.y>=0){//<180
			_clip = _clip||(- dot((posWorld - _SectionPoint),_SectionPlane)<0);
			_clip = _clip||(- dot((posWorld - _SectionPoint),_SectionPlane2)<0);
			if(_clip) discard;

		}
		if(vcross.y<0){//>180
			if((_SectionOffset - dot((posWorld - _SectionPoint),_SectionPlane)<0)&&(- dot((posWorld - _SectionPoint),_SectionPlane2)<0)) discard;
		}
		//#else //
		#endif
		#if CLIP_PLANE
			#if DISSOLVE
			float dist = -dot((posWorld - _SectionPoint),_SectionPlane)*(1-2*_inverse);
			float transparency = saturate(1/_spread*dist + 0.5);
			#if FADE_PLANE
				float4 col = tex2D(_TransitionGradient, float2(transparency,1));
				transparency = col.r;
			#endif
			if(transparency<1)
				{
					if(randFromTexture(posWorld, _Noise, _NoiseScale)>transparency||transparency==0) discard;
				}
			#else
			if((_SectionOffset - dot((posWorld - _SectionPoint),_SectionPlane))*(1-2*_inverse)<0) discard;
			//if(((_SectionOffset - dot((posWorld - _SectionPoint + _SectionPlane*0.07),_SectionPlane))>0)||((_SectionOffset - dot((posWorld - _SectionPoint - _SectionPlane*0.07),_SectionPlane))<0)) discard;//two paralell planes test
			#endif
		#endif
		#if CLIP_SPHERE
			#if DISSOLVE
			float dist = length(posWorld - _SectionPoint);
			float transparency = (1-2*_inverse)*saturate(dist/_spread + 0.5 - _Radius/_spread);
			#if FADE_SPHERE
				float4 col = tex2D(_TransitionGradient, float2(transparency,1));
				transparency = col.r;
			#endif
			if(transparency<1)
				{
					if(randFromTexture(posWorld, _Noise, _NoiseScale)>transparency||transparency==0) discard;
				}

			#else
			if(
				(1-2*_inverse)*
				(dot((posWorld - _SectionPoint),(posWorld - _SectionPoint)) - _Radius*_Radius)<0) discard; //_inverse = 1 : negative to clip the outside of the sphere
			#endif
		#endif

#if CLIP_SPHERE_OUT
			_clip = ((dot((posWorld - _SectionPoint), (posWorld - _SectionPoint)) - _Radius * _Radius) > 0);
			//bool belowGround = (posWorld.y < 0);
			//_clip = _clip || !belowGround;
			if(_clip) discard;
#endif
		#if CLIP_CUBE
		half _sign = 1-2*_inverse;
		if((_SectionOffset - dot((posWorld - _SectionPoint -_Radius*_SectionPlane),-_SectionPlane)*_sign<0)&&(_SectionOffset - dot((posWorld - _SectionPoint +_Radius*_SectionPlane),-_SectionPlane)*_sign>0)
		&&(_SectionOffset - dot((posWorld - _SectionPoint -_Radius*_SectionPlane2),-_SectionPlane2)*_sign<0)&&(_SectionOffset - dot((posWorld - _SectionPoint +_Radius*_SectionPlane2),-_SectionPlane2)*_sign>0) 
		&&(_SectionOffset - dot((posWorld - _SectionPoint -_Radius*_SectionPlane3),-_SectionPlane3)*_sign<0)&&(_SectionOffset - dot((posWorld - _SectionPoint +_Radius*_SectionPlane3),-_SectionPlane3)*_sign>0))
		discard;
		//if((_SectionOffset - dot((posWorld - _SectionPoint -_Radius*_SectionPlane2),-_SectionPlane2)<0)&&(_SectionOffset - dot((posWorld - _SectionPoint +_Radius*_SectionPlane2),-_SectionPlane2)>0)) discard;
		#endif


#if CLIP_TUBES
		bool _clipTubes = false;
		int _centerCountTruncated = min(_centerCount, 64);
		for (int i = 0; i < _centerCountTruncated; i++)
		{
			_clipTubes = _clipTubes || ((dot(posWorld - _centerPoints[i] - _AxisDirs[i] * dot(_AxisDirs[i], posWorld - _centerPoints[i]), posWorld - _centerPoints[i] - _AxisDirs[i] * dot(_AxisDirs[i], posWorld - _centerPoints[i])) - _Radiuses[i] * _Radiuses[i]) < 0);
		}

		//}
		if (_inverse == 0)
		{
			//if(_clip) discard;
			_clip = _clip || _clipTubes;
		}
		else
		{
			//if(!_clip) discard;
			_clip = _clip || !_clipTubes;
		}
#endif

		#if CLIP_BOX
		_clip = clipBox(posWorld, _WorldToBoxMatrix, 0.5*_SectionScale);
		if(_clip) discard;
		#endif

		#if CLIP_CORNER
		_clip = clipCorner(posWorld, _WorldToBoxMatrix);
		if(_clip) discard;
		#endif

	}
	#if CLIP_BOX || CLIP_SPHERE_OUT
	void ClipWithCaps(float3 posWorld) {
		bool _clip = false;
	#if CLIP_BOX
			_clip = clipBoxCone(posWorld, normalize(_WorldSpaceCameraPos - posWorld), _WorldToBoxMatrix, 0.5*_SectionScale);
			if (_inverse == 1) _clip = !_clip;

	#endif
	#if CLIP_SPHERE_OUT
			bool inCone = false;
			float3 pointToCam = _WorldSpaceCameraPos - posWorld;
			float coneProj = dot(pointToCam, coneDir);
			float coneDist = length(pointToCam - coneProj*coneDir);
			bool inFront = length(posWorld - _WorldSpaceCameraPos) < hideRadius;//
			bool outsideSphere = length(posWorld - _SectionPoint)>_Radius;
			inCone = (coneDist / coneProj < _Radius / hideRadius);
			_clip = !inCone || inFront && outsideSphere;
			//bool belowGround = (posWorld.y < 0);
			//_clip = _clip && !belowGround;
			//_clip = inFront;
	#endif
		if(_clip) discard;
	}

	#define PLANE_CLIPWITHCAPS(posWorld) ClipWithCaps(posWorld); //preprocessor macro that will produce an empty block if no clipping planes are used.
	#endif

	#if FADE_PLANE || FADE_SPHERE
		inline float4 fadeTransition(float3 posWorld)
		{
			#if FADE_PLANE
			float dist = -dot((posWorld - _SectionPoint),_SectionPlane)*(1-2*_inverse);
			float transparency = saturate(dist/_spread + 0.5);
			float4 col = tex2D(_TransitionGradient, float2(transparency,1));
			float4 rgbcol = tex2D(_TransitionGradient, float2(transparency,0.5));
			rgbcol.a = col.r;
			return rgbcol;
			#endif
			#if FADE_SPHERE
			float dist = length(posWorld - _SectionPoint);
			float transparency = (1-2*_inverse)*saturate(dist/_spread + 0.5 - _Radius/_spread);
			float4 col = tex2D(_TransitionGradient, float2(transparency,1));
			float4 rgbcol = tex2D(_TransitionGradient, float2(transparency,0.5));
			rgbcol.a = col.r;
			return rgbcol;
			#endif
		}

		inline float fadeTransparency(float3 posWorld)
		{
			float transparency = 0;
			#if FADE_PLANE
			float dist = -dot((posWorld - _SectionPoint),_SectionPlane);//*(1-2*_inverse);
			transparency = (dist/_spread + 0.5);
			#endif
			#if FADE_SPHERE
			float dist = length(posWorld - _SectionPoint);
			transparency = (dist/_spread + 0.5 - _Radius/_spread);//*(1-2*_inverse);
			#endif
			return transparency;
		}
		#define TRANSFADE(posWorld) fadeTransparency(posWorld);
		#define PLANE_FADE(posWorld) fadeTransition(posWorld);
	#endif

//preprocessor macro that will produce an empty block if no clipping planes are used.
#define PLANE_CLIP(posWorld) PlaneClip(posWorld);
    
#else
//empty definition
#define PLANE_CLIP(s)
#define PLANE_CLIPWITHCAPS(s) //empty definition
//#define PLANE_FADE(s)
#endif


#endif // PLANE_CLIPPING_INCLUDED
// Made with Amplify Shader Editor v1.9.2.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BK/Impostor"
{
	Properties
	{
		[Enum(Front,2,Back,1,Both,0)]_Cull("Render Face", Int) = 2
		[Toggle(_HIDESIDES_ON)] _HideSides("Hide Sides", Float) = 0
		_HidePower("Hide Power", Float) = 2.5
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[Header(Main Maps)][Space(10)]_MainColor("Main Color", Color) = (1,1,1,0)
		_Diffuse("Diffuse", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_Mask("Mask", 2D) = "white" {}
		_NormalPower("Normal Power", Range( 0 , 1)) = 1
		_VertexOcclusionPower("Vertex Occlusion Power", Range( 0 , 1)) = 0
		[Space(10)][Header(Gradient Parameters)][Space(10)]_GradientColor("Gradient Color", Color) = (1,1,1,0)
		_GradientFalloff("Gradient Falloff", Range( 0 , 2)) = 2
		_GradientPosition("Gradient Position", Range( 0 , 1)) = 0.5
		[Toggle(_INVERTGRADIENT_ON)] _InvertGradient("Invert Gradient", Float) = 0
		[Space(10)][Header(Color Variation)][Space(10)]_ColorVariation("Color Variation", Color) = (1,0,0,0)
		_ColorVariationPower("Color Variation Power", Range( 0 , 1)) = 1
		_ColorVariationNoise("Color Variation Noise", 2D) = "white" {}
		_NoiseScale("Noise Scale", Float) = 0.5
		[Space(10)][Header(Multipliers)][Space(10)]_WindMultiplier("BaseWind Multiplier", Float) = 1
		[Space(10)]_WindTrunkPosition("Wind Trunk Position", Float) = 0
		_WindTrunkContrast("Wind Trunk Contrast", Float) = 10
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull [_Cull]
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		#pragma shader_feature_local _INVERTGRADIENT_ON
		#pragma shader_feature_local _HIDESIDES_ON
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPosition;
		};

		struct SurfaceOutputStandardCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			half3 Translucency;
		};

		uniform int _Cull;
		uniform float WindSpeed;
		uniform float WindPower;
		uniform float WindBurstsSpeed;
		uniform float WindBurstsScale;
		uniform float WindBurstsPower;
		uniform float _WindTrunkContrast;
		uniform float _WindTrunkPosition;
		uniform float _WindMultiplier;
		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float _NormalPower;
		uniform sampler2D _Diffuse;
		uniform float4 _Diffuse_ST;
		uniform float4 _MainColor;
		uniform float4 _GradientColor;
		uniform float _GradientPosition;
		uniform float _GradientFalloff;
		uniform float4 _ColorVariation;
		uniform float _ColorVariationPower;
		uniform sampler2D _ColorVariationNoise;
		uniform float _NoiseScale;
		uniform sampler2D _Mask;
		uniform float4 _Mask_ST;
		uniform float _VertexOcclusionPower;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform float _HidePower;
		uniform float _Cutoff = 0.5;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		inline float Dither8x8Bayer( int x, int y )
		{
			const float dither[ 64 ] = {
				 1, 49, 13, 61,  4, 52, 16, 64,
				33, 17, 45, 29, 36, 20, 48, 32,
				 9, 57,  5, 53, 12, 60,  8, 56,
				41, 25, 37, 21, 44, 28, 40, 24,
				 3, 51, 15, 63,  2, 50, 14, 62,
				35, 19, 47, 31, 34, 18, 46, 30,
				11, 59,  7, 55, 10, 58,  6, 54,
				43, 27, 39, 23, 42, 26, 38, 22};
			int r = y * 8 + x;
			return dither[r] / 64; // same # of instructions as pre-dividing due to compiler magic
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float temp_output_102_0 = ( _Time.y * WindSpeed );
			float2 appendResult10_g2 = (float2(WindBurstsSpeed , WindBurstsSpeed));
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult11_g2 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 panner13_g2 = ( 1.0 * _Time.y * appendResult10_g2 + appendResult11_g2);
			float simplePerlin2D17_g2 = snoise( panner13_g2*( WindBurstsScale / 100.0 ) );
			simplePerlin2D17_g2 = simplePerlin2D17_g2*0.5 + 0.5;
			float temp_output_129_0 = ( WindPower * ( simplePerlin2D17_g2 * WindBurstsPower ) );
			float BaseWindColor288 = v.color.g;
			float4 temp_cast_0 = (pow( ( 1.0 - BaseWindColor288 ) , _WindTrunkPosition )).xxxx;
			float4 temp_output_299_0 = saturate( CalculateContrast(_WindTrunkContrast,temp_cast_0) );
			float3 appendResult113 = (float3(( ( sin( temp_output_102_0 ) * temp_output_129_0 ) * temp_output_299_0 ).r , 0.0 , ( ( cos( temp_output_102_0 ) * ( temp_output_129_0 * 0.5 ) ) * temp_output_299_0 ).r));
			float3 BaseWind151 = ( appendResult113 * _WindMultiplier );
			v.vertex.xyz += BaseWind151;
			v.vertex.w = 1;
			float4 ase_screenPos = ComputeScreenPos( UnityObjectToClipPos( v.vertex ) );
			o.screenPosition = ase_screenPos;
		}

		inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi )
		{
			#if !defined(DIRECTIONAL)
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0.rgb, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandard r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Metallic = s.Metallic;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandard (r, viewDir, gi) + c;
		}

		inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi )
		{
			#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
				gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
			#else
				UNITY_GLOSSY_ENV_FROM_SURFACE( g, s, data );
				gi = UnityGlobalIllumination( data, s.Occlusion, s.Normal, g );
			#endif
		}

		void surf( Input i , inout SurfaceOutputStandardCustom o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			float3 _Normal342 = UnpackScaleNormal( tex2D( _Normal, uv_Normal ), _NormalPower );
			o.Normal = _Normal342;
			float2 uv_Diffuse = i.uv_texcoord * _Diffuse_ST.xy + _Diffuse_ST.zw;
			float4 tex2DNode56 = tex2D( _Diffuse, uv_Diffuse );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			#ifdef _INVERTGRADIENT_ON
				float staticSwitch306 = ( 1.0 - ase_worldNormal.y );
			#else
				float staticSwitch306 = ase_worldNormal.y;
			#endif
			float clampResult39 = clamp( ( ( staticSwitch306 + (-2.0 + (_GradientPosition - 0.0) * (1.0 - -2.0) / (1.0 - 0.0)) ) / _GradientFalloff ) , 0.0 , 1.0 );
			float4 lerpResult46 = lerp( _MainColor , _GradientColor , clampResult39);
			float4 blendOpSrc53 = lerpResult46;
			float4 blendOpDest53 = _ColorVariation;
			float4 lerpBlendMode53 = lerp(blendOpDest53,( blendOpDest53/ max( 1.0 - blendOpSrc53, 0.00001 ) ),_ColorVariationPower);
			float3 ase_worldPos = i.worldPos;
			float2 appendResult71 = (float2(ase_worldPos.x , ase_worldPos.z));
			float4 lerpResult58 = lerp( lerpResult46 , ( saturate( lerpBlendMode53 )) , ( _ColorVariationPower * pow( tex2D( _ColorVariationNoise, ( appendResult71 * ( _NoiseScale / 100.0 ) ) ) , 3.0 ) ));
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			float4 tex2DNode385 = tex2D( _Mask, uv_Mask );
			float4 lerpResult386 = lerp( tex2DNode56 , lerpResult58 , ( _MainColor.a * tex2DNode385.b ));
			float4 _Albedo339 = lerpResult386;
			o.Albedo = _Albedo339.rgb;
			o.Occlusion = pow( tex2DNode385.g , _VertexOcclusionPower );
			float3 temp_cast_1 = (1.0).xxx;
			o.Translucency = temp_cast_1;
			o.Alpha = 1;
			float _Opacity231 = tex2DNode56.a;
			float4 ase_screenPos = i.screenPosition;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 clipScreen217 = ase_screenPosNorm.xy * _ScreenParams.xy;
			float dither217 = Dither8x8Bayer( fmod(clipScreen217.x, 8), fmod(clipScreen217.y, 8) );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 normalizeResult214 = normalize( cross( ddy( ase_worldPos ) , ddx( ase_worldPos ) ) );
			float dotResult200 = dot( ase_worldViewDir , normalizeResult214 );
			float clampResult222 = clamp( ( ( ( 1.0 - ( ( 1.0 - abs( dotResult200 ) ) * 2.0 ) ) * _Opacity231 ) * _HidePower ) , 0.0 , 1.0 );
			dither217 = step( dither217, clampResult222 );
			float OpacityDither205 = dither217;
			#ifdef _HIDESIDES_ON
				float staticSwitch234 = OpacityDither205;
			#else
				float staticSwitch234 = _Opacity231;
			#endif
			clip( staticSwitch234 - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustom keepalpha fullforwardshadows exclude_path:deferred dithercrossfade vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 customPack2 : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack2.xyzw = customInputData.screenPosition;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.screenPosition = IN.customPack2.xyzw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19201
Node;AmplifyShaderEditor.CommentaryNode;150;-6016,1792;Inherit;False;3070.769;864.2947;;25;399;254;113;108;296;294;290;299;111;112;165;109;104;129;105;106;103;102;101;100;164;151;298;297;295;Wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;302;-6016,2816;Inherit;False;577.3333;543;;5;380;293;288;287;55;VertexColor;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;226;-6016,1152;Inherit;False;2814;511;;20;217;283;224;223;215;396;222;205;209;216;207;204;200;214;199;213;212;211;210;397;Dithering;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;210;-5968,1456;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DdyOpNode;211;-5776,1392;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DdxOpNode;212;-5776,1488;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;359;-6656,-640;Inherit;False;1565;669;;12;43;38;39;37;46;35;33;306;29;155;28;27;Color Gradient;1,1,1,1;0;0
Node;AmplifyShaderEditor.CrossProductOpNode;213;-5648,1424;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;27;-6624,-512;Inherit;True;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;199;-5568,1200;Inherit;True;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;214;-5488,1424;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;29;-6400,-384;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-6528,-256;Float;False;Property;_GradientPosition;Gradient Position;12;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;360;-6016,128;Inherit;False;2428.53;854.9357;Color blend controlled by world-space noise;20;231;339;388;386;367;362;71;72;42;41;50;161;391;392;385;56;58;53;45;48;Color Variation;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;200;-5312,1312;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;155;-6192,-256;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-2;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;306;-6240,-464;Inherit;False;Property;_InvertGradient;Invert Gradient;13;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;204;-5184,1312;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-6016,-64;Float;False;Property;_GradientFalloff;Gradient Falloff;11;0;Create;True;0;0;0;False;0;False;2;0.9;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;33;-5968,-368;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;37;-5712,-192;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;207;-5056,1312;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;43;-5712,-576;Float;False;Property;_MainColor;Main Color;4;0;Create;True;0;0;0;False;2;Header(Main Maps);Space(10);False;1,1,1,0;0.4101034,0.7264151,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;38;-5712,-384;Float;False;Property;_GradientColor;Gradient Color;10;0;Create;True;0;0;0;False;3;Space(10);Header(Gradient Parameters);Space(10);False;1,1,1,0;0.2224449,0.490566,0.01619794,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;216;-4880,1280;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;46;-5360,-320;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;48;-5328,224;Inherit;False;Property;_ColorVariation;Color Variation;14;0;Create;True;0;0;0;False;3;Space(10);Header(Color Variation);Space(10);False;1,0,0,0;0.1103595,0.2924528,0.2567374,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;45;-5328,400;Inherit;False;Property;_ColorVariationPower;Color Variation Power;15;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;209;-4736,1296;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;53;-4944,192;Inherit;True;ColorDodge;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;58;-4560,176;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;363;-6656,-1024;Inherit;False;895.731;255.9596;;3;342;65;57;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;56;-4576,512;Inherit;True;Property;_Diffuse;Diffuse;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;385;-4576,736;Inherit;True;Property;_Mask;Mask;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;39;-5548.915,-190.9036;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;392;-5471.41,460.96;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;391;-5496.221,423.7485;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;161;-5120,640;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;3;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-4864,640;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldPosInputsNode;41;-5984,512;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;42;-5984,704;Inherit;False;Property;_NoiseScale;Noise Scale;17;0;Create;True;0;0;0;False;0;False;0.5;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;72;-5792,704;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;71;-5792,560;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;362;-5632,640;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;367;-5440,592;Inherit;True;Property;_ColorVariationNoise;Color Variation Noise;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;55;-5984,2880;Inherit;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;287;-5696,2880;Inherit;False;MicroWindColor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;288;-5696,3008;Inherit;False;BaseWindColor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;293;-5696,3136;Inherit;False;DepositLayerColor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;354;-2496,288;Inherit;False;Constant;_Transluency;Transluency;25;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;205;-3456,1344;Inherit;False;OpacityDither;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;215;-4570.3,1329;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;234;-2496,384;Inherit;False;Property;_HideSides;Hide Sides;1;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;232;-2755.553,352;Inherit;False;231;_Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;233;-2752,449;Inherit;False;205;OpacityDither;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;222;-4000,1280;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;224;-4224,1280;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;223;-4576,1440;Inherit;False;Property;_HidePower;Hide Power;2;0;Create;True;0;0;0;False;0;False;2.5;4.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;283;-4784,1488;Inherit;False;231;_Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;396;-3840,1472;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;397;-4224,1568;Inherit;False;287;MicroWindColor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DitheringNode;217;-3651,1344;Inherit;False;1;False;4;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;3;SAMPLERSTATE;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;380;-5696,3264;Inherit;False;VertexOcclusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;383;-2762.681,968.1882;Inherit;False;Property;_VertexOcclusionPower;Vertex Occlusion Power;9;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-6624,-960;Float;False;Property;_NormalPower;Normal Power;8;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;65;-6336,-960;Inherit;True;Property;_Normal;Normal;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;342;-6016,-960;Inherit;False;_Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;153;-2726.159,580.5167;Inherit;False;151;BaseWind;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;382;-2442.681,820.1882;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;295;-4832,2368;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;297;-4640,2432;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleContrastOpNode;298;-4480,2464;Inherit;False;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;151;-3200,1920;Inherit;False;BaseWind;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;164;-3456,1920;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;100;-5888,1920;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;101;-5888,2048;Inherit;False;Global;WindSpeed;Wind Speed;20;1;[HideInInspector];Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;102;-5632,1968;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;103;-5376,1856;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;106;-5376,2112;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-5376,1984;Inherit;False;Global;WindPower;Wind Power;21;1;[HideInInspector];Create;True;0;0;0;False;0;False;0.01;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;129;-5120,1984;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;-4864,1856;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-4608,2096;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;165;-3712,2112;Inherit;False;Property;_WindMultiplier;BaseWind Multiplier;18;0;Create;False;0;0;0;False;3;Space(10);Header(Multipliers);Space(10);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;112;-4096,1856;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;111;-4096,2096;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;299;-4256,2464;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;290;-5056,2368;Inherit;False;288;BaseWindColor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;294;-4896,2464;Inherit;False;Property;_WindTrunkPosition;Wind Trunk Position;19;0;Create;True;0;0;0;False;1;Space(10);False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;296;-4896,2560;Inherit;False;Property;_WindTrunkContrast;Wind Trunk Contrast;20;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;-4864,2176;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;113;-3920,1920;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;254;-3712,1856;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;399;-5376,2240;Inherit;False;BK_Wind;-1;;2;511f08b81f352dd4e9774d4c87a1b504;0;0;2;FLOAT3;56;FLOAT;57
Node;AmplifyShaderEditor.LerpOp;386;-3968,176;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;388;-4144,448;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;339;-3808,176;Inherit;False;_Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;231;-3808,608;Inherit;False;_Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;346;-2432,0;Inherit;False;339;_Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;384;-2432,96;Inherit;False;342;_Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.IntNode;402;-2000.134,-17.06012;Inherit;False;Property;_Cull;Render Face;0;1;[Enum];Create;False;1;;0;1;Front,2,Back,1,Both,0;True;0;False;2;2;False;0;1;INT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-1989.13,120.9673;Float;False;True;-1;7;;0;0;Standard;BK/Impostor;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;3;21;-1;-1;0;False;0;0;True;_Cull;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;211;0;210;0
WireConnection;212;0;210;0
WireConnection;213;0;211;0
WireConnection;213;1;212;0
WireConnection;214;0;213;0
WireConnection;29;0;27;2
WireConnection;200;0;199;0
WireConnection;200;1;214;0
WireConnection;155;0;28;0
WireConnection;306;1;27;2
WireConnection;306;0;29;0
WireConnection;204;0;200;0
WireConnection;33;0;306;0
WireConnection;33;1;155;0
WireConnection;37;0;33;0
WireConnection;37;1;35;0
WireConnection;207;0;204;0
WireConnection;216;0;207;0
WireConnection;46;0;43;0
WireConnection;46;1;38;0
WireConnection;46;2;39;0
WireConnection;209;0;216;0
WireConnection;53;0;46;0
WireConnection;53;1;48;0
WireConnection;53;2;45;0
WireConnection;58;0;46;0
WireConnection;58;1;53;0
WireConnection;58;2;50;0
WireConnection;39;0;37;0
WireConnection;392;0;391;0
WireConnection;391;0;43;4
WireConnection;161;0;367;0
WireConnection;50;0;45;0
WireConnection;50;1;161;0
WireConnection;72;0;42;0
WireConnection;71;0;41;1
WireConnection;71;1;41;3
WireConnection;362;0;71;0
WireConnection;362;1;72;0
WireConnection;367;1;362;0
WireConnection;287;0;55;1
WireConnection;288;0;55;2
WireConnection;293;0;55;3
WireConnection;205;0;217;0
WireConnection;215;0;209;0
WireConnection;215;1;283;0
WireConnection;234;1;232;0
WireConnection;234;0;233;0
WireConnection;222;0;224;0
WireConnection;224;0;215;0
WireConnection;224;1;223;0
WireConnection;396;0;283;0
WireConnection;396;1;222;0
WireConnection;396;2;397;0
WireConnection;217;0;222;0
WireConnection;380;0;55;4
WireConnection;65;5;57;0
WireConnection;342;0;65;0
WireConnection;382;0;385;2
WireConnection;382;1;383;0
WireConnection;295;0;290;0
WireConnection;297;0;295;0
WireConnection;297;1;294;0
WireConnection;298;1;297;0
WireConnection;298;0;296;0
WireConnection;151;0;164;0
WireConnection;164;0;113;0
WireConnection;164;1;165;0
WireConnection;102;0;100;0
WireConnection;102;1;101;0
WireConnection;103;0;102;0
WireConnection;106;0;102;0
WireConnection;129;0;105;0
WireConnection;129;1;399;57
WireConnection;104;0;103;0
WireConnection;104;1;129;0
WireConnection;109;0;106;0
WireConnection;109;1;108;0
WireConnection;112;0;104;0
WireConnection;112;1;299;0
WireConnection;111;0;109;0
WireConnection;111;1;299;0
WireConnection;299;0;298;0
WireConnection;108;0;129;0
WireConnection;113;0;112;0
WireConnection;113;2;111;0
WireConnection;254;0;113;0
WireConnection;386;0;56;0
WireConnection;386;1;58;0
WireConnection;386;2;388;0
WireConnection;388;0;392;0
WireConnection;388;1;385;3
WireConnection;339;0;386;0
WireConnection;231;0;56;4
WireConnection;0;0;346;0
WireConnection;0;1;384;0
WireConnection;0;5;382;0
WireConnection;0;7;354;0
WireConnection;0;10;234;0
WireConnection;0;11;153;0
ASEEND*/
//CHKSM=297BC053F59FC6D7AB4E098A0AC44597F243D575
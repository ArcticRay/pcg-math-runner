// Made with Amplify Shader Editor v1.9.2.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BK/Clouds"
{
	Properties
	{
		_ViewDistance("View Distance", Float) = 5000
		_DirectionSpeed("Direction / Speed", Vector) = (20,5,10,-5)
		[Header(Main)][Space(10)]_Noise01("Noise 01", 2D) = "white" {}
		_Tiling01("Tiling", Float) = 0.02
		_Noise01Power("Power", Range( 0 , 1)) = 0.75
		_Noise02("Noise 02", 2D) = "white" {}
		_Tiling02("Tiling", Float) = 0.03
		_Noise02Power("Power", Range( 0 , 1)) = 0.3
		_Normal("Normal", 2D) = "bump" {}
		_DistortionPower("Distortion Power", Range( 0 , 0.1)) = 0
		[Space(10)][Header(Clouds)][Space(10)]_ScatteringColor("Color", Color) = (1,1,1,1)
		_Coverage("Coverage", Range( 0 , 1)) = 0.3
		_Softness("Softness", Range( 0 , 1)) = 0.25
		[HideInInspector]_cloudsPosition("cloudsPosition", Float) = 1
		[Space(10)][Header(Scattering)][Space(10)]_CloudsColor("Color", Color) = (0.5849056,0.5849056,0.5849056,1)
		_ScatteringPower("Absorption", Range( 0 , 50)) = 0.1
		[HideInInspector]_cloudsHeight("cloudsHeight", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityStandardUtils.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		struct Input
		{
			float3 worldPos;
			float eyeDepth;
			float2 uv_texcoord;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float _cloudsPosition;
		uniform float _cloudsHeight;
		uniform float _Coverage;
		uniform float _ScatteringPower;
		uniform float4 _ScatteringColor;
		uniform float4 _CloudsColor;
		uniform float _ViewDistance;
		uniform sampler2D _Noise02;
		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float _DistortionPower;
		uniform float4 _DirectionSpeed;
		uniform float _Tiling02;
		uniform sampler2D _Noise01;
		uniform float _Tiling01;
		uniform float _Noise01Power;
		uniform float _Noise02Power;
		uniform float _Softness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float cameraDepthFade66 = (( i.eyeDepth -_ProjectionParams.y - 0.0 ) / _ViewDistance);
			float DistanceFade124 = ( 1.0 - cameraDepthFade66 );
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			float3 tex2DNode227 = UnpackScaleNormal( tex2D( _Normal, uv_Normal ), _DistortionPower );
			float2 appendResult230 = (float2(tex2DNode227.r , tex2DNode227.g));
			float2 appendResult161 = (float2(_DirectionSpeed.z , _DirectionSpeed.w));
			float3 ase_worldPos = i.worldPos;
			float2 appendResult4 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 panner159 = ( _Time.y * appendResult161 + appendResult4);
			float3 temp_cast_1 = (tex2D( _Noise02, ( appendResult230 + ( panner159 * ( _Tiling02 / 10000.0 ) ) ) ).r).xxx;
			float3 temp_cast_2 = (tex2D( _Noise02, ( appendResult230 + ( panner159 * ( _Tiling02 / 10000.0 ) ) ) ).r).xxx;
			float3 linearToGamma92 = LinearToGammaSpace( temp_cast_2 );
			float2 appendResult160 = (float2(_DirectionSpeed.x , _DirectionSpeed.y));
			float2 panner157 = ( _Time.y * appendResult160 + appendResult4);
			float3 temp_cast_3 = (tex2D( _Noise01, ( appendResult230 + ( panner157 * ( _Tiling01 / 10000.0 ) ) ) ).r).xxx;
			float3 temp_cast_4 = (tex2D( _Noise01, ( appendResult230 + ( panner157 * ( _Tiling01 / 10000.0 ) ) ) ).r).xxx;
			float3 linearToGamma91 = LinearToGammaSpace( temp_cast_4 );
			float3 blendOpSrc155 = linearToGamma92;
			float3 blendOpDest155 = ( linearToGamma91 * _Noise01Power );
			float3 lerpBlendMode155 = lerp(blendOpDest155,	max( blendOpSrc155, blendOpDest155 ),_Noise02Power);
			float3 Noise121 = ( saturate( lerpBlendMode155 ));
			float saferPower34 = abs( saturate( ( abs( ( _cloudsPosition - ase_worldPos.y ) ) / _cloudsHeight ) ) );
			float Coverage211 = _Coverage;
			float CloudHeight130 = ( 1.0 - pow( saferPower34 , ( 1.0 - Coverage211 ) ) );
			float3 temp_cast_5 = (( 1.0 - (0.0 + (Coverage211 - 0.0) * (1.0 - 0.0) / (0.98 - 0.0)) )).xxx;
			float3 saferPower19 = abs( saturate( (float3( 0,0,0 ) + (( Noise121 * CloudHeight130 ) - temp_cast_5) * (float3( 1,0,0 ) - float3( 0,0,0 )) / (float3( 1,0,0 ) - temp_cast_5)) ) );
			float3 temp_cast_6 = (_Softness).xxx;
			float3 CloudCoverage134 = pow( saferPower19 , temp_cast_6 );
			c.rgb = 0;
			c.a = saturate( ( DistanceFade124 * CloudCoverage134 ) ).x;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult109 = dot( ase_worldViewDir , -ase_worldlightDir );
			float saferPower114 = abs( saturate( dotResult109 ) );
			float temp_output_114_0 = pow( saferPower114 , 5.0 );
			float saferPower34 = abs( saturate( ( abs( ( _cloudsPosition - ase_worldPos.y ) ) / _cloudsHeight ) ) );
			float Coverage211 = _Coverage;
			float CloudHeight130 = ( 1.0 - pow( saferPower34 , ( 1.0 - Coverage211 ) ) );
			float saferPower197 = abs( CloudHeight130 );
			float temp_output_197_0 = pow( saferPower197 , _ScatteringPower );
			float4 Scattering119 = ( ( ( ase_lightColor * temp_output_114_0 ) + ( temp_output_114_0 * unity_AmbientSky * ase_lightColor ) ) * temp_output_197_0 );
			float Lerp223 = temp_output_197_0;
			float4 CloudLighting137 = ( Scattering119 + ( ( _ScatteringColor * Lerp223 ) + ( unity_FogColor * _CloudsColor ) ) );
			o.Emission = CloudLighting137.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha fullforwardshadows nofog vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
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
				o.customPack1.x = customInputData.eyeDepth;
				o.customPack1.yz = customInputData.uv_texcoord;
				o.customPack1.yz = v.texcoord;
				o.worldPos = worldPos;
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
				surfIN.eyeDepth = IN.customPack1.x;
				surfIN.uv_texcoord = IN.customPack1.yz;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19201
Node;AmplifyShaderEditor.CommentaryNode;123;-2432,-1920;Inherit;False;2941.861;834.6103;;30;228;227;233;232;143;2;10;9;147;121;155;148;151;92;91;150;158;161;160;157;3;22;159;141;142;25;4;23;162;230;Noise;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;131;-2432,-1024;Inherit;False;1409.247;449.601;;12;130;35;34;33;30;31;29;28;27;26;173;213;Clouds Height;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;27;-2400,-864;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;26;-2400,-960;Inherit;False;Property;_cloudsPosition;cloudsPosition;13;1;[HideInInspector];Create;True;0;0;0;False;0;False;1;1025;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;136;-2432,-384;Inherit;False;1538.37;354.3396;;13;211;16;134;19;20;18;17;90;37;122;189;132;212;Clouds Coverage;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;28;-2176,-912;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1536,-128;Inherit;False;Property;_Coverage;Coverage;11;0;Create;True;0;0;0;False;0;False;0.3;0.604;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-2064,-752;Inherit;False;Property;_cloudsHeight;cloudsHeight;16;1;[HideInInspector];Create;True;0;0;0;False;0;False;1;500;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;29;-2016,-912;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;211;-1088,-128;Inherit;False;Coverage;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;213;-2400,-672;Inherit;False;211;Coverage;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;30;-1856,-832;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;127;-2432,128;Inherit;False;2942.103;510.0996;;18;197;145;107;109;114;111;110;115;112;117;118;116;195;198;119;223;106;108;Scattering;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;33;-1728,-832;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;126;-2432,-2304;Inherit;False;891;255;;4;124;67;66;65;Distance Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;173;-2064,-656;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-2384,-2224;Inherit;False;Property;_ViewDistance;View Distance;0;0;Create;True;0;0;0;False;0;False;5000;6000;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;108;-2400,192;Float;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CameraDepthFade;66;-2176,-2240;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;35;-1408,-768;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;130;-1248,-768;Inherit;False;CloudHeight;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;67;-1920,-2240;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;124;-1760,-2240;Inherit;False;DistanceFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;219;-2432,768;Inherit;False;1150.069;638.0049;;10;137;225;224;234;210;200;202;178;199;120;Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;212;-2400,-128;Inherit;False;211;Coverage;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;189;-2112,-208;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.98;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;122;-2400,-320;Inherit;False;121;Noise;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;132;-2400,-224;Inherit;False;130;CloudHeight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-2208,-320;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;90;-1888,-192;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;17;-1728,-320;Inherit;False;5;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;1,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;18;-1536,-320;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-1536,-224;Inherit;False;Property;_Softness;Softness;12;0;Create;True;0;0;0;False;0;False;0.25;0.805;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;134;-1088,-320;Inherit;False;CloudCoverage;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;135;764.954,109.2661;Inherit;False;134;CloudCoverage;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;125;764.954,-18.73394;Inherit;False;124;DistanceFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;1020.954,45.26607;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;69;1148.954,45.26607;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;1020.954,-146.7339;Inherit;False;137;CloudLighting;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1272.543,-146.7339;Float;False;True;-1;2;;0;0;CustomLighting;BK/Clouds;False;False;False;False;False;False;False;False;False;True;False;False;False;False;True;True;True;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SimpleTimeNode;162;-2208,-1216;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-1792,-1536;Inherit;False;Property;_Tiling01;Tiling;3;0;Create;False;0;0;0;False;0;False;0.02;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;4;-2208,-1744;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;25;-1600,-1536;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;10000;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;-1792,-1232;Inherit;False;Property;_Tiling02;Tiling;6;0;Create;False;0;0;0;False;0;False;0.03;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;141;-1600,-1232;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;10000;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;159;-2016,-1296;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1344,-1600;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;3;-2400,-1792;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PannerNode;157;-2016,-1600;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;160;-2208,-1488;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;161;-2208,-1376;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;158;-2416,-1488;Inherit;False;Property;_DirectionSpeed;Direction / Speed;1;0;Create;True;0;0;0;False;0;False;20,5,10,-5;50,5,50,-20;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;150;-512,-1696;Inherit;False;Property;_Noise01Power;Power;4;0;Create;False;0;0;0;False;0;False;0.75;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LinearToGammaNode;91;-512,-1792;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LinearToGammaNode;92;-512,-1472;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-512,-1376;Inherit;False;Property;_Noise02Power;Power;7;0;Create;False;0;0;0;False;0;False;0.3;0.811;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-192,-1792;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendOpsNode;155;0,-1488;Inherit;False;Lighten;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;121;256,-1488;Inherit;False;Noise;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;9;-1344,-1808;Inherit;True;Property;_Noise01;Noise 01;2;0;Create;True;0;0;0;False;2;Header(Main);Space(10);False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;10;-832,-1808;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-832,-1488;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;143;-1344,-1296;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;232;-1008,-1696;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;233;-1008,-1392;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;230;-1344,-1888;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;227;-1728,-1776;Inherit;True;Property;_Normal;Normal;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;147;-1344,-1488;Inherit;True;Property;_Noise02;Noise 02;5;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;106;-2400,352;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;223;256,384;Inherit;False;Lerp;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;119;256,240;Inherit;False;Scattering;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;198;-464,480;Float;False;Property;_ScatteringPower;Absorption;15;0;Create;False;0;0;0;False;0;False;0.1;24.2;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;116;-1280,304;Inherit;False;3;3;0;FLOAT;1;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;-1280,192;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;117;-1120,240;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;112;-1536,352;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;115;-1536,480;Inherit;False;unity_AmbientSky;0;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-1696,288;Float;False;Constant;_ScatteringSize;5;14;0;Create;False;0;0;0;False;0;False;5;0.98;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;111;-1696,192;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;109;-1888,192;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;107;-2112,352;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;228;-1728,-1872;Inherit;False;Property;_DistortionPower;Distortion Power;9;0;Create;True;0;0;0;False;0;False;0;0;0;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;145;96,240;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-2400,1024;Inherit;False;223;Lerp;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;199;-2400,832;Inherit;False;Property;_ScatteringColor;Color;10;0;Create;False;0;0;0;False;3;Space(10);Header(Clouds);Space(10);False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;178;-2400,1120;Inherit;False;unity_FogColor;0;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;202;-2400,1216;Inherit;False;Property;_CloudsColor;Color;14;0;Create;False;0;0;0;False;3;Space(10);Header(Scattering);Space(10);False;0.5849056,0.5849056,0.5849056,1;0.5849056,0.5849056,0.5849056,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;200;-2144,960;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;210;-1888,1088;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;234;-2112,1152;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;-1920,896;Inherit;False;119;Scattering;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;225;-1664,976;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;137;-1536,976;Inherit;False;CloudLighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;195;-485.7,385.6;Inherit;False;130;CloudHeight;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;197;-160,384;Inherit;True;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;19;-1248,-320;Inherit;False;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;114;-1536,224;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;34;-1568,-768;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
WireConnection;28;0;26;0
WireConnection;28;1;27;2
WireConnection;29;0;28;0
WireConnection;211;0;16;0
WireConnection;30;0;29;0
WireConnection;30;1;31;0
WireConnection;33;0;30;0
WireConnection;173;0;213;0
WireConnection;66;0;65;0
WireConnection;35;0;34;0
WireConnection;130;0;35;0
WireConnection;67;0;66;0
WireConnection;124;0;67;0
WireConnection;189;0;212;0
WireConnection;37;0;122;0
WireConnection;37;1;132;0
WireConnection;90;0;189;0
WireConnection;17;0;37;0
WireConnection;17;1;90;0
WireConnection;18;0;17;0
WireConnection;134;0;19;0
WireConnection;68;0;125;0
WireConnection;68;1;135;0
WireConnection;69;0;68;0
WireConnection;0;2;139;0
WireConnection;0;9;69;0
WireConnection;4;0;3;1
WireConnection;4;1;3;3
WireConnection;25;0;23;0
WireConnection;141;0;142;0
WireConnection;159;0;4;0
WireConnection;159;2;161;0
WireConnection;159;1;162;0
WireConnection;22;0;157;0
WireConnection;22;1;25;0
WireConnection;157;0;4;0
WireConnection;157;2;160;0
WireConnection;157;1;162;0
WireConnection;160;0;158;1
WireConnection;160;1;158;2
WireConnection;161;0;158;3
WireConnection;161;1;158;4
WireConnection;91;0;10;1
WireConnection;92;0;2;1
WireConnection;148;0;91;0
WireConnection;148;1;150;0
WireConnection;155;0;92;0
WireConnection;155;1;148;0
WireConnection;155;2;151;0
WireConnection;121;0;155;0
WireConnection;10;0;9;0
WireConnection;10;1;232;0
WireConnection;2;0;147;0
WireConnection;2;1;233;0
WireConnection;143;0;159;0
WireConnection;143;1;141;0
WireConnection;232;0;230;0
WireConnection;232;1;22;0
WireConnection;233;0;230;0
WireConnection;233;1;143;0
WireConnection;230;0;227;1
WireConnection;230;1;227;2
WireConnection;227;5;228;0
WireConnection;223;0;197;0
WireConnection;119;0;145;0
WireConnection;116;0;114;0
WireConnection;116;1;115;0
WireConnection;116;2;112;0
WireConnection;118;0;112;0
WireConnection;118;1;114;0
WireConnection;117;0;118;0
WireConnection;117;1;116;0
WireConnection;111;0;109;0
WireConnection;109;0;108;0
WireConnection;109;1;107;0
WireConnection;107;0;106;0
WireConnection;145;0;117;0
WireConnection;145;1;197;0
WireConnection;200;0;199;0
WireConnection;200;1;120;0
WireConnection;210;0;200;0
WireConnection;210;1;234;0
WireConnection;234;0;178;0
WireConnection;234;1;202;0
WireConnection;225;0;224;0
WireConnection;225;1;210;0
WireConnection;137;0;225;0
WireConnection;197;0;195;0
WireConnection;197;1;198;0
WireConnection;19;0;18;0
WireConnection;19;1;20;0
WireConnection;114;0;111;0
WireConnection;114;1;110;0
WireConnection;34;0;33;0
WireConnection;34;1;173;0
ASEEND*/
//CHKSM=B09FAE483E80DCCD27CD734206FB2332D878DFF6
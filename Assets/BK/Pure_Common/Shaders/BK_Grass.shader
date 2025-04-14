// Made with Amplify Shader Editor v1.9.2.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BK/Grass"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.29
		_WindMultiplier("Wind Multiplier", Float) = 1
		_DepthFadeMultiplier("DepthFadeMultiplier", Float) = 1
		[Space(10)]_Color01("Color 01", Color) = (1,1,1,1)
		_Color02("Color 02", Color) = (1,1,1,1)
		[Space(10)]_MainTex("Texture", 2D) = "white" {}
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.1
		_ColorVariationPower("Color Variation Power", Range( 0 , 1)) = 1
		_Normal("Normal", 2D) = "bump" {}
		_NormalPower("Normal Power", Range( 0 , 1)) = 1
		[Toggle(_NORMALWORLDSPACEUVS_ON)] _NormalWorldSpaceUVs("Normal WorldSpace UVs", Float) = 0
		[Space(10)]_Noise("Noise", 2D) = "white" {}
		_NoiseTiling("Noise Tiling", Float) = 1.09
		[Toggle(_NOISEWORLDSPACEUVS_ON)] _NoiseWorldSpaceUVs("Noise WorldSpace UVs", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" }
		Cull Off
		AlphaToMask On
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.0
		#pragma multi_compile_instancing
		#pragma shader_feature_local _NORMALWORLDSPACEUVS_ON
		#pragma shader_feature_local _NOISEWORLDSPACEUVS_ON
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float customSurfaceDepth145;
		};

		uniform float MicroSpeed;
		uniform float MicroFrequency;
		uniform float MicroPower;
		uniform float _WindMultiplier;
		uniform sampler2D _Normal;
		uniform float _NoiseTiling;
		uniform float _NormalPower;
		uniform float4 _Color01;
		uniform float4 _Color02;
		uniform sampler2D _Noise;
		uniform float _ColorVariationPower;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Smoothness;
		uniform float GrassRenderDist;
		uniform float _DepthFadeMultiplier;
		uniform float _Cutoff = 0.29;


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


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 appendResult109 = (float3(ase_worldPos.z , ase_worldPos.y , ase_worldPos.x));
			float2 temp_cast_0 = (( MicroSpeed * 0.5 )).xx;
			float2 panner110 = ( 1.0 * _Time.y * temp_cast_0 + appendResult109.xy);
			float simplePerlin2D112 = snoise( panner110 );
			simplePerlin2D112 = simplePerlin2D112*0.5 + 0.5;
			float4 MicroWind125 = ( ( ( float4( ( ( ( sin( ( ( appendResult109 + simplePerlin2D112 ) * ( MicroFrequency * 2.0 ) ) ) * v.texcoord.xy.y ) * MicroPower ) * v.color.r ) , 0.0 ) * float4(12,3.6,1,1) ) * 0.05 ) * _WindMultiplier );
			v.vertex.xyz += MicroWind125.xyz;
			v.vertex.w = 1;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 customSurfaceDepth145 = ase_vertex3Pos;
			o.customSurfaceDepth145 = -UnityObjectToViewPos( customSurfaceDepth145 ).z;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float2 appendResult40 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 WorldSpaceUVs160 = ( appendResult40 * _NoiseTiling );
			#ifdef _NORMALWORLDSPACEUVS_ON
				float2 staticSwitch185 = WorldSpaceUVs160;
			#else
				float2 staticSwitch185 = i.uv_texcoord;
			#endif
			o.Normal = UnpackScaleNormal( tex2D( _Normal, staticSwitch185 ), _NormalPower );
			#ifdef _NOISEWORLDSPACEUVS_ON
				float2 staticSwitch192 = WorldSpaceUVs160;
			#else
				float2 staticSwitch192 = i.uv_texcoord;
			#endif
			float4 lerpResult48 = lerp( _Color01 , _Color02 , ( tex2D( _Noise, staticSwitch192 ).r * _ColorVariationPower ));
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode2 = tex2D( _MainTex, uv_MainTex );
			float4 _Albedo194 = ( lerpResult48 * tex2DNode2 );
			o.Albedo = _Albedo194.rgb;
			float3 hsvTorgb223 = RGBToHSV( tex2DNode2.rgb );
			o.Smoothness = ( _Smoothness * ( i.vertexColor.r * ( 1.0 - pow( hsvTorgb223.z , 1.5 ) ) ) );
			o.Alpha = 1;
			float _Alpha202 = tex2DNode2.a;
			float temp_output_239_0 = ( GrassRenderDist * _DepthFadeMultiplier );
			float cameraDepthFade145 = (( i.customSurfaceDepth145 -_ProjectionParams.y - ( temp_output_239_0 / 2.0 ) ) / max( temp_output_239_0 , 100.0 ));
			float DistanceFade150 = ( 1.0 - cameraDepthFade145 );
			float3 temp_cast_2 = ((1.5 + (i.uv_texcoord.y - 0.11) * (8.0 - 1.5) / (0.52 - 0.11))).xxx;
			float3 temp_cast_3 = ((1.5 + (i.uv_texcoord.y - 0.11) * (8.0 - 1.5) / (0.52 - 0.11))).xxx;
			float3 gammaToLinear146 = GammaToLinearSpace( temp_cast_3 );
			float clampResult149 = clamp( (gammaToLinear146).x , 0.0 , 1.0 );
			float BaseOpacity152 = clampResult149;
			clip( ( ( _Alpha202 * DistanceFade150 ) * BaseOpacity152 ) - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			AlphaToMask Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.0
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
				float3 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				half4 color : COLOR0;
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
				o.customPack1.z = customInputData.customSurfaceDepth145;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
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
				surfIN.customSurfaceDepth145 = IN.customPack1.z;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
Node;AmplifyShaderEditor.CommentaryNode;135;-3840,-384;Inherit;False;3196.689;476.6296;;26;125;213;214;130;133;124;122;123;121;120;118;119;116;117;115;113;114;111;112;110;109;126;108;127;235;236;Wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;127;-3808,-64;Float;False;Global;MicroSpeed;MicroSpeed;18;1;[HideInInspector];Create;False;0;0;0;False;0;False;2;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;108;-3808,-320;Float;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;-3568,-64;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;109;-3568,-304;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode;110;-3392,-192;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;112;-3200,-192;Inherit;False;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-3200,-64;Float;False;Global;MicroFrequency;MicroFrequency;19;1;[HideInInspector];Create;False;0;0;0;False;0;False;5;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;163;-3840,-2816;Inherit;False;1153.912;317.6656;World-Space UVs;0;UVs;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;114;-2944,-64;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;113;-2944,-304;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;115;-2752,-320;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;139;-3838,-2464;Inherit;False;1311.633;478.7;;9;238;237;142;231;145;147;150;240;239;Distance Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;140;-3841.647,-1916.839;Inherit;False;1312.323;284;;6;152;149;234;146;144;143;Base Opacity;1,1,1,1;0;0
Node;AmplifyShaderEditor.SinOpNode;117;-2528,-320;Inherit;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;116;-2560,-96;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;143;-3809.647,-1852.839;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;119;-2304,-320;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-2304,-96;Float;False;Global;MicroPower;MicroPower;20;0;Create;False;0;0;0;False;0;False;0.05;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;134;-3840,-1408;Inherit;False;2180.593;765.4294;;13;191;176;192;165;47;46;48;1;107;3;194;2;202;Diffuse / Colors;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;120;-2048,-96;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;144;-3585.647,-1852.839;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.11;False;2;FLOAT;0.52;False;3;FLOAT;1.5;False;4;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;121;-2048,-320;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;123;-1792,-320;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector4Node;122;-1792,-96;Inherit;False;Constant;_WaveAndDistance;WaveAndDistance;4;0;Create;True;0;0;0;False;0;False;12,3.6,1,1;12,3.6,1,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-1536,-224;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;133;-1536,-96;Inherit;False;Constant;_Float0;Float 0;8;0;Create;True;0;0;0;False;0;False;0.05;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;39;-3808,-2752;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;40;-3616,-2720;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-3616,-2608;Inherit;False;Property;_NoiseTiling;Noise Tiling;12;0;Create;True;0;0;0;False;0;False;1.09;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;178;-3424,-2688;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;160;-3040,-2688;Inherit;False;WorldSpaceUVs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;155;-976,-1920;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;203;-1408,-1920;Inherit;False;202;_Alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;195;-1152,-2816;Inherit;False;194;_Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;191;-3808,-1344;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;176;-3808,-1216;Inherit;False;160;WorldSpaceUVs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;192;-3584,-1296;Inherit;False;Property;_NoiseWorldSpaceUVs;Noise WorldSpace UVs;13;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;165;-3264,-1312;Inherit;True;Property;_Noise;Noise;11;0;Create;True;0;0;0;False;1;Space(10);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;46;-3248,-1072;Inherit;False;Property;_ColorVariationPower;Color Variation Power;7;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-2928,-1200;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;48;-2400,-1248;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;1;-2656,-1152;Inherit;False;Property;_Color02;Color 02;4;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;107;-2656,-1344;Inherit;False;Property;_Color01;Color 01;3;0;Create;True;0;0;0;False;1;Space(10);False;1,1,1,1;0.5613207,0.8245283,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;130;-1280,-224;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0.05;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;214;-1280,-96;Inherit;False;Property;_WindMultiplier;Wind Multiplier;1;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;-1056,-224;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-864,-224;Inherit;False;MicroWind;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;2;-2432,-896;Inherit;True;Property;_MainTex;Texture;5;0;Create;False;0;0;0;False;1;Space(10);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-2080,-1024;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;194;-1920,-1024;Inherit;False;_Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;38;-1288,-2046;Inherit;False;125;MicroWind;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-640,-2432;Float;False;True;-1;4;;0;0;Standard;BK/Grass;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.29;True;True;0;True;Opaque;;AlphaTest;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;5;False;;10;False;;0;5;False;;10;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;True;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;186;-2000,-2752;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;184;-2000,-2560;Inherit;False;160;WorldSpaceUVs;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;185;-1744,-2672;Inherit;False;Property;_NormalWorldSpaceUVs;Normal WorldSpace UVs;10;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;182;-1712,-2560;Inherit;False;Property;_NormalPower;Normal Power;9;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;181;-1424,-2688;Inherit;True;Property;_Normal;Normal;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;157;-1861,-2306;Inherit;False;Property;_Smoothness;Smoothness;6;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;218;-1958.423,-2147.891;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;228;-1723.804,-2094.988;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;217;-1525.918,-2299.233;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;226;-1941.425,-1909.415;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;229;-1792.433,-1912.442;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;154;-1184,-1792;Inherit;False;152;BaseOpacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GammaToLinearNode;146;-3313.647,-1852.839;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;234;-3104,-1856;Inherit;False;True;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;149;-2896,-1856;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;152;-2752,-1856;Inherit;False;BaseOpacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;223;-2128.009,-599.3849;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FunctionNode;235;-1002.241,-71.69711;Inherit;False;BK_Wind;-1;;1;511f08b81f352dd4e9774d4c87a1b504;0;0;2;FLOAT3;56;FLOAT;57
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;236;-805.0259,-14.06116;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;202;-1920,-800;Inherit;False;_Alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;153;-1408,-1792;Inherit;False;150;DistanceFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;-1193,-1922;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;150;-2752,-2240;Inherit;False;DistanceFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;145;-3192,-2236;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;147;-2907,-2236;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;238;-3401,-2256;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;231;-3412,-2143;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;239;-3585.213,-2190.525;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;237;-3444,-2413;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;240;-3808,-2160;Inherit;False;Property;_DepthFadeMultiplier;DepthFadeMultiplier;2;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;-3808,-2240;Inherit;False;Global;GrassRenderDist;GrassRenderDist;9;0;Create;True;0;0;0;False;0;False;5000;-72.5;0;0;0;1;FLOAT;0
WireConnection;126;0;127;0
WireConnection;109;0;108;3
WireConnection;109;1;108;2
WireConnection;109;2;108;1
WireConnection;110;0;109;0
WireConnection;110;2;126;0
WireConnection;112;0;110;0
WireConnection;114;0;111;0
WireConnection;113;0;109;0
WireConnection;113;1;112;0
WireConnection;115;0;113;0
WireConnection;115;1;114;0
WireConnection;117;0;115;0
WireConnection;119;0;117;0
WireConnection;119;1;116;2
WireConnection;144;0;143;2
WireConnection;121;0;119;0
WireConnection;121;1;118;0
WireConnection;123;0;121;0
WireConnection;123;1;120;1
WireConnection;124;0;123;0
WireConnection;124;1;122;0
WireConnection;40;0;39;1
WireConnection;40;1;39;3
WireConnection;178;0;40;0
WireConnection;178;1;179;0
WireConnection;160;0;178;0
WireConnection;155;0;156;0
WireConnection;155;1;154;0
WireConnection;192;1;191;0
WireConnection;192;0;176;0
WireConnection;165;1;192;0
WireConnection;47;0;165;1
WireConnection;47;1;46;0
WireConnection;48;0;107;0
WireConnection;48;1;1;0
WireConnection;48;2;47;0
WireConnection;130;0;124;0
WireConnection;130;1;133;0
WireConnection;213;0;130;0
WireConnection;213;1;214;0
WireConnection;125;0;213;0
WireConnection;3;0;48;0
WireConnection;3;1;2;0
WireConnection;194;0;3;0
WireConnection;0;0;195;0
WireConnection;0;1;181;0
WireConnection;0;4;217;0
WireConnection;0;10;155;0
WireConnection;0;11;38;0
WireConnection;185;1;186;0
WireConnection;185;0;184;0
WireConnection;181;1;185;0
WireConnection;181;5;182;0
WireConnection;228;0;218;1
WireConnection;228;1;229;0
WireConnection;217;0;157;0
WireConnection;217;1;228;0
WireConnection;226;0;223;3
WireConnection;229;0;226;0
WireConnection;146;0;144;0
WireConnection;234;0;146;0
WireConnection;149;0;234;0
WireConnection;152;0;149;0
WireConnection;223;0;2;0
WireConnection;236;0;235;56
WireConnection;202;0;2;4
WireConnection;156;0;203;0
WireConnection;156;1;153;0
WireConnection;150;0;147;0
WireConnection;145;2;237;0
WireConnection;145;0;238;0
WireConnection;145;1;231;0
WireConnection;147;0;145;0
WireConnection;238;0;239;0
WireConnection;231;0;239;0
WireConnection;239;0;142;0
WireConnection;239;1;240;0
ASEEND*/
//CHKSM=DF86AD5C6E7AC8858036F8A2D8F0C7D6755523C2
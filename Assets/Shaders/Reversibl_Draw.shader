Shader "Custom/Cutout Double-sided"
{
	Properties
	{
		_Cutoff ("Cutoff", Range(0,1)) = 0.5
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MainTex2("Texture2", 2D) = "white"{}
		_Glossiness ("Smoothness", Range(0,1)) = 0.1
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}

	SubShader
	{
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
		//Tags { "Queue" = "Transparent" "RenderType" = "TransparentCutout" }
		LOD 200

		Cull Off

CGPROGRAM

		//#pragma surface surf Standard alphatest:_Cutoff addshadow fullforwardshadows
		#pragma surface surf Standard addshadow  fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _MainTex2;

		struct Input
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;


		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float2 uvCoord;// = IN.uv_MainTex / 16.0;
			uvCoord.x = IN.uv_MainTex.x / 16.0;
			uvCoord.y = IN.uv_MainTex.y / 4.0;
			fixed4 c = tex2D (_MainTex2, IN.uv_MainTex) * _Color;
			//fixed4 c = tex2D(_MainTex2, uvCoord) * _Color;
			//o.Albedo = c.rgb;

			fixed3 finalColor = c.rgb * 2.0;// _Brightness;
			fixed gray = 0.2125 * c.r + 0.7154 * c.g + 0.0721 * c.b;
			fixed3 grayColor = fixed3(gray, gray, gray);
			finalColor = lerp(grayColor, finalColor, 0.8);// _Saturation);

			fixed3 avgColor = fixed3(0.5, 0.5, 0.5);
			finalColor = lerp(avgColor, finalColor, 0.6);// _Contrast);

			o.Albedo = finalColor;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			o.Albedo = pow(o.Albedo, 2.2f);//2021.10.12 added by pengyuan for gamma correction
		}

ENDCG

		Cull Front

CGPROGRAM

		//#pragma surface surf Standard alphatest:_Cutoff fullforwardshadows vertex:vert
		#pragma surface surf Standard fullforwardshadows vertex:vert
		//#pragma surface surf Standard vertex:vert
		#pragma target 3.0

		sampler2D _MainTex;


		struct Input
		{
			float2 uv_MainTex;
		};

		void vert (inout appdata_full v)
		{
			v.normal.xyz = v.normal * -1;
		}

		//half _Saturation = 1.0;
		//half _Brightness = 0.4;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;


		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float2 uvCoord;
			uvCoord.x = 1.0 - IN.uv_MainTex.x;
			uvCoord.y = IN.uv_MainTex.y;
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 c = tex2D(_MainTex, uvCoord) * _Color;

			fixed3 finalColor = c.rgb * 2.0;// _Brightness;
			fixed gray = 0.2125 * c.r + 0.7154 * c.g + 0.0721 * c.b;
			fixed3 grayColor = fixed3(gray, gray, gray);
			finalColor = lerp(grayColor, finalColor, 0.5);// _Saturation);

			fixed3 avgColor = fixed3(0.5, 0.5, 0.5);
			finalColor = lerp(avgColor, finalColor, 0.8);// _Contrast);

			o.Albedo = finalColor;
			//o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			o.Albedo = pow(o.Albedo,  2.2f);//2021.9.17 added by pengyuan for gamma correction
		}

ENDCG
	}

	FallBack "Diffuse"
}
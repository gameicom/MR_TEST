Shader "Custom/Rim" {		
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_RimColor("RimColor", Color) = (1,1,1,1) // 림라이트 색깔
		_RimPower("RimPower", Range(1,10)) = 5  // 림라이트 색세기
		_RimBlinkPower("RimBlinkPower", Range(1,10)) = 5  // 깜빡임시간
		_RimBlinkRange("RimBlinkRange", Range(0.5,1.0))  = 0.5 //깜빡임정도
		_LineCount("LineCount", Range(1,30)) = 5 // 선 갯수
		_LinePower("LinePower", Range(1,50)) = 30 // 선 넓이
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
		
		CGPROGRAM
		#pragma surface surf Lambert noambient alpha:fade

		sampler2D _MainTex;
		float4 _RimColor;
		float _RimPower;
		float _RimBlinkPower;
		float _RimBlinkRange;
		float _LineCount;
		float _LinePower;

		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			//o.Albedo = c.rgb;

			float rim = saturate(dot(o.Normal, IN.viewDir));
			rim = pow(1-rim,_RimPower) + pow(frac(IN.worldPos.g * _LineCount - _Time.y),_LinePower);

			o.Emission = _RimColor.rgb;
			o.Alpha = rim * (sin(_Time.y * _RimBlinkPower) * 0.5 + _RimBlinkRange);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
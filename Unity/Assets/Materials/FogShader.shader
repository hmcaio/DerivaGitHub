Shader "Custom/FogShader"
{
	Properties
	{
    	//_MainTex ("Base (RGB)", 2D) = "white" {}
		_ObjectColor ("Object color", Color) = (0.0, 0.0, 0.0, 1.0)
  	}
  	
  	SubShader
  	{
  		Tags { "RenderType"="Opaque" }
	    Fog {Mode  Off}
	    LOD 200
	    
	    CGPROGRAM
	    #pragma surface surf Lambert finalcolor:mycolor vertex:myvert

	    //sampler2D _MainTex;
		uniform fixed4 _ObjectColor;
	    uniform half4 unity_FogColor;
	    uniform half4 unity_FogStart;
	    uniform half4 unity_FogEnd;
	    uniform half4 unity_FogDensity;

	    struct Input
	    {
	    	float2 uv_MainTex;
	      	half fog;
	    };

	    void myvert (inout appdata_full v, out Input data)
	    {
	    	UNITY_INITIALIZE_OUTPUT(Input,data);
	      	float pos = length(mul (UNITY_MATRIX_MV, v.vertex).xyz);
	      	float diff = unity_FogEnd.x - unity_FogStart.x;

	      	//linear
	      	//float invDiff = 1.0f / diff;
	      	//data.fog = clamp ((unity_FogEnd.x - pos) * invDiff, 0.0, 1.0);

			//exp
			float f = pos * unity_FogDensity;
			data.fog = saturate(1 / pow(2.71828,  f));

			//exp2
			//float f = pos * unity_FogDensity;
			//data.fog = saturate(1 / pow(2.71828,  f * f));
	    }
	    
	    void mycolor (Input IN, SurfaceOutput o, inout fixed4 color)
	    {
	    	fixed3 fogColor = unity_FogColor.rgb;
	      	#ifdef UNITY_PASS_FORWARDADD
	      	fogColor = 0;
	      	#endif
	      	color.rgb = lerp (fogColor, color.rgb, IN.fog);
	    }

	    void surf (Input IN, inout SurfaceOutput o)
	    {
	    	//half4 c = tex2D (_MainTex, IN.uv_MainTex);
	      	//o.Albedo = c.rgb;
	      	//o.Alpha = c.a;

			o.Albedo = _ObjectColor.rgb;
	      	o.Alpha = _ObjectColor.a;
	    }
	    ENDCG
	}
	
	FallBack "Diffuse"
}
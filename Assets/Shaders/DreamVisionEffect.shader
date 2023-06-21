Shader "Custom/DreamVisionEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VisionColor ("Vision Color", Color) = (0, 0, 0, 1)
        _VisionIntensity ("Vision Intensity", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        fixed4 _VisionColor;
        float _VisionIntensity;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            c.rgb -= _VisionColor.rgb * _VisionIntensity;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
Shader "Custom/TestSurface"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.0, 0.4, 0.6, 1.0)
        _MainTex ("Normal Map", 2D) = "bump" {}
        _Speed ("Wave Speed", Float) = 0.5
        _Smoothness ("Smoothness", Range(0,1)) = 0.9

        _NormWave2 ("Wave2 Normal", 2D) = "bump" {}
        _Wave2xSpeed ("Wave2 Speed x", Float) = 0.3
        _wave2ySpeed ("Wave2 Speed y", Float) = 0.2
        //_wave2Velocity ("Wave 2 Velocity", float2) = (0.3, 0.2){}

        _wave2Concentration ("Wave2 Concentration", Float) = 0.5

        _foamSaturation ("Foam Saturation", float) = 0.5
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)

        _fadeSaturation ("Fade Saturation", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }//
        LOD 200
        //ZWrite Off
        //Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard alpha vertex:vert //alpha:fade
        #pragma target 3.0

        //sampler2D _CameraDepthTexture;

        sampler2D _MainTex;
        fixed4 _Color;
        float _Speed;
        float _Smoothness;

        sampler2D _NormWave2;
        float _Wave2xSpeed;
        float _wave2ySpeed;
        float _wave2Concentration;

        float _foamSaturation;
        fixed4 _FoamColor;

        float _fadeSaturation;

        struct Input
        {
           // float4 screenPos;
           // float eyeDepth;
            float2 uv_MainTex;
            float2 uv_NormWave2;
        };

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            //COMPUTE_EYEDEPTH(o.eyeDepth);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 scrolledUV = IN.uv_MainTex + float2(_Time.y * _Speed * 0.1, _Time.y * _Speed * 0.05);
            float2 scrolledUV2 = IN.uv_NormWave2 + float2(_Time.y * _Wave2xSpeed, _Time.y * _wave2ySpeed);
            float2 combinedUV = scrolledUV + (tex2D(_NormWave2, scrolledUV2).rg - 0.5) * _wave2Concentration;

            //half3 normal1 = UnpackNormal(tex2D(_MainTex, scrolledUV));
            //half3 normal2 = UnpackNormal(tex2D(_NormWave2, scrolledUV2));

            //o.Normal = normalize(lerp(normal1, normal2, _wave2Concentration));


            
            
            o.Smoothness = _Smoothness;


            // calculate depth
           // float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
            //float sceneZ = LinearEyeDepth(rawZ);
            //float waterSurface = IN.eyeDepth;
            //float fade = saturate(_fadeSaturation * (sceneZ - waterSurface));

            o.Normal = UnpackNormal(tex2D(_MainTex, combinedUV));


            //o.Albedo = lerp(_Color.rgb, _FoamColor.rgb, 1-fade);
            o.Albedo = _Color.rgb;

            o.Alpha = _Color.a;
            //o.Alpha = fade * _Color.a;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
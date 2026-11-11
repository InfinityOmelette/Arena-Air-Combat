Shader "Custom/TestSurface"
{
    Properties
    {
        [Header(Primary Wave Settings)]
        _Color ("Water Color", Color) = (0.0, 0.4, 0.6, 1.0)
        _ColorTex("Color Texture", 2D) = "white" {}
        _ColorLerp("Color Lerp", Range(0,1)) = 0.5

        _MainTex ("Normal Map", 2D) = "bump" {}
        _Speed ("Wave 1 Speed Modifier", Float) = 0.5
        _WaveVelocity("Wave 1 Velocity", Vector) = (0.3, 0.3, 0.0, 0.0)
        _Smoothness ("Smoothness", Range(0,1)) = 0.9

        [Header(Wave 2 (Normal Distortion) Settings)]
        _NormWave2 ("Wave2 Normal", 2D) = "bump" {}
        //_Wave2xSpeed ("Wave2 Speed x", Float) = 0.3
        //_wave2ySpeed ("Wave2 Speed y", Float) = 0.2
        _Wave2Velocity ("Wave2 Velocity", Vector) = (-.005, -.005, 0.0, 0.0)
        //_wave2Velocity ("Wave 2 Velocity", float2) = (0.3, 0.2){}
        _wave2Concentration ("Wave2 Concentration", Range(0,1)) = 0.5

        [Header(Wave 3 (Normal Blend) Settings)]
        _ColWave3 ("Wave3 Color", 2D) = "white" {}
        _NormWave3 ("Wave3 Normal", 2D) = "bump" {}
        _Wave3SpeedMod ("Wave3 Speed Modifier", Float) = 1.0
        _Wave3Velocity ("Wave3 Velocity", Vector) = (0.0, 0.0, 0.0, 0.0)
        _Wave3Concentration ("Wave3 Concentration", Range(0,1)) = 0.5
        _Wave3ColConcentration ("Wave3 Color Concentration", Range(0,1)) = 0.5

        [Header(Wave4 (Color Blend) Settings)]
        _ColWave4 ("Wave4 Color", 2D) = "white" {}
        _Wave4SpeedMod ("Wave4 SpeedModifier", Float) = 1.0
        _Wave4Velocity("Wave4 Velocity", Vector) = (0.0, 0.0, 0.0, 0.0)
        _Wave4ColConcentration ("Wave4 Color Concentration", Range(0,1)) = 0.5



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
        sampler2D _ColorTex;
        float _ColorLerp;
        fixed4 _Color;
        float _Speed;
        float4 _WaveVelocity;
        float _Smoothness;

        sampler2D _NormWave2;
        //float _Wave2xSpeed;
        //float _wave2ySpeed;
        float4 _Wave2Velocity;
        float _wave2Concentration;

        sampler2D _ColWave3;
        sampler2D _NormWave3;
        float _Wave3SpeedMod;
        float4 _Wave3Velocity;
        float _Wave3Concentration;
        float _Wave3ColConcentration;

        sampler2D _ColWave4;
        float _Wave4SpeedMod;
        float4 _Wave4Velocity;
        float _Wave4ColConcentration;        

        float _foamSaturation;
        fixed4 _FoamColor;

        float _fadeSaturation;

        

        struct Input
        {
           // float4 screenPos;
           // float eyeDepth;
            float2 uv_MainTex;
            float2 uv_NormWave2;
            float2 uv_NormWave3;
            float2 uv_ColWave3;
            float2 uv_ColWave4;
            //float4 _WaveVelocity;
            //float4 _Wave2Velocity;
        };

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            //COMPUTE_EYEDEPTH(o.eyeDepth);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 scrolledUV = IN.uv_MainTex + float2(_Time.y * _Speed * _WaveVelocity.x, _Time.y * _Speed * _WaveVelocity.y);
            float2 scrolledUV2 = IN.uv_NormWave2 + float2(_Time.y * _Wave2Velocity.x, _Time.y * _Wave2Velocity.y);
            float2 uvDistort = (tex2D(_NormWave2, scrolledUV2).rg - 0.5) * _wave2Concentration;
            float2 combinedUV_12 = scrolledUV + uvDistort;

            //half3 normal1 = UnpackNormal(tex2D(_MainTex, scrolledUV));
            //half3 normal2 = UnpackNormal(tex2D(_NormWave2, scrolledUV2));
            half3 normal12 = UnpackNormal(tex2D(_MainTex, combinedUV_12));

            float2 scrolledUV3 = uvDistort + IN.uv_NormWave3 + _Wave3SpeedMod * float2(_Time.y * _Wave3Velocity.x, _Time.y * _Wave3Velocity.y);
            half3 normal3 = UnpackNormal(tex2D(_NormWave3, scrolledUV3 + uvDistort));
            half3 fullNormal = normalize(lerp(normal12, normal3, _Wave3Concentration));
            //o.Normal = normalize(lerp(normal1, normal2, _wave2Concentration));
            o.Normal = fullNormal;

            float2 scrolledUV4 = IN.uv_ColWave4 + _Wave4SpeedMod * float2(_Time.y * _Wave4Velocity.x, _Time.y * _Wave4Velocity.y);
            
            o.Smoothness = _Smoothness;


            // calculate depth
           // float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
            //float sceneZ = LinearEyeDepth(rawZ);
            //float waterSurface = IN.eyeDepth;
            //float fade = saturate(_fadeSaturation * (sceneZ - waterSurface));

            //o.Normal = UnpackNormal(tex2D(_MainTex, combinedUV_12));


            //o.Albedo = lerp(_Color.rgb, _FoamColor.rgb, 1-fade);
            //o.Albedo = tex2D(_ColorTex, scrolledUV).rgb;
            //o.Albedo = _Color.rgb;
            fixed3 colorTex = tex2D(_ColorTex, combinedUV_12).rgb;
            //fixed3 mainColMix = lerp(_Color.rgb, colorTex, _ColorLerp);
            fixed3 colorTex2 = tex2D(_ColWave3, scrolledUV3).rgb;
            fixed3 mixColor = lerp(colorTex, colorTex2, _Wave3ColConcentration);

            fixed3 colorTex3 = tex2D(_ColWave4, scrolledUV4).rgb;
            fixed3 mixColor2 = lerp(mixColor, colorTex3, _Wave4ColConcentration);

            o.Albedo = lerp(_Color.rgb, mixColor2, _ColorLerp);

            o.Alpha = _Color.a;
            //o.Alpha = fade * _Color.a;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
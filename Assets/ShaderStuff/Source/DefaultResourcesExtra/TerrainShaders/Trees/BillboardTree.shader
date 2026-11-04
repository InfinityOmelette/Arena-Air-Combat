// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/TerrainEngine/BillboardTree" {
    Properties {
        _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
    }

    SubShader {
        Tags { "Queue" = "Transparent-100" "IgnoreProjector"="True" "RenderType"="TreeBillboard" }

        Pass {
            ColorMask rgb
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "TerrainEngine.cginc"

            float _fadeEndDist;
            float _fadeStartPercent;
            float _testFadeEndDist = 500;

            struct v2f {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR0;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
                float fadeFactor : TEXCOORD2;
            };

            v2f vert (appdata_tree_billboard v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                TerrainBillboardTree(v.vertex, v.texcoord1.xy, v.texcoord.y);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.x = v.texcoord.x;
                o.uv.y = v.texcoord.y > 0;
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.pos);


                // Calculate fade factor based on distance
                float3 viewPos = UnityObjectToViewPos(v.vertex).xyz;
                float viewDepth = -viewPos.z;
                float fadeStartDist = _fadeEndDist * _fadeStartPercent;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 camPos = _WorldSpaceCameraPos;
                float distanceToCamera = distance(worldPos, camPos);
                float distLerp = (distanceToCamera - fadeStartDist) / (_fadeEndDist - fadeStartDist);
                o.fadeFactor = 1.0 - clamp(distLerp, 0.0, 1.0);
                
                
                return o;
            }

            sampler2D _MainTex;
            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 col = tex2D( _MainTex, input.uv);
                col.rgb *= input.color.rgb;
                col.a *= input.fadeFactor; // Apply fade factor to alpha 

                clip(col.a);
                UNITY_APPLY_FOG(input.fogCoord, col);

                //fixed4 shitCol = fixed4(input.fadeFactor, 0, 0, col.a);
                return col;
            }
            ENDCG
        }
    }

    Fallback Off
}

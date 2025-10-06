// Copyright (c) Meta Platforms, Inc. and affiliates.

Shader "MRMotifs/SDFShader"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _EdgeSharpness("Edge Sharpness", Range(1,20)) = 8.0
        _Level("Level", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha
        BlendOp Add

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _EdgeSharpness;
            float _Level;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID        // 👈 for XR single-pass instancing
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO            // 👈 per-eye data
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);            // 👈 initialize instance for current eye
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);  // 👈 ensure correct eye sampling

                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // Basic SDF dissolve logic (preserved from your original intent)
                float dist = tex2D(_MainTex, i.uv).r;
                float mask = smoothstep(
                    _Level - (1.0 / _EdgeSharpness),
                    _Level + (1.0 / _EdgeSharpness),
                    dist
                );

                col.a *= mask;
                return col;
            }
            ENDHLSL
        }
    }

    FallBack Off
}

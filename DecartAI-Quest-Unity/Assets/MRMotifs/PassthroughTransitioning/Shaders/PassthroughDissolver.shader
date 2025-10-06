// Copyright (c) Meta Platforms, Inc. and affiliates.

Shader "MRMotifs/SelectivePassthroughDissolver"
{
    Properties
    {
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _Level("Dissolution Level", Range(0,1)) = 0.0
        _EdgeSharpness("Edge Sharpness", Range(1,20)) = 8.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-1" }
        LOD 200

        Cull Off
        ZWrite Off
        ZTest Always

        // 🔸 This blend setup tells the compositor to "show passthrough here"
        // SrcAlpha = 0 means no virtual color, only passthrough.
        // Alpha channel drives passthrough visibility.
        Blend Zero OneMinusSrcAlpha
        BlendOp Add

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _NoiseTex;
            float _Level;
            float _EdgeSharpness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float noise = tex2D(_NoiseTex, i.uv).r;

                // Dissolve mask controls where passthrough shows through
                float mask = saturate((noise - _Level) * _EdgeSharpness);

                // alpha = 1 means passthrough fully visible (virtual hidden)
                // alpha = 0 means normal virtual view
                float alpha = mask;

                // No color contribution — only passthrough control
                return float4(0, 0, 0, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

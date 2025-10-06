Shader "MRMotifs/SelectivePassthroughDissolverStereo"
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

        // Premultiplied-style passthrough blending
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float noise = tex2D(_NoiseTex, i.uv).r;

                // Dissolve control
                float mask = saturate((noise - _Level) * _EdgeSharpness);

                // alpha = passthrough amount (1 = passthrough, 0 = virtual)
                float alpha = mask;

                // No RGB output, only alpha for passthrough blend
                return float4(0, 0, 0, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

Shader "Custom/SDFShaderEdgeStretch"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _EdgeFade("Edge Fade Width", Range(0,0.3)) = 0.15
        _StretchAmount("Edge Stretch Amount", Range(0,0.2)) = 0.05
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _EdgeFade;
            float _StretchAmount;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float2 uv:TEXCOORD0; float4 vertex:SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 StretchUV(float2 uv, float stretch)
            {
                // squeeze inner region so 0..stretch and 1-stretch..1 map to exact edges
                float2 stretched = uv;
                stretched.x = (uv.x < stretch) ? 0.0 :
                              (uv.x > 1.0 - stretch) ? 1.0 :
                              (uv.x - stretch) / (1.0 - 2.0 * stretch);
                stretched.y = (uv.y < stretch) ? 0.0 :
                              (uv.y > 1.0 - stretch) ? 1.0 :
                              (uv.y - stretch) / (1.0 - 2.0 * stretch);
                return stretched;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = StretchUV(i.uv, _StretchAmount);
                fixed4 col = tex2D(_MainTex, uv) * _Color;

                // fade edges outward
                float2 edge = smoothstep(0.0, _EdgeFade, i.uv) *
                              (1.0 - smoothstep(1.0 - _EdgeFade, 1.0, i.uv));
                float fade = edge.x * edge.y;
                col.a *= fade;

                return col;
            }
            ENDCG
        }
    }
}

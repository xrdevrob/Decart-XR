Shader "Robertolino/UI/SDF Shader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FadeWidth ("Fade Width", Range(0.01, 0.5)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _FadeWidth;
            // Signed Distance Function for a box
            float sdBox(float2 p, float2 b)
            {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // Apply tint and vertex color
                col *= _Color * i.color;

                // Convert UV to centered coordinates (-0.5 to 0.5)
                float2 centeredUV = i.uv - 0.5;

                // Box half-size (covers the full UV space)
                float2 boxSize = float2(0.5, 0.5);

                // Calculate signed distance
                float dist = sdBox(centeredUV, boxSize);

                // Create alpha fade (1.0 at center, 0.0 at edges)
                // dist is negative inside, positive outside
                float alphaFade = saturate(-dist / _FadeWidth);

                // Apply fade to alpha
                col.a *= alphaFade;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}

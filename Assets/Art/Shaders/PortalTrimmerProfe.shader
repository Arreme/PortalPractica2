Shader "Tecnocampus/PortalShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MaskTex("Mask texture", 2D) = "white" {}
        _Cutout("Cutout", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags{ "RenderQueue" = "Geometry " "IgnoreProjector" = "True" "RenderType" = "Opaque" }
        Lighting Off
        Cull Back
        ZWrite On
        ZTest Less

        Fog{ Mode Off }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define TAU 6.2831853071
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            float GetWave(float uv) {
                float2 uvsCentered = uv * 2 - 1;
                float radialDistance = length( uvsCentered );
                float wave = cos((radialDistance - _Time.y * 0.1) * TAU * 5) * 0.5 + 0.5;
                wave *= 1 - radialDistance;
                return wave;
            }

            v2f vert(appdata v)
            {
                v2f o;
                v.vertex.y = GetWave(v.uv);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float _Cutout;
            fixed4 frag(v2f i) : SV_Target
            {
                i.screenPos /= i.screenPos.w;
                fixed4 l_MaskColor = tex2D(_MaskTex, i.uv);
                if (l_MaskColor.a < _Cutout)
                    clip(-1);
                fixed4 col = tex2D(_MainTex, float2(i.screenPos.x, i.screenPos.y));
                return col;
            }
            ENDCG
        }
    }
}

Shader "Catacumba/Item Highlight"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        Pass
        {
            Tags {
                "LightMode" = "Entity"
                "RenderType" = "Transparent"
                "Queue"= "Transparent"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed2 toPolar(fixed2 uv)
            {
                return fixed2(length(uv), atan2(uv.y, uv.x));
            }

            float stripes(fixed2 pc) // polar coords
            {
                return max(0.75, step(0.5, frac(pc.y*3.141592)));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = (i.uv - 0.5) * 2.;
                float2 pc = toPolar(uv);

                float res = 5.;
                //float a = (floor(saturate(1. - pc.x) * res) % res) / res ;
                pc.x = pow(pc.x, 2.0);
                float t = frac(pc.x - _Time.y); 
                float a = saturate(1. - t); // - step(t, 0.35);
                a = (floor(saturate(a) * res) % res) / res ;

                float a2 = saturate(1. - pc.x);
                a2 = pow(a2, 0.25);
                a *= a2;

                float4 clr = _Color * a;
                return clr;
            }
            ENDCG
        }
    }
}

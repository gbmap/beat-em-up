Shader "Catacumba/Particle"
{
    Properties
    {
        _MainTex ("Mask", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "LightMode" = "Entity"
            "RenderType" = "Transparent"
            "Queue"= "Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                fixed4 color : COLOR; 
                float4 animBlend : TEXCOORD1;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 animBlend : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.uv.zw, _MainTex);

                o.color = v.color;
                o.animBlend = v.animBlend;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col1 = tex2D(_MainTex, i.uv.xy);
                fixed4 col2 = tex2D(_MainTex, i.uv.zw);
                //return col;
                return i.color * lerp(col1, col2, i.animBlend.x) * col1.a * col2.a;
            }
            ENDCG
        }
    }
}

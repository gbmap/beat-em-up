Shader "Catacumba/Health_Quad"
{
    Properties
    {
        _HighHealthColor ("Full Health Color", Color) = (0,0,0,0)
        _LowHealthColor ("Low Health Color", Color) = (0,0,0,0)
        _PoiseBarColor ("Stamina Color", Color) = (0,0,0,0)
        _HealthPercentage ("Health Percentage", Range(0, 1)) = 0.0 
        _PoisePercentage ("Poise Percentage", Range(0,1)) = 0.0
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

            float4 _HighHealthColor;
            float4 _LowHealthColor;
            float4 _PoiseBarColor;
            float  _HealthPercentage;
            float  _PoisePercentage;

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

                float hpBarOuterSDF = 1. - step(1., length(uv));
                float hpBarInnerSDF = 1. - step(.8, length(uv));
                float hpBarSDF      = hpBarOuterSDF - hpBarInnerSDF;
                float hpBarMaskSDF  = hpBarSDF * (1. - step(_HealthPercentage-.5, pc.y/(2.*3.141592)));
                float staminaBarOuterSDF = (1. - step(.7, length(uv)));
                float staminaBarSDF = hpBarInnerSDF - staminaBarOuterSDF;
                      staminaBarSDF *= (1. - step(_PoisePercentage-.5, pc.y/(2.*3.141592)));

                fixed4 healthClr = lerp(_LowHealthColor, _HighHealthColor, _HealthPercentage*_HealthPercentage); 

                float stripesSDF = stripes(pc);

                float4 innerGlow  = (pow(pc.x, sin(_Time.y)*1.0+7.0)*hpBarInnerSDF) * healthClr;
                float4 hpBar      = lerp(fixed4(.05,.04,.05,.8)*hpBarSDF, healthClr, hpBarMaskSDF);
                float4 staminaBar = _PoiseBarColor*staminaBarSDF; 

                float4 finalColor = max(max(staminaBar, hpBar)*stripesSDF, innerGlow); 
                return finalColor;
                       //col = lerp(fixed4(.2,.2,.2, .2), _PoiseBarColor, staminaBarSDF);
                //col *= stripes(pc);

                //return col;
            }
            ENDCG
        }
    }
}

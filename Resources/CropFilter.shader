// 
//   NatCorder Performance Extensions
//   Copyright (c) 2021 Yusuf Olokoba.
//

Shader "Hidden/NCPX/CropFilter" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4x4 _Transform;

            fixed4 frag (v2f_img i) : SV_Target {
                float4 uv = mul(_Transform, float4(i.uv.xy, 0.0, 1.0));
                if (any(uv < 0.0) || any(uv > 1.0))
                    return float4(0.0, 0.0, 0.0, 1.0);
                return tex2D(_MainTex, uv.xy);
            }
            ENDCG
        }
    }
}

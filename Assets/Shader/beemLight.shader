Shader "Unlit/Gradation_AlphaRange_Unlit"
{
    Properties
    {
        _Color1("Color_1 (Bottom)" , Color) = (1,1,1,1)
        _Color2("Color_2 (Top)" , Color) = (1,1,1,1)

        [Header(Alpha Settings)]
        _AlphaMin("Alpha (Bottom)", Range(0, 1)) = 0.0
        _AlphaMax("Alpha (Top)", Range(0, 1)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 法線は不要になったため削除
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // 法線は不要になったため削除
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color1;
            float4 _Color2;
            float _AlphaMin;
            float _AlphaMax;

            // 法線は不要になったためvert関数をシンプルに戻す
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // グラデーション色を計算
                fixed4 gradientColor = lerp(_Color1, _Color2, i.uv.y);

                // ライティング計算をすべて削除
                // finalRGBはグラデーションのRGB値をそのまま使用
                fixed3 finalRGB = gradientColor.rgb;
                
                // アルファ値の計算
                float verticalAlpha = lerp(_AlphaMin, _AlphaMax, i.uv.y);
                float finalAlpha = gradientColor.a * verticalAlpha;
                
                // 最終的な色を決定
                return fixed4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
}
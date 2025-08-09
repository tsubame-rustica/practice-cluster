Shader "Custom/SimpleSpotLight"
{
    Properties
    {
        _Color ("Light Color", Color) = (1,1,1,0.5)
        _Radius ("Radius", Range(0.1, 2.0)) = 1.0
        _Softness ("Edge Softness", Range(0.01, 1.0)) = 0.3
        _Brightness ("Brightness", Range(0, 5)) = 1.5
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "PreviewType"="Plane"
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            fixed4 _Color;
            float _Radius;
            float _Softness;
            float _Brightness;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 中心からの距離を計算
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.texcoord, center) * 2.0; // 0-1の範囲に正規化
                
                // スポットライトのマスク計算
                float mask = 1.0 - smoothstep(_Radius - _Softness, _Radius, dist);
                
                // 中心からの距離による明るさの調整
                float centerBrightness = 1.0 - (dist * 0.5);
                mask *= centerBrightness;
                
                // 最終色
                fixed4 col = _Color;
                col.rgb *= _Brightness;
                col.a *= mask;
                
                return col;
            }
            ENDCG
        }
    }
}

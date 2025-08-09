Shader "Custom/SpotLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Light Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0, 5)) = 1.0
        _InnerRadius ("Inner Radius", Range(0, 1)) = 0.2
        _OuterRadius ("Outer Radius", Range(0, 1)) = 0.8
        _Falloff ("Falloff", Range(0.1, 5)) = 1.0
        _DistanceFade ("Distance Fade", Range(0.1, 10)) = 2.0
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 objectPos : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Intensity;
            float _InnerRadius;
            float _OuterRadius;
            float _Falloff;
            float _DistanceFade;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.objectPos = v.vertex.xyz;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // UV座標を中心から放射状に計算
                float2 center = float2(0.5, 0.5);
                float2 uv = i.uv - center;
                float dist = length(uv);
                
                // スポットライトの円形グラデーション
                float spotMask = 1.0 - smoothstep(_InnerRadius, _OuterRadius, dist);
                
                // 距離による減衰
                float3 objCenter = float3(0, 0, 0);
                float objDist = length(i.objectPos - objCenter);
                float distanceFade = 1.0 / (1.0 + objDist * _DistanceFade);
                
                // フォールオフ（光の強度カーブ）
                spotMask = pow(spotMask, _Falloff);
                
                // テクスチャサンプリング
                fixed4 tex = tex2D(_MainTex, i.uv);
                
                // 最終色計算
                fixed4 col = tex * _Color * _Intensity;
                col.a *= spotMask * distanceFade;
                
                // フォグ適用
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Unlit/Transparent"
}

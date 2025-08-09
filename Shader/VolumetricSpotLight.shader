Shader "Custom/VolumetricSpotLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Light Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0, 10)) = 2.0
        _ConeAngle ("Cone Angle", Range(0, 1)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.1
        _DistanceAttenuation ("Distance Attenuation", Range(0.1, 5)) = 1.0
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2.0
        _NoiseSpeed ("Noise Speed", Range(0, 5)) = 1.0
        _VolumetricDensity ("Volumetric Density", Range(0, 2)) = 0.8
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+100"
            "IgnoreProjector"="True"
        }
        LOD 200
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Front
        
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
                float3 localPos : TEXCOORD2;
                float depth : TEXCOORD3;
                UNITY_FOG_COORDS(4)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Intensity;
            float _ConeAngle;
            float _EdgeSoftness;
            float _DistanceAttenuation;
            float _NoiseScale;
            float _NoiseSpeed;
            float _VolumetricDensity;
            
            // シンプルなノイズ関数
            float noise(float3 pos)
            {
                return frac(sin(dot(pos, float3(12.9898, 78.233, 45.164))) * 43758.5453);
            }
            
            float fbm(float3 pos)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for(int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(pos * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                return value;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.localPos = v.vertex.xyz;
                o.depth = -UnityObjectToViewPos(v.vertex).z;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // ローカル座標でのスポットライトの計算
                float3 lightDir = normalize(float3(0, 0, -1)); // Z軸負の方向
                float3 toFragment = normalize(i.localPos);
                
                // スポットライトの角度計算
                float cosAngle = dot(-toFragment, lightDir);
                float spotEffect = smoothstep(_ConeAngle - _EdgeSoftness, _ConeAngle + _EdgeSoftness, cosAngle);
                
                // 距離による減衰
                float distance = length(i.localPos);
                float attenuation = 1.0 / (1.0 + distance * _DistanceAttenuation);
                
                // ボリューメトリックノイズ
                float3 noisePos = i.worldPos * _NoiseScale + _Time.y * _NoiseSpeed;
                float volumetricNoise = fbm(noisePos) * _VolumetricDensity;
                
                // UV座標からの追加マスク
                float2 center = float2(0.5, 0.5);
                float uvDist = distance(i.uv, center);
                float uvMask = 1.0 - smoothstep(0.3, 0.8, uvDist);
                
                // テクスチャサンプリング
                fixed4 tex = tex2D(_MainTex, i.uv);
                
                // 最終的な強度計算
                float finalIntensity = spotEffect * attenuation * uvMask * (0.5 + volumetricNoise);
                
                // 色の計算
                fixed4 col = tex * _Color * _Intensity;
                col.a *= finalIntensity;
                
                // 深度によるフェード（オプション）
                col.a *= saturate(1.0 - i.depth * 0.1);
                
                // フォグ適用
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Unlit/Transparent"
}

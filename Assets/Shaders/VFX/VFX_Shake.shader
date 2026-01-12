Shader "VFX/Shake" {
    Properties {
        _Texture ("Texture", 2D) = "white" {}  
        _ShakeFrequency ("Shake Frequency", Float) = 1.0  // 一秒shake多少次，来回算一次
        _PivotPoint ("Pivot Point", Vector) = (0.5, 0.5, 0, 0)  // 旋转中心点(UV空间)
        _ShakeAngle ("Shake Angle", Float) = 1
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Texture;
            float _ShakeFrequency;
            float _ShakeAngle;
            float2 _PivotPoint;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 计算旋转角度（带方向控制）
                int updown = sin(_Time.x * 3.14 * _ShakeFrequency ) > 0 ? 1 : -1;
                float angle = _ShakeAngle * 3.14 / 180 * updown;
                
                // 将UV坐标平移到旋转中心
                float2 uv = i.uv - _PivotPoint;
                
                // 构造旋转矩阵
                float sinRot, cosRot;
                sincos(angle, sinRot, cosRot);
                float2x2 rotMatrix = float2x2(cosRot, -sinRot,
                                             sinRot, cosRot);
                
                // 应用旋转并移回原位置
                uv = mul(rotMatrix, uv)* 1.903 + _PivotPoint;
                

                // 采样纹理
                fixed4 col = tex2D(_Texture, uv) * step(0, uv.x) * step(uv.x, 1) * step(0, uv.y) * step(uv.y, 1);

                

                return col;
            }
            ENDCG
        }
    }
}
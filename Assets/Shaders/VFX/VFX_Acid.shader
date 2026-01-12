Shader "VFX/Acid" {
    Properties {
        _Texture ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _LerpDistance("LerpDistance",Float)  = 0
    }
    SubShader {
        Tags { 
            "Queue"="Transparent"     
            "RenderType"="Transparent"
            "IgnoreProjector"="True"  //  防止投影干扰
        }
        
        ZTest [unity_GUIZTestMode]
        ZWrite Off

        //  核心设置：启用混合 + 关闭深度写入
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
            float4 _Color;
            float _LerpDistance;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv ;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_Texture, frac(i.uv + float2(0,_Time.y)));
             col.a =col.a * lerp(0,1,(i.uv.y >0.5?(1-i.uv.y ):i.uv.y )/ _LerpDistance);

                return fixed4(col.rgb * _Color.rgb,col.a *_Color.a); 
            }
            ENDCG
        }
    }
}
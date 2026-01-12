Shader "VFX/BounceEffect"
{
    Properties
    {
        _MainTex ("MainTexture", 2D) = "white" {}
        _Speed("Speed",Float) = 1
        _DuangRange("DuangRange",Vector) = (0.9,1.1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"
               "Queue"="Transparent"  
               "IgnoreProjector"="True" 
            }
        LOD 100

        ZTest [unity_GUIZTestMode]
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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

            sampler2D _MainTex;
            float _Speed;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            Vector _DuangRange;

            v2f vert (appdata v)
            {
                v2f o;
                float t = _Time.y+_MainTex_TexelSize.z/_MainTex_TexelSize.w;
                float pi = 3.14;
                o.vertex = UnityObjectToClipPos(v.vertex );
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv =(o.uv - float2(0.5,0.5))* float2(lerp(_DuangRange.x,_DuangRange.y,(sin(t*pi *_Speed)+1)/2),lerp(_DuangRange.z,_DuangRange.w,(cos(t*pi *_Speed)+1)/2))+ float2(0.5,0.5);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col.a = (i.uv.x>1||i.uv.x<0||i.uv.y<0||i.uv.y>1)?0:col.a;
                return col;
            }
            ENDCG
        }
    }
}

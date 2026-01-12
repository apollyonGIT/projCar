Shader "VFX/Roll"
{
    Properties
    {
        _MainTex ("MainTexture", 2D) = "white" {}
        _RollDirection("Roll Direction",Vector) = (10,0,0)
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
            Vector _RollDirection;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex );
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
              float2  remapUV = frac(i.uv + _RollDirection * _Time.x);
                fixed4 col = tex2D(_MainTex, remapUV);

                return col;
            }
            ENDCG
        }
    }
}

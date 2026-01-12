Shader "Weather/BubbleVisibleArea"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _BubbleRadius("Bubble Radius",Float) = 1
        _BubbleRate("Bubble Rate",Float) = 1
        _SoftDistance("Soft Distance",Float) = 1
        _Bubble_Zero_Alpha ("zero alpha", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
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
                float4 pos_object : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _BubbleRadius;
            float _BubbleRate;
            float _SoftDistance;
            float _Bubble_Zero_Alpha;
            float _Arg1;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.pos_object = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                
                // ©ийсещещ
                float i_dis = i.pos_object.x*i.pos_object.x*_BubbleRate*_BubbleRate+i.pos_object.y*i.pos_object.y;
                float bubble_radius_pow = _BubbleRadius*_BubbleRadius;
                float colMulRate = 1;
                float distance_rate = clamp((bubble_radius_pow - i_dis + _SoftDistance)/(bubble_radius_pow ) * 3.14,0,3.14);

                
                colMulRate = 0.5*cos(distance_rate)+0.5;
                if(colMulRate < _Bubble_Zero_Alpha) colMulRate = _Bubble_Zero_Alpha;
                
                col.a = _Arg1 * colMulRate;

                return col;
            }
            ENDCG
        }
    }
}

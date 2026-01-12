Shader "Weather/Weather_Lightning"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Alpha("Alpha", Float) = 1
        _LightningWidth("Lightning Width", Float) = 1
        _Seg("Segment", Float) = 100
        _Feq("Fequency", Float) = 100
        _DeltaTime("Delta Time", Float) = 0.02
        _Seed("Seed", Float) = 1
        _Width("Width",Float) = 1
        _TimeLine("Time Line",Float) = 0
        _YunWidth("Yun Width", Float) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        // 低分辨率化波形，波形点之间进行插值

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
            float4 _MainTex_ST;
            float _LightningWidth;
            float    _Seg;
            float _Feq;
            float _DeltaTime;
            float _Seed;
            fixed4 _Color;
            float _TimeLine;
            float _Alpha;
            float _YunWidth;
            float _Width;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            float Rand(float s)
            {
                return frac(sin(s) * 43758.5453123 * _Seed)/2 + 0.5;
            }
            float Formula(float x)
            {
                float rand = Rand(floor((_TimeLine)/_DeltaTime)*_DeltaTime);
                return (sin(0.5 *rand * _Feq * x) + sin(0.2  *rand * _Feq* x) + cos(0.02 *rand * _Feq * x))/5 * _Width + 0.5;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Remap Y
                float y = i.uv.y;
                y = y + ((sin(0.5 * y) + sin(0.2 * y) + cos(0.02 * y)))/3 * 5;
                float unitSeg = 1 / _Seg;
                float segIndex = floor(y / unitSeg);
                // 计算上一个y
                float y0 = segIndex * unitSeg;
                // 计算下一个y
                float y1 = segIndex * unitSeg + unitSeg;
                // 计算插值
                float t = fmod(y , unitSeg) / unitSeg;

                float x = lerp(Formula(y0),Formula(y1),t);
                float x_noSeg = (Formula(y)-0.5) / 5  + 0.5;

                fixed4 col;

                if(abs(i.uv.x - x)<_LightningWidth)
                {
                    col = fixed4(_Alpha >= 0? _Color.rgb : float3(1,1,1),abs(_Alpha));
                }
                else if(abs(i.uv.x - x) < _LightningWidth + _YunWidth)
                {
                    float t = (abs(i.uv.x - x) - _LightningWidth) / _YunWidth;
                    col = fixed4(_Alpha >= 0? _Color.rgb: float3(1,1,1),abs(_Alpha)/2 * (1-t)*(1-t));
                }
                else
                {
                    col = fixed4(_Color.rgb,0);
                }
                return col;
            }
            ENDCG
        }
    }
}

Shader "VFX/ChaseWarning"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LineMaxHeight01("Line Max Height 01", Float) = 0.3
        _LineMinHeight01("Line Min Height 01", Float) = 0
        _Density("Density", Float) = 1
        _FadeDis("Fade Distance", Float) = 0.1
        _Shift("Shift", Float) = 0
        _Color("Color", Color) = (1,1,1,1)
        _Speed("Speed", Float) = 1
        _BaseAlpha("Base Alpha", Float) = 0.3
        _BaseAlphaFade("Base Alpha Fade", Float) = 0.3
        // _PixelizeX("PixelizeX",Float) = 1024
        _PixelizeY("PixelizeY",Float) = 1024
        _MaxChasingNum("Max Chasing Num", Float) = 15
        _CarSpeedThredshold("Car Speed Thredshold", Float) = 5.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest [unity_GUIZTestMode]
        Cull Off
        ZWrite Off
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
            float _LineMaxHeight01;
            float _LineMinHeight01;
            float _FadeDis;
            float _Density;
            float _Shift;
            fixed4 _Color;
            float _Speed;
            float _BaseAlpha;
            float _BaseAlphaFade;
            float _MaxChasingNum;
            float _chase_enemy_num;
            float _car_speed_X;
            float _CarSpeedThredshold;
            // float _PixelizeX;
            float _PixelizeY;
            float WaveFunc01(float x01)
            {
                float a1 = 37.6 * _Density;
                float a2 = 85 * _Density;
                float a3 = 20 * _Density;
                float b2 = 50;
                float b3 = _Time.y * _Speed;
                return sin((sin(a1 * x01) + sin(a2 * x01 + b2) + sin(a3 * x01 + b3))+_Shift);
            }
            float Rand(float s)
            {
                return frac(sin(s) * 43758.5453123);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // uv remap
                float2 uv = i.uv;
                uv = float2(uv.x,floor(uv.y * _PixelizeY) / _PixelizeY);
                float height = WaveFunc01(uv.y) * _LineMaxHeight01;
                float dis =  max(height,_LineMinHeight01) - uv.x;
                float alpha = lerp(0,1,saturate((dis + _FadeDis)/_FadeDis));
                float t01 = saturate(_chase_enemy_num / _MaxChasingNum)/* * saturate(_car_speed_X/_CarSpeedThredshold) */;
                float baseAlpha = lerp(_BaseAlpha,0, saturate( (uv.x) / (_BaseAlphaFade * t01)));

                alpha = uv.x < 0 ? 0 : max(alpha,baseAlpha);


                fixed4 col = fixed4(_Color.rgb,_Color.a * alpha * t01);
                return col;
            }
            ENDCG
        }
    }
}

// WindSand.shader
Shader "Weather/SimFog"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white"{}
        _WindDir ("Wind Direction", Vector) = (1, 0, 0, 0) // 风向(xy)和强度(z)
        _NoiseScale ("Noise Scale", Float) = 10
        _Speed ("Speed", Float) = 0.5
        _WindDirMix ("Wind Direction Mix", Vector) = (1, 0, 0, 0) // 风向(xy)和强度(z)
        _NoiseScaleMix ("Noise Scale Mix", Float) = 10
        _SpeedMix("SpeedMix",Float) = 0.2
        _Density ("Base Density", Range(0, 1)) = 0.3
        _AlphaMul("AlphaMul",Float) = 1
        _BaseColor("BaseColor",Color) = (1,1,1,1)
        _SoftDownEdge("Soft Down Edge", Float) = 0
        _BubbleRadius("Bubble Radius",Float) = 1
        _BubbleRate("Bubble Rate",Float) = 1
        _BubbleSoft("Soft",Float) = 1
        _Bubble_Zero_Alpha ("zero alpha", Float) = 0.1
        // _PixelWidth("Pixel Width",Float) = 2200 // 经实际测试获得，一般情况下勿动
        // _PixelHeight("Pixel Height",Float) = 1050 // 经实际测试获得，一般情况下勿动
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent"
               "Queue"="Transparent"  
               "IgnoreProjector"="True"  
           }
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

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 pos_object : TEXCOORD1;
            };

            sampler2D _MainTex;
            float2 _WindDir;
            float2 _WindDirMix;
            float _NoiseScale;
            float _NoiseScaleMix;
            float _Speed;
            float _SpeedMix;
            float _Density;
            float _AlphaMul;
            float _SoftDownEdge;
            fixed4 _BaseColor;
            float _Arg1;
            float _BubbleRadius;
            float _BubbleRate;
            float _BubbleSoft;
            float _Bubble_Zero_Alpha;
            // float _PixelWidth;
            // float _PixelHeight;

            // Simplex 噪声函数
            float3 mod289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float2 mod289(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
            float3 permute(float3 x) { return mod289(((x*34.0)+1.0)*x); }

            float snoise(float2 v)
            {
                const float4 C = float4(0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439);
                float2 i = floor(v + dot(v, C.yy));
                float2 x0 = v - i + dot(i, C.xx);
                float2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                float4 x12 = x0.xyxy + C.xxzz;
                x12.xy -= i1;
                i = mod289(i);
                float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));
                float3 m = max(0.5 - float3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
                m = m*m;
                m = m*m;
                float3 x = 2.0 * frac(p * C.www) - 1.0;
                float3 h = abs(x) - 0.5;
                float3 ox = floor(x + 0.5);
                float3 a0 = x - ox;
                m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h*h);
                float3 g;
                g.x = a0.x * x0.x + h.x * x0.y;
                g.yz = a0.yz * x12.xz + h.yz * x12.yw;
                return 130.0 * dot(m, g);
            }

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.pos_object = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算UV为其最近的像素中心UV // 像素化效果十分不明显，完全没必要
                // float unitX = 1/_PixelWidth;
                // float unitY = 1/_PixelHeight;
                // float2 fixedUV = float2(floor(i.uv.x/unitX)*unitX + unitX/2,floor(i.uv.y/unitY)*unitY + unitY/2);
                float2 fixedUV = i.uv;
                // 基础风场运动（沿风向偏移）
                float2 windOffset = _WindDir * _Time.y * _Speed;
                float2 windOffsetMix = _WindDirMix * _Time.y * _SpeedMix;

                float2 movingUV = fixedUV - windOffset;
                float2 movingUVMix = fixedUV - windOffsetMix;

                // 叠加噪声模拟沙粒随机性
                float noise = snoise(movingUV * _NoiseScale) * 0.5 + 0.5;
                float noiseMix = snoise(movingUVMix * _NoiseScaleMix)* 0.5 + 0.5;

                // 混合基础浓度和噪声
                float sandDensity = saturate(_Density + (noise+noiseMix)/2 * 0.3);
                
                // 边缘衰减
                 //float edgeMask = 1 - pow((abs(fixedUV.y) - 0.5) * 2, 2);
               // sandDensity *= edgeMask;
                float softMulter = lerp(0,_SoftDownEdge,fixedUV.y);

                fixed4 col = fixed4(_BaseColor.rgb,sandDensity*_AlphaMul*softMulter * _Arg1);

                // 可视泡泡
                float i_dis = i.pos_object.x*i.pos_object.x*_BubbleRate*_BubbleRate+i.pos_object.y*i.pos_object.y;
                float bubble_radius_pow = _BubbleRadius*_BubbleRadius;
                float colMulRate = 1;
                float mi = bubble_radius_pow - i_dis;
                if(mi > 0)
                {
                    colMulRate = _BubbleSoft / (mi + _BubbleSoft) ;
                    if(colMulRate < _Bubble_Zero_Alpha) colMulRate = _Bubble_Zero_Alpha;
                }
                else
                {
                    colMulRate = 1;
                }
                col.a *= colMulRate;

                return col;
            }
            ENDCG
        }
    }
}
Shader "VFX/Frame Animation"
{
    Properties
    {
        _SpriteSheet ("Sprite Sheet", 2D) = "white" {}
        _Columns ("Columns", Int) = 4     // 横向帧数
        _Rows ("Rows", Int) = 4          // 纵向帧数
        _Speed ("Speed", Float) = 10      // 播放速度（帧/秒）
        [Toggle] _Loop ("Loop", Float) = 1 // 是否循环
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "DisableBatching"="True" // 避免合批导致UV错误
        }

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
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _SpriteSheet;
            float4 _SpriteSheet_ST;
            int _Columns;
            int _Rows;
            float _Speed;
            float _Loop;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _SpriteSheet);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算总帧数和当前帧索引
                int totalFrames = _Columns * _Rows;
                float frameTime = _Time.y * _Speed;
                int frameIndex = (int)frameTime % totalFrames;
                
                // 非循环模式则停在最后一帧
                if (_Loop < 0.5 && frameTime >= totalFrames) 
                    frameIndex = totalFrames - 1;

                // 计算当前帧的UV偏移
                int row = frameIndex / _Columns;
                int col = frameIndex % _Columns;
                float2 frameUV = float2( (i.uv.x / _Columns) + (float)col / _Columns,   (i.uv.y / _Rows) + 1.0 - (float)(row + 1) / _Rows   );// Sprite Sheet通常倒序排列
                   
                 
              

                return tex2D(_SpriteSheet, frameUV);
            }
            ENDCG
        }
    }
}
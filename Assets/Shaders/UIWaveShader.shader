Shader "UI/CustomMaskedWave"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Fill Control (0 = Empty, 1 = Full)
        _FillAmount ("Fill Amount", Range(0, 1)) = 1.0
        
        // Wobble / Slosh Parameters (Rocking left & right)
        _WobbleSpeed ("Wobble Speed", Float) = 4.0
        _WobbleAmount ("Wobble Intensity", Range(0, 0.5)) = 0.08
        
        // Surface Ripple Parameters (Small surface waves)
        _WaveSpeed ("Ripple Speed", Float) = 6.0
        _WaveFrequency ("Ripple Frequency", Float) = 12.0
        _WaveAmplitude ("Ripple Height", Float) = 0.02
        
        // Required UI Masking Properties
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float _FillAmount;
            float _WobbleSpeed;
            float _WobbleAmount;
            float _WaveSpeed;
            float _WaveFrequency;
            float _WaveAmplitude;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                
                // 1. Calculate side-to-side slosh / wobble (pivots around center X = 0.5)
                float wobbleAngle = sin(_Time.y * _WobbleSpeed) * _WobbleAmount;
                float wobbleOffset = (uv.x - 0.5) * wobbleAngle;
                
                // 2. Calculate small surface ripples
                float ripple = sin((uv.x * _WaveFrequency) + (_Time.y * _WaveSpeed)) * _WaveAmplitude;
                
                // 3. Cut off pixels above total fill height + wobble + ripple
                if (uv.y > _FillAmount + wobbleOffset + ripple)
                {
                    discard;
                }

                fixed4 c = tex2D(_MainTex, uv) * IN.color;
                return c;
            }
            ENDCG
        }
    }
}
Shader "UI/EyeBlink"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Closure ("Closure", Range(0,1)) = 0
        _Color ("Lid Color", Color) = (0,0,0,1)
        _Feather ("Edge Feather", Range(0.001,0.3)) = 0.06
        _VignetteStrength ("Vignette Strength", Range(0,0.5)) = 0.05
        _IrisShape ("Iris Width", Range(0.3,1.5)) = 1.0
        _IrisHeight ("Iris Height", Range(0.3,1.5)) = 1.0
        _EyeWidth ("Eye Width (fraction of screen)", Range(0.25,0.75)) = 0.5
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
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _Closure;
            fixed4 _Color;
            float _Feather;
            float _VignetteStrength;
            float _IrisShape;
            float _IrisHeight;
            float _EyeWidth;
            float4 _ClipRect;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            // Compute the eye-lid alpha for a single eye.
            // localX: horizontal position within that eye's space (-1..1)
            // centeredY: screen-vertical position (-1..1)
            // Returns 0 = fully visible, 1 = fully covered by lid
            float EyeLidAlpha(float localX, float centeredY)
            {
                float openingHeight = max(0.001, 1.0 - _Closure);
                float xAspect = localX / _IrisShape;
                float xSq = xAspect * xAspect;
                float eyeBound = openingHeight * sqrt(max(0.0, 1.0 - xSq));
                float dist = (abs(centeredY) / _IrisHeight) - eyeBound;
                return smoothstep(-_Feather, _Feather, dist);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centered = i.uv * 2.0 - 1.0;

                // Split screen into two halves. Each eye covers _EyeWidth of the screen.
                // Left eye:  UV.x from 0 to _EyeWidth  → centered.x from -1 to boundary
                // Right eye: UV.x from (1-_EyeWidth) to 1 → centered.x from boundary to 1
                float boundary = (1.0 - _EyeWidth) * 2.0 - 1.0;

                float localX;
                float lidAlpha;

                if (centered.x < boundary)
                {
                    // Left eye: remap [-1, boundary] → [-1, 1]
                    localX = (centered.x + 1.0) / (boundary + 1.0) * 2.0 - 1.0;
                    lidAlpha = EyeLidAlpha(localX, centered.y);
                }
                else
                {
                    // Right eye: remap [boundary, 1] → [-1, 1]
                    localX = (centered.x - boundary) / (1.0 - boundary) * 2.0 - 1.0;
                    lidAlpha = EyeLidAlpha(localX, centered.y);
                }

                // Very subtle vignette
                float vignette = smoothstep(0.75, 1.6, length(centered)) * _VignetteStrength;
                float vignetteAlpha = vignette * (1.0 - _Closure * 0.4);

                float finalAlpha = max(lidAlpha, vignetteAlpha) * _Color.a;
                return fixed4(_Color.rgb, finalAlpha * i.color.a);
            }
            ENDCG
        }
    }
}

Shader "Sprites/Rocket Core Additive"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _EdgeColor ("Edge Tint", Color) = (1,0.9960784,0.2627451,1)
        [PerRendererData] _EndFade ("End Fade (UV)", Float) = 0.1
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment CoreFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 _EdgeColor;
            float _EndFade;

            fixed4 CoreFrag(v2f IN) : SV_Target
            {
                fixed4 color = SampleSpriteTexture(IN.texcoord) * IN.color;

                // The full-texture sprite runs from 0 to 1 on each axis.
                float lateral = abs(IN.texcoord.x * 2.0 - 1.0);
                float sideFade = 1.0 - smoothstep(0.0, 1.0, lateral);
                float endDistance = min(IN.texcoord.y, 1.0 - IN.texcoord.y);
                float endFade = smoothstep(0.0, max(_EndFade, 0.0001), endDistance);

                float edgeTint = smoothstep(0.05, 0.9, lateral);
                color.rgb *= lerp(fixed3(1, 1, 1), _EdgeColor.rgb, edgeTint);
                color.a *= sideFade * endFade;
                // Premultiply once: Blend One One adds this feathered radiance.
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}

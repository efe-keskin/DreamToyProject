Shader "Sprites/Explosion Ray Additive"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [PerRendererData] _Aspect ("Ray Height / Width", Float) = 6
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
            #pragma fragment RayFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            float _Aspect;

            fixed4 RayFrag(v2f IN) : SV_Target
            {
                float aspect = max(_Aspect, 1.0);
                float2 capsulePosition = (IN.texcoord - 0.5) * float2(1.0, aspect);
                float halfSegment = (aspect - 1.0) * 0.5;
                float2 capOffset = float2(capsulePosition.x, max(abs(capsulePosition.y) - halfSegment, 0.0));
                float coverage = 1.0 - smoothstep(0.28, 0.5, length(capOffset));

                fixed4 color = IN.color;
                color.a *= SampleSpriteTexture(IN.texcoord).a * coverage;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}

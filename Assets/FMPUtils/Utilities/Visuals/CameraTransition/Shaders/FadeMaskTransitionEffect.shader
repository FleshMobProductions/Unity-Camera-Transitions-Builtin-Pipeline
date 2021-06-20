Shader "FMPUtils/CameraTransitions/FadeMaskTransitionEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OriginCamTex("Origin Camera Texture", 2D) = "black" {}
        [Tooltip(Greyscale Texture that defines the transition. The Color value determines when the target camera texture gets visible. 0 means at the start, 1 at the end)]
        _TransitionMaskTex("Transition Fade Mask", 2D) = "black" {}
        _TransitionProgress("Transition Progress", Range(0,1)) = 0.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off 
        ZWrite Off 
        ZTest Always

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
            };

            float _TransitionProgress;
            sampler2D _MainTex;
            sampler2D _OriginCamTex;
            sampler2D _TransitionMaskTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 colTowards = tex2D(_MainTex, i.uv);
                fixed4 colFrom = tex2D(_OriginCamTex, i.uv);
                float colTowardsThreshold = tex2D(_TransitionMaskTex, i.uv).r;
                return _TransitionProgress >= colTowardsThreshold ? colTowards : colFrom;
            }
            ENDCG
        }
    }
}

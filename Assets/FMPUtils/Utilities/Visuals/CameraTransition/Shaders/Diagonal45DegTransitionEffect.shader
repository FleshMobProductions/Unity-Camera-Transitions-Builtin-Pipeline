Shader "FMPUtils/CameraTransitions/Diagonal45DegTransitionEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OriginCamTex("Origin Camera Texture", 2D) = "black" {}
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pos = i.uv;             
                fixed4 colTowards = tex2D(_MainTex, i.uv);
                fixed4 colFrom = tex2D(_OriginCamTex, i.uv);
                float yToXAspectRatio = _ScreenParams.y / _ScreenParams.x;
                float mask = (yToXAspectRatio <= 1
                            ? (pos.x + (1.0 - pos.y) * yToXAspectRatio) * 0.5 + _TransitionProgress > 1.0 
                            : (pos.x / yToXAspectRatio + (1.0 - pos.y)) * 0.5 + _TransitionProgress > 1.0) 
                            ? 1.0 : 0.0;
                return lerp(colFrom, colTowards, mask);
            }
            ENDCG
        }
    }
}

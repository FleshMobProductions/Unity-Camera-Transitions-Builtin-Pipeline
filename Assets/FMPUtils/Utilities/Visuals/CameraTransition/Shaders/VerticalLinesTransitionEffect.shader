Shader "FMPUtils/CameraTransitions/VerticalLinesTransitionEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OriginCamTex("Origin Camera Texture", 2D) = "black" {}
        _TransitionProgress("Transition Progress", Range(0,1)) = 0.0
        _ColumnCount("Number of Columns", Range(1,40)) = 6
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
            int _ColumnCount;
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
                float progress = saturate(_TransitionProgress);

                float segment = floor(progress * _ColumnCount);
                int currentPixelSegment = floor(pos.x * _ColumnCount);
                if (currentPixelSegment < segment) {
                    return colTowards;
                }
                if (currentPixelSegment > segment) {
                    return colFrom;
                }
                float sectionStep = 1.0 / _ColumnCount;
                float sectionProgress = (progress % sectionStep) * _ColumnCount;
                // Have the vertical line appear so that the start position is where the previous line has ended, and from 
                // it's start position (top or bottom) moves in a way that reveals more of the fromCamera texture and less of the toCamera texture over time
                float progressMask = segment % 2.0 == 0.0 ? step(pos.y, sectionProgress) : step(1.0 - sectionProgress, pos.y);
                return lerp(colFrom, colTowards, progressMask);
            }
            ENDCG
        }
    }
}

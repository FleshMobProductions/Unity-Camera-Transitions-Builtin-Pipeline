Shader "FMPUtils/CameraTransitions/DiamondTransitionEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OriginCamTex("Origin Camera Texture", 2D) = "black" {}
        _TransitionProgress("Transition Progress", Range(0,1)) = 0.0
        // Below 0.04, the pattern won't finish completely when progress is 1
        // The higher the grid size goes,the faster the effect will finish as of now
        _GridSize("Grid Size", Range(0.04,0.08)) = 0.05
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
            #include "TransitionEffectUtilities.cginc"

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

            static const float unscaledEffectDuration = 4.0;
            static const float PI = 3.141592653589793238462;

            float _TransitionProgress;
            float _GridSize;
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
                fixed4 colCamera = tex2D(_MainTex, i.uv);
                fixed4 colTex = tex2D(_OriginCamTex, i.uv);
                float progress = saturate(_TransitionProgress + 0.2);
                float ut = progress * unscaledEffectDuration;

                float2 gridPos = pos % _GridSize;
                float2 gridID = floor(pos / _GridSize);

                // Getting the number of cells that fit onto the screen
                float maxXID = floor(1.0 / _GridSize);
                float maxYID = floor(1.0 / _GridSize);

                // Looping time in a triangle like way
                float breakPoint = unscaledEffectDuration;
	            float modX = ut % (breakPoint * 2.);
	            float pingPong = modX < breakPoint ? modX : -modX + breakPoint * 2.;
                float t = pingPong - 1.;
    
                float2 cellUV = gridPos / _GridSize;
                cellUV -= 0.5;
                cellUV *= 2.0;
                
                float2 rotatedUV = rotate(cellUV, PI * 0.25);
    
                if (modX < breakPoint)
    	            rotatedUV = rotate(rotatedUV, PI * 0.5);
    
                rotatedUV /= 1.41421356;  // sqrt(2.);
                rotatedUV = abs(rotatedUV);
    
                // Get cell distance by adding the x and y of the cell positions
                float dist = (maxXID - gridID.x) + gridID.y; 

                // Getting the current position in the animation
                float l = (t - dist * 0.05);
    
                float sizeX = pow(lerp(-0.01, 1.0, 1. - l), 3.);
                float sizeY = 1. - smoothstep(0.0, 1., l);
    
	            float alpha = 1. - pow(l * 1.612, 7.);
                
                // Scalings tested with a grid size between 0.04 and 0.08
                // A separate diagonal line is tried to be aligned with the end of the diamond pattern effect, 
                // a threshold that determines that ever everything below the line should have the colCamera 
                // (fully transitioned). This line should ideally move in sync with diagonal line formed where
                // the diamond pattern transition stops
                float scalingMul = 14.0 + (_GridSize - 0.03) / (0.04) * 13.2;
                float diagonalProgressScaling = 1.0 + _GridSize * scalingMul;
                float offsetMul = 11.0;
                float diagonalOffset = -_GridSize * offsetMul;
                // Get distance of the current pixel by adding the x and y of the pixel position. offset the value
                // and add a scaled progess to get a diagonal line that approximately follows the end of the effect. 
                bool isBelowDiagonalThreshold = diagonalOffset + ((pos.x + (1.0 - pos.y)) * 0.5) + _TransitionProgress * diagonalProgressScaling <= 1.0;
                float mask = ((sizeX * (1. - rotatedUV.x / sizeY) > rotatedUV.y) && isBelowDiagonalThreshold) ? 1. : 0.;
                
                return lerp(colCamera, colTex, mask);
            }
            ENDCG
        }
    }
}

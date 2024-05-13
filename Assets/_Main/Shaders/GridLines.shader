Shader "GridLines" {
   
   Properties {
      _GridThickness ("Grid Thickness", Float) = 0.02
      _WorldGridSpacing ("World Grid Spacing", Float) = 0.645
      _GridColor ("Grid Color", Color) = (0.5, 0.5, 1.0, 1.0)
      _OutsideColor ("Color Outside Grid", Color) = (0.0, 0.0, 0.0, 0.0)
      _FadeDistance ("Fade Distance", Float) = 5.0 // Distance at which the grid starts to fade
      [HideInInspector] _CursorHitPosition ("Cursor Intersection with Grid", Vector) = (0.0, 0.0, 0.0)
   }

   SubShader {
      Tags {
         "Queue" = "Transparent"
      }

      Pass {
         ZWrite Off
         Blend SrcAlpha OneMinusSrcAlpha

         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
 
         uniform float _GridThickness;
         uniform float _WorldGridSpacing; // New property for world space grid spacing
         uniform float4 _GridColor;
         uniform float4 _OutsideColor;
         uniform float _FadeDistance; // Distance at which the grid starts to fade
         uniform float4 _CursorHitPosition;

         struct vertexInput {
            float4 vertex : POSITION;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float2 gridPos : TEXCOORD0; // Use float2 to store grid position
            float distanceToMouse : TEXCOORD1; // Store distance to mouse cursor
         };
 
         vertexOutput vert(vertexInput input) {
            vertexOutput output; 
 
            output.pos =  UnityObjectToClipPos(input.vertex);
            output.gridPos = input.vertex.xz / _WorldGridSpacing; // Store XZ coordinates relative to world grid spacing
            
            // Calculate world position of the vertex
            float3 worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
            // Calculate distance to mouse cursor
            output.distanceToMouse = length(worldPos.xz - _CursorHitPosition.xz);
            
            return output;
         }
 
         float4 frag(vertexOutput input) : COLOR {
            float2 fracPos = frac(input.gridPos); // Calculate fractional part of grid position

            // Adjust thickness to ensure that (0,0) is a cell point
            float2 thickness = _GridThickness / _WorldGridSpacing;

            // Calculate alpha based on distance to mouse cursor
            float alpha = saturate(1.0 - input.distanceToMouse / _FadeDistance);

            // Apply fade effect
            float4 finalColor = lerp(_OutsideColor, _GridColor, alpha);

            // Check if the point is within the grid lines
            if (abs(fracPos.x - 0.5) < thickness.x || abs(fracPos.y - 0.5) < thickness.y) {
               return finalColor;
            } else {
               return _OutsideColor;
            }
         }
 
         ENDCG  
      }
   }
}

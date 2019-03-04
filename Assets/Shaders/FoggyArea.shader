Shader "Custom/FoggyArea"
{
    Properties
    {
		_FogColour("Fog Colour", Color) = (0,0,0,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_FogOpacity("Fog Opacity", range(0,1)) = 0.5
    }
    SubShader
    {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
		Pass
		{
			CGPROGRAM
			#pragma vertex vertex
			#pragma fragment fragment

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

			int _SourcesCount;
			int _VertexCount;
			fixed _FogOpacity;
			fixed2 _Polygon[2048];
			fixed4 _FogColour;
			fixed4 _MainTex_ST;
			sampler2D _MainTex;
			sampler2D _VisitedAreas;

			int rotate(int vertexIndex, int maxLimit)
			{
				return vertexIndex - _VertexCount * step(maxLimit, vertexIndex);
			}

			fixed calculateSlope(int firstVertexIndex, int maxLimit)
			{
				return (_Polygon[firstVertexIndex].y - _Polygon[rotate(firstVertexIndex + 1, maxLimit)].y) *
					rcp(_Polygon[firstVertexIndex].x - _Polygon[rotate(firstVertexIndex + 1, maxLimit)].x);
			}

			fixed calculateIntercept(int firstVertexIndex, fixed slope)
			{
				return mad(-slope, _Polygon[firstVertexIndex].x, _Polygon[firstVertexIndex].y);
			}

			//determines if a segment is upright or upside down
			//output mapping: 1 for upright or straight and -1 for upside down
			fixed calculateOrientation(int firstVertexIndex, int maxLimit)
			{
				return step(_Polygon[firstVertexIndex].x, 
					_Polygon[rotate(firstVertexIndex + 1, maxLimit)].x) * 2 - 1;
			}

			bool intersectsSegment(fixed2 pixel, fixed orientation, int firstVertexIndex, fixed slope, fixed intercept, int maxLimit)
			{
				return step(pixel.y, mad(slope, pixel.x, intercept)) *
					step(_Polygon[firstVertexIndex].x * orientation, pixel.x * orientation) *
					step(pixel.x * orientation, _Polygon[rotate(firstVertexIndex + 1, maxLimit)].x * orientation);
			}

			bool isOdd(int number)
			{
				return (number & (number + 1)) - number;
			}

			bool inPolygon(int polygonID, fixed2 pixel)
			{
				//the number of segments the ray intersects
				//the ray is coming from the pixel and going straight up
				int segmentsCount = 0;
				int polygonStart = polygonID * _VertexCount;
				int polygonEnd = polygonStart + _VertexCount;
				for (int i = polygonStart; i < polygonEnd; ++i)
				{
					fixed slope = calculateSlope(i, polygonEnd);
					fixed intercept = calculateIntercept(i, slope);
					fixed isUpright = calculateOrientation(i, polygonEnd);
					segmentsCount += intersectsSegment(pixel, isUpright, i, slope, intercept, polygonEnd);
				}
				return isOdd(segmentsCount);
			}

			v2f vertex(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 fragment(v2f input) : SV_Target
			{
				fixed4 white = fixed4(1,1,1,1);
				fixed4 pixelColour = _FogColour + tex2Dlod(_VisitedAreas, fixed4(input.uv, 0, 0)).r
					* white * _FogOpacity * step(3, _VertexCount);
				for (int i = 0; i < _SourcesCount; ++i)
					pixelColour += inPolygon(i, input.uv) * white * step(3, _VertexCount);
				return tex2D(_MainTex, input.uv) * saturate(pixelColour);
			}
			ENDCG
		}
    }
    FallBack "Diffuse"
}

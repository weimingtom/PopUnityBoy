﻿Shader "NewChromantics/PopUnityBoy/BackgroundShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		VRamTexture("VRamTexture", 2D ) = "black" {}
		IoRamTexture("IoRamTexture", 2D ) = "black" {}
		PaletteTexture("PaletteTexture", 2D ) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Gba.cginc"

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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;


	
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			bool IsBackgroundEnabled(int Layer)
			{
				//	dispcont bit 8+Layer
				return true;
			}
			bool IsObjectLayerEnabled()
			{
				return IsBackgroundEnabled(4);
			}

			#define BG0CNT	8
			#define BG1CNT	10
			#define BG2CNT	12
			#define BG2CNT	14
			#define BGXCNT(bg)	GetIoRam16( BG0CNT + (bg*2) )

			//	16 bit scroll pairs
			#define BG0HOFS	0x10
			#define BG0VOFS	0x12
			#define BGXHOFS(bg)	fmod( GetIoRam16( BG0HOFS + (bg*4) ), 511 )
			#define BGXVOFS(bg)	fmod( GetIoRam16( BG0VOFS + (bg*4) ), 511 )


			void GetBackgroundColour(int Background,int2 xy,inout float4 Colour)
			{
			#define RETURN_ERROR	{	Colour.xyz = float3(1,0,0);		return;	}
				if ( !IsBackgroundEnabled(Background) )
					return;

				//	z order, match with sprites!
				int BgContext = BGXCNT( Background );
				int Priority = And3( BgContext );

				if ( Background == 0 && BgContext != 196 )
					RETURN_ERROR;

				//int ResolutionMode = And3( RightShift( BgContext, 14 ) );
				int ResolutionMode = (BgContext >> 14) & 3;
				if ( Background == 0 && ResolutionMode != 0 )
					RETURN_ERROR;
				int2 Resolutions[4];
				Resolutions[0] = int2( 256, 256 );
				Resolutions[1] = int2( 512, 256 );
				Resolutions[2] = int2( 256, 512 );
				Resolutions[3] = int2( 512, 512 );
				int2 Resolution = Resolutions[ResolutionMode];
				int width = Resolution.x;
				int height = Resolution.y;

				int screenBase = And31( RightShift(BgContext,8) )  * 0x800;
				int charBase = And3( RightShift(BgContext,2) ) * 0x4000;
				int hofs = BGXHOFS(Background);
				int vofs = BGXVOFS(Background);
				hofs = 0;
				vofs = 0;

				bool TileColourCount256 = (BgContext & (1<<7)) != 0;
				//bool TileColourCount256 = And1( RightShift( BgContext, 7 ) ) != 0;
				if ( !TileColourCount256 )
					RETURN_ERROR;


                int bgy = ((xy.y + vofs) & (height - 1)) / 8;
				int tileIdx = screenBase + (((bgy & 31) * 32) * 2);
				switch ((BgContext >> 14) & 0x3)
                {
                    case 2: if (bgy >= 32) tileIdx += 32 * 32 * 2; break;
                    case 3: if (bgy >= 32) tileIdx += 32 * 32 * 4; break;
                }

                int tileY = ((xy.y + vofs) & 0x7) * 8;

                int i = xy.x;
                int bgx = ((i + hofs) & (width - 1)) / 8;
                int tmpTileIdx = tileIdx + ((bgx & 31) * 2);
                if (bgx >= 32) 
                	tmpTileIdx += 32 * 32 * 2;
                int tileChar = GetVRam16( tmpTileIdx );
                int x = (i + hofs) & 7;
                int y = tileY;
                if ((tileChar & (1 << 10)) != 0) 
                	x = 7 - x;
                if ((tileChar & (1 << 11)) != 0) 
               		y = 56 - y;

                int TileVRamIndex = charBase + ((tileChar & 0x3FF) * 64) + y + x;
                int PaletteIndex = GetVRam8( TileVRamIndex );
                if ( PaletteIndex != 0 )
                {
                	Colour.xyz = GetPalette15Colour( PaletteIndex );
                }
              

			}

			fixed4 frag (v2f i) : SV_Target
			{
				//	clear colour
				float4 Colour = GetBgColour();
				float2 ScreenSize = float2( 240, 160 );
				int2 xy = i.uv * ScreenSize;

				GetBackgroundColour( 0, xy, Colour );
				GetBackgroundColour( 1, xy, Colour );
				GetBackgroundColour( 2, xy, Colour );
				GetBackgroundColour( 3, xy, Colour );

				return Colour;
			}
			ENDCG
		}
	}
}

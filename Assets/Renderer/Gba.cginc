﻿#if !defined(VRamTexture)
sampler2D VRamTexture;
float4 VRamTexture_TexelSize;
#endif

#if !defined(IoRamTexture)
sampler2D IoRamTexture;
float4 IoRamTexture_TexelSize;
#endif

#if !defined(PaletteTexture)
sampler2D PaletteTexture;
float4 PaletteTexture_TexelSize;
#endif


/*	gr: if these are not in the parent shader then all zero..
int PaletteRedStart = 0;
int PaletteGreenStart = 5;
int PaletteBlueStart = 10;
int PaletteEndianSwap = 0;
*/
#define PaletteRedStart			0
#define PaletteGreenStart		5
#define PaletteBlueStart		10
#define PaletteEndianSwap		0

int RightShift(int Value,int ShiftAmount)	
{
	return Value >> ShiftAmount;
	//return floor(f / pow(2,ShiftAmount) );	
}

int LeftShift(int Value,int ShiftAmount)	
{
	return Value << ShiftAmount;
	//return f * pow(2,ShiftAmount);	
}

int And3(int Value)
{
	return (Value & 3);
	//Value = fmod( Value, 3 );
	//return Value;
}

int And1(int Value)
{
	return (Value & 1);
	//Value = fmod( Value, 1 );
	//return Value;
}


int And31(int Value)
{
	return (Value & 31);
	//Value = fmod( Value, 3 );
	//return Value;
}


//	adapted from https://stackoverflow.com/a/27551433
float4 UnpackColor15(int f) 
{
/*
	float a = RightShift( f, 15 );
	float r = RightShift( f - LeftShift(a,15), 10 );
	float g = RightShift( f - LeftShift(r,10), 5 );
	float b = floor( f - LeftShift(r,10) - LeftShift(g,5) );
*/
	int Colour16 = f;
	/*
	int a = (Colour16 >> 15) & 1;
	int r = (Colour16 >> 10) & 31;
	int g = (Colour16 >> 5) & 31;
	int b = (Colour16 >> 0) & 31;
	*/
	int a = 1;
	int r = (Colour16 >> PaletteRedStart) & 31;
	int g = (Colour16 >> PaletteGreenStart) & 31;
	int b = (Colour16 >> PaletteBlueStart) & 31;

	//	normalize
	float Max5 = 31;
	float4 rgba = float4( r,g,b,a ) / float4( Max5, Max5, Max5, 1 );
	rgba = clamp( 0, 1, rgba );

	//	gr: alpha is always 0?
	//a = 1;


	return rgba;
}

float4 GetPalette15Colour(int Index)
{
	int SourceWidth = PaletteTexture_TexelSize.z;
	int SourceHeight = PaletteTexture_TexelSize.w;
	int Samplex = Index % SourceWidth;
	int Sampley = Index / SourceWidth;
	float2 Sampleuv = float2(Samplex,Sampley) / float2(SourceWidth,SourceHeight);

	//	scale float data to 16 bit
	//	gr: this doesnt work - maybe too big for floats? colours seem to bleed
	/*
	int2 Scalar = ( PaletteEndianSwap ) ? int2( 256, 256*256 ) : int2( 256*256, 256 );
	int2 PaletteColour8_8 = tex2D(PaletteTexture, Sampleuv).xy * Scalar;
	int PaletteColour16 = PaletteColour8_8.x + PaletteColour8_8.y;
	*/
	//	gr: works!
	int2 PaletteColour8_8 = tex2D(PaletteTexture, Sampleuv).xy * 256;
	int2 Scalar = ( PaletteEndianSwap ) ? int2( 256, 1 ) : int2( 1, 256 );
	int PaletteColour16 = (PaletteColour8_8.x*Scalar.x) + (PaletteColour8_8.y*Scalar.y);

	float4 rgba = UnpackColor15( PaletteColour16 );
	return rgba;
}

int GetVRam8(int Index)
{
	float w = VRamTexture_TexelSize.z;
	float h = VRamTexture_TexelSize.w;
	float u = (Index % w) / w;
	float v = (Index / w) / h;

	int Value = tex2D( VRamTexture, float2(u,v) ) * 256;

	return Value;
}

int GetVRam16(int Index)
{
	int x = GetVRam8( Index ) ;
	x += GetVRam8( Index+1 ) * 256;
	return x;
}


int GetIoRam8(int Index)
{
	float w = IoRamTexture_TexelSize.z;
	float h = IoRamTexture_TexelSize.w;
	float u = (Index % w) / w;
	float v = (Index / w) / h;

	int Value = tex2D( IoRamTexture, float2(u,v) ).x * 256.0f;

	return Value;
}

int GetIoRam16(int Index)
{
	int a = GetIoRam8( Index+0 );
	int b = GetIoRam8( Index+1 ) * 256;
	return a+b;
}

//	mode 1 stuff
float4 GetBgColour()
{
	return GetPalette15Colour( 0 );
}


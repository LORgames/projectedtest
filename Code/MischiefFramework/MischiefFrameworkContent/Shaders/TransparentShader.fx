//--------------------------------------------------------------------------------------
// Shared Variables
//--------------------------------------------------------------------------------------

float ALPHA_CUTOFF = 1.00f;

float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldViewProjection;
float4x4 WorldInverseTranspose;

//The texture that is rendered on the object(s)
texture Texture;

//The depth mask
texture DepthTexture;
texture PreRenderedScene;

// material components
float4 MaterialDiffuseColor = float4(0.6f, 0.6f, 0.6f, 1.0f);
float4 MaterialAmbientColor = float4(0.9f, 0.9f, 0.9f, 1.0f);
float4 MaterialSpecularColor = float4(0.0f, 0.0f, 0.0f, 1.0f);
float  MaterialSpecularPower = 2.0f;

float MaterialAlpha = 0.5f;

// camera components
shared float3 CameraPosition;

// Other settings?
bool   TextureEnabled = true;

//--------------------------------------------------------------------------------------
// Shader Input/Output structures
//--------------------------------------------------------------------------------------

// Vertex shader input structure.
struct VSInput {
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

// Vertex shader output structure
struct VSOutput {
	float4 Position: POSITION0;
	float2 TexCoord: TEXCOORD0;
	float3 Normal:	 TEXCOORD1;
	float2 Depth:	 TEXCOORD2;

	float4 ScreenPos: TEXCOORD3;
};

struct PSOutput {
    half4 Color : COLOR0;
};

//-----------------------------------------------------------------------------------------
// Textures and Samplers
//-----------------------------------------------------------------------------------------

// anisotropic filtering
sampler2D DiffuseMap = sampler_state {
	Texture = (Texture);
	MinFilter = ANISOTROPIC; //ANISOTROPIC
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
	MaxAnisotropy = 4;
};

sampler2D DepthMap = sampler_state {
	Texture = (DepthTexture);
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = POINT;
	AddressU = WRAP;
	AddressV = WRAP;
};

sampler2D SceneMap = sampler_state {
	Texture = (PreRenderedScene);
	MinFilter = POINT;
	MagFilter = POINT;
	MipFilter = POINT;
	AddressU = WRAP;
	AddressV = WRAP;
};

//--------------------------------------------------------------------------------------
// Simple shaders
//--------------------------------------------------------------------------------------

VSOutput VS_GBuffer(VSInput Input) {
	VSOutput Output;

	Output.TexCoord = Input.TexCoord;

	Output.Position = mul(Input.Position, WorldViewProjection);
	Output.ScreenPos = Output.Position;

	float3 normal = mul(float4(Input.Normal, 0.0f), WorldInverseTranspose).xyz;
	Output.Normal = normalize(normal);

	Output.Depth.x = Output.Position.z;
	Output.Depth.y = Output.Position.w;

	return Output;
}

PSOutput PS_GBuffer(VSOutput Input) {
	PSOutput Output;

	float2 spos = Input.ScreenPos.xy / Input.ScreenPos.w;
	spos.xy = 0.5*(spos.xy+1.0);
	spos.y = -spos.y;

	if(Input.Depth.x / Input.Depth.y > tex2D(DepthMap, spos).r) {
		clip(-1);
	}

	// DIFFUSE
	if(TextureEnabled) {
		Output.Color = tex2D(DiffuseMap, Input.TexCoord);
	} else {
		Output.Color = MaterialDiffuseColor;
	}

	// RETURN
    return Output;
}

//--------------------------------------------------------------------------------------
// Simple techniques
//--------------------------------------------------------------------------------------

technique GBuffer {
	pass P0 {
		vertexShader = compile vs_2_0 VS_GBuffer();
		pixelShader = compile ps_2_0 PS_GBuffer();
	}
}
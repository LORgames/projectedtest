//--------------------------------------------------------------------------------------
// Shared Variables
//--------------------------------------------------------------------------------------

float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldViewProjection;
float4x4 WorldInverseTranspose;
float4x4 CameraViewProjection;

//The texture that is rendered on the object(s)
texture Texture;
texture TextureNormalMap;
texture TextureShadowMap;

// material components
float4 MaterialDiffuseColor = float4(0.6f, 0.6f, 0.6f, 1.0f);
float4 MaterialAmbientColor = float4(0.9f, 0.9f, 0.9f, 1.0f);
float4 MaterialSpecularColor = float4(0.0f, 0.0f, 0.0f, 1.0f);
float  MaterialSpecularPower = 2.0f;

// camera components
shared float3 CameraPosition;

// Other settings?
float  Time;
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
	float3 Normal:	 TEXCOORD0;
	float2 Depth:	 TEXCOORD1;
	float4 ProjTex:  TEXCOORD2;
	float4 worldPos: TEXCOORD3;
};

struct PSOutput {
    half4 Color : COLOR0;
    half4 Normal : COLOR1;
    half4 Depth : COLOR2;
};

// vertex output structure used by ShadowMap technique
struct ShadowOutput {
	float4 Position: POSITION0;
	float2 Depth: TEXCOORD0;
};

//-----------------------------------------------------------------------------------------
// Textures and Samplers
//-----------------------------------------------------------------------------------------

// anisotropic filtering
sampler2D DiffuseMap = sampler_state {
	Texture = (Texture);
	MinFilter = ANISOTROPIC; //ANISOTROPIC
	MagFilter = ANISOTROPIC;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
	MaxAnisotropy = 4;
};

// anisotropic filtering
sampler2D NormalMap = sampler_state {
	Texture = <TextureNormalMap>;
	MinFilter = LINEAR; //ANISOTROPIC
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
	MaxAnisotropy = 4;
};

//--------------------------------------------------------------------------------------
// Simple shaders
//--------------------------------------------------------------------------------------

VSOutput VS_GBuffer(VSInput Input) {
	VSOutput Output;

	Output.Position = mul(Input.Position, WorldViewProjection);
	Output.worldPos = mul(Input.Position, World);

	float3 normal = mul(float4(Input.Normal, 0.0f), WorldInverseTranspose).xyz;
	Output.Normal = normalize(normal);

	Output.ProjTex = mul(Input.Position, CameraViewProjection);

	Output.Depth.x = Output.Position.z;
	Output.Depth.y = Output.Position.w;

	return Output;
}

PSOutput PS_GBuffer(VSOutput Input) {
	PSOutput Output;

	// DIFFUSE
	Input.ProjTex.xy /= Input.ProjTex.w;
	Input.ProjTex.x =  0.5f*Input.ProjTex.x + 0.5f;
	Input.ProjTex.y = -0.5f*Input.ProjTex.y + 0.5f;
	Output.Color = tex2D(DiffuseMap, Input.ProjTex.xy);
    
	// SPECULAR INTENSITY
    Output.Color.a = (MaterialSpecularColor.r+MaterialSpecularColor.g+MaterialSpecularColor.b)/3.0f;

	// NORMAL
    Output.Normal.rgb = 0.5f * (Input.Normal + 1.0f);

    // SPECULAR POWER
    Output.Normal.a = MaterialSpecularPower;

	// DEPTH
    Output.Depth = Input.Depth.x / Input.Depth.y;

	// RETURN
    return Output;
}

ShadowOutput GenerateShadowMapVS(VSInput Input) {
	ShadowOutput Output;

	Output.Position = mul(Input.Position, WorldViewProjection);
	Output.Depth.x = Output.Position.z;
	Output.Depth.y = Output.Position.w;

	return Output;
}

float4 GenerateShadowMapPS(ShadowOutput Input) : COLOR {
	// output the depth of the pixel from the light source, normalized into the view space
	return float4(Input.Depth.x / Input.Depth.y, 0, 0, 0);
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

// technique used to generate the shadow map
technique GenerateShadowMapTech {
	pass P0 {
		vertexShader = compile vs_2_0 GenerateShadowMapVS();
        pixelShader  = compile ps_2_0 GenerateShadowMapPS();
	}
}
//-----------------------------------------------------------------------------
// SkinnedEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Shared Variables
//--------------------------------------------------------------------------------------
#define SKINNED_EFFECT_MAX_BONES   72

//The texture that is rendered on the object(s)
texture Texture;

float4 MaterialDiffuseColor = float4(0.8f, 0.5f, 0.4f, 1.0f);
float4 MaterialAmbientColor = float4(0.9f, 0.2f, 0.0f, 1.0f);
float4 MaterialSpecularColor = float4(0.2f, 0.2f, 0.2f, 1.0f);
float  MaterialSpecularPower = 8.0f;

float3 EyePosition;

float4x3 Bones[SKINNED_EFFECT_MAX_BONES];

float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldViewProjection;
float4x4 WorldInverseTranspose;

int ShaderIndex = 1;

//--------------------------------------------------------------------------------------
// Shader Input/Output structures
//--------------------------------------------------------------------------------------
struct VSInput {
    float4 Position:SV_Position;
    float3 Normal:	NORMAL;
    float2 TexCoord:TEXCOORD0;
    int4   Indices:	BLENDINDICES0;
    float4 Weights:	BLENDWEIGHT0;
};

struct VSOutput {
	float4 Position:POSITION0;
	float2 TexCoord:TEXCOORD0;
	float3 Normal:	TEXCOORD1;
	float2 Depth:	TEXCOORD2;
};

struct PSOutput {
    half4 Color : COLOR0;
    half4 Normal: COLOR1;
    half4 Depth : COLOR2;
};

// vertex output structure used by ShadowMap technique
struct ShadowOutput {
	float4 Position: POSITION0;
	float2 Depth: TEXCOORD0;
};

//--------------------------------------------------------------------------------------
// Textures and Samplers
//--------------------------------------------------------------------------------------
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

//--------------------------------------------------------------------------------------
// Some helper functions
//--------------------------------------------------------------------------------------

void Skin(inout VSInput vin, uniform int boneCount) {
    float4x3 skinning = 0;

    [unroll]
    for (int i = 0; i < boneCount; i++) {
        skinning += Bones[vin.Indices[i]] * vin.Weights[i];
    }

    vin.Position.xyz = mul(vin.Position, skinning);
    vin.Normal = mul(vin.Normal, (float3x3)skinning);
}

int BoneCounts[3] = { 1, 2, 4 };

//--------------------------------------------------------------------------------------
// The shaders themselves
//--------------------------------------------------------------------------------------

VSOutput VS_Skinned(VSInput Input) {
    VSOutput Output;
    
    Skin(Input, BoneCounts[ShaderIndex]);

    Output.TexCoord = Input.TexCoord;

	Output.Position = mul(Input.Position, WorldViewProjection);

	float3 normal = mul(float4(Input.Normal, 0.0f), WorldInverseTranspose).xyz;
	Output.Normal = normalize(normal);

	Output.Depth.x = Output.Position.z;
	Output.Depth.y = Output.Position.w;

    return Output;
}

// Pixel shader: pixel lighting.
PSOutput PS_GBuffer(VSOutput Input) : SV_Target0 {
    PSOutput Output;

	// DIFFUSE
    Output.Color = tex2D(DiffuseMap, Input.TexCoord);
	
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

	Skin(Input, BoneCounts[ShaderIndex]);

	Output.Position = mul(Input.Position, WorldViewProjection);

	Output.Depth.x = Output.Position.z;
	Output.Depth.y = Output.Position.w;

	return Output;
}

float4 GenerateShadowMapPS(ShadowOutput Input) : COLOR {
	// output the depth of the pixel from the light source, normalized into the view space
	return float4(Input.Depth.x / Input.Depth.y, 0, 0, 0);
}

Technique SkinnedEffect {
    Pass {
        VertexShader = compile vs_2_0 VS_Skinned();
        PixelShader  = compile ps_2_0 PS_GBuffer();
    }
}

// technique used to generate the shadow map
technique GenerateShadowMapTech {
	pass P0 {
		vertexShader = compile vs_2_0 GenerateShadowMapVS();
        pixelShader  = compile ps_2_0 GenerateShadowMapPS();
	}
}


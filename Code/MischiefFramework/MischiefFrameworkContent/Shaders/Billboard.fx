//-----------------------------------------------------------------------------
// Billboard.fx
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Camera parameters.
float4x4 View;
float4x4 Projection;

// Parameters controlling the wind effect.
float3 WindDirection = float3(1, 0, 0);
float WindWaveSize = 0.1;
float WindRandomness = 1;
float WindSpeed = 4;
float WindAmount;
float WindTime;

// 1 means we should only accept opaque pixels.
// -1 means only accept transparent pixels.
float AlphaTestDirection = 1;
float AlphaTestThreshold = 0.95;

// Parameters describing the billboard itself.
float BillboardWidth;
float BillboardHeight;

texture Texture;

sampler TextureSampler = sampler_state {
    Texture = (Texture);
};

struct VSInput {
    float3 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float Random : TEXCOORD1;
};

struct VSOutput {
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal :   TEXCOORD1;
	float2 Depth :	  TEXCOORD2;
};

struct PSOutput {
    half4 Color : COLOR0;
    half4 Normal : COLOR1;
    half4 Depth : COLOR2;
};

VSOutput VSGrass(VSInput Input) {
    VSOutput Output;

    // Apply a scaling factor to make some of the billboards
    // shorter and fatter while others are taller and thinner.
    float squishFactor = 0.75 + abs(Input.Random) / 2;

    float width = BillboardWidth * squishFactor;
    float height = BillboardHeight / squishFactor;

    // Flip half of the billboards from left to right. This gives visual variety
    // even though we are actually just repeating the same texture over and over.
    if (Input.Random < 0)
        width = -width;

    // Work out what direction we are viewing the billboard from.
    float3 viewDirection = View._m02_m12_m22;

    float3 rightVector = normalize(cross(viewDirection, Input.Normal));

    // Calculate the position of this billboard vertex.
    float3 position = Input.Position;

    // Offset to the left or right.
    position += rightVector * (Input.TexCoord.x - 0.5) * width;
    
    // Offset upward if we are one of the top two vertices.
    position += Input.Normal * (1 - Input.TexCoord.y) * height;

    // Work out how this vertex should be affected by the wind effect.
    float waveOffset = dot(position, WindDirection) * WindWaveSize;
    
    waveOffset += Input.Random * WindRandomness;
    
    // Wind makes things wave back and forth in a sine wave pattern.
    float wind = sin(WindTime * WindSpeed + waveOffset) * WindAmount;
    
    // But it should only affect the top two vertices of the billboard!
    wind *= (1 - Input.TexCoord.y);
    
    position += WindDirection * wind;

    // Apply the camera transform.
    float4 viewPosition = mul(float4(position, 1), View);

    Output.Position = mul(viewPosition, Projection);

    Output.TexCoord = Input.TexCoord;
    
    // Compute lighting.
	Output.Normal = Input.Normal;

	//Depth
	Output.Depth.x = Output.Position.z;
	Output.Depth.y = Output.Position.w;

    return Output;
}

PSOutput PSGrass(VSOutput Input) {
	PSOutput Output;

	//Standard Effect things
    Output.Color = tex2D(TextureSampler, Input.TexCoord);
	Output.Normal.rgb = 0.5f * (Input.Normal + 1.0f);
	Output.Depth = Input.Depth.x / Input.Depth.y;

	//Alpha Test
	clip((Output.Color.a - AlphaTestThreshold) * AlphaTestDirection);

	//Specular
	Output.Color.a = 0.0f;
	Output.Normal.a = 0;

    return Output;
}

technique GBuffer {
    pass {
        VertexShader = compile vs_2_0 VSGrass();
        PixelShader = compile ps_2_0 PSGrass();
    }
}

// The current scene
texture DiffuseTexture;

sampler DiffuseSampler = sampler_state {
    Texture = (DiffuseTexture);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

// The other textures
texture LightMap;

sampler LightSampler = sampler_state {
    Texture = (LightMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

float4 PSFinalOutput(float2 TextureCoordinate : TEXCOORD0) : COLOR0 {
    float3 diffuseColor = tex2D(DiffuseSampler, TextureCoordinate).rgb;
    float4 light = tex2D(LightSampler, TextureCoordinate);

    float3 diffuseLight = light.rgb;
    float specularLight = light.a;

	//return float4(tex2D(DiffuseSampler, TextureCoordinate).rgb, 1);
	return float4 ((diffuseColor * diffuseLight * 0.7f + specularLight + diffuseColor * 0.3f), 1);
}

technique FinalOutput {
    pass P0 {
        PixelShader = compile ps_2_0 PSFinalOutput();
    }
}
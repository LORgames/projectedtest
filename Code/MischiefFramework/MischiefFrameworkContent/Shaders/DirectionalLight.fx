//direction of the light
float3 lightDirection;

//color of the light 
float3 Color; 

//position of the camera, for specular light
float3 cameraPosition; 

//this is used to compute the world-position
float4x4 InvertViewProjection; 

// diffuse color, and specularIntensity in the alpha channel
texture colorMap; 

// normals, and specularPower in the alpha channel
texture normalMap;

//depth
texture depthMap;

//the prerendered depth map from the lights point of view
texture shadowMap;
float4x4 lightViewProjection;
static const float SMAP_SIZE = 1024.0f;
static const float SHADOW_EPSILON = 0.00033f;

sampler colorSampler = sampler_state {
    Texture = (colorMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;
};

sampler depthSampler = sampler_state {
    Texture = (depthMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler normalSampler = sampler_state {
    Texture = (normalMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

sampler shadowSampler = sampler_state {
    Texture = (shadowMap);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = POINT;
    MinFilter = POINT;
    Mipfilter = POINT;
};

struct VertexShaderInput {
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput {
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

float2 halfPixel;

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {
    VertexShaderOutput output;
    output.Position = float4(input.Position,1);
    //align texture coordinates
    output.TexCoord = input.TexCoord - halfPixel;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input, uniform bool useShadowMap) : COLOR0 {
    //get normal data from the normalMap
    float4 normalData = tex2D(normalSampler,input.TexCoord);
    
	//transform normal back into [-1,1] range
    float3 normal = 2.0f * normalData.xyz - 1.0f;
    
	//get specular power, and get it into [0,255] range]
    float specularPower = normalData.a * 255;
    
	//get specular intensity from the colorMap
    float specularIntensity = tex2D(colorSampler, input.TexCoord).a;
    
    //read depth
    float depthVal = tex2D(depthSampler,input.TexCoord).r;

    //compute screen-space position
    float4 position;
    position.x =  input.TexCoord.x * 2.0f - 1.0f;
    position.y =-(input.TexCoord.y * 2.0f - 1.0f);
    position.z = depthVal;
    position.w = 1.0f;
    
	//transform to world space
    position = mul(position, InvertViewProjection);
    position /= position.w;
    
    //surface-to-light vector
    float3 lightVector = -normalize(lightDirection);

    //compute diffuse light
    float NdL = max(0, dot(normal,lightVector));
    float3 diffuseLight = NdL * Color.rgb;

    //reflexion vector
    float3 reflectionVector = normalize(reflect(-lightVector, normal));
    
	//camera-to-surface vector
    float3 directionToCamera = normalize(cameraPosition - position);
    
	//compute specular light
    float specularLight = specularIntensity * pow( saturate(dot(reflectionVector, directionToCamera)), specularPower);

	float shadowCoeff = 1.0f;

	if(useShadowMap) {
		float4 positionInLightSpace = mul(position, lightViewProjection);
		positionInLightSpace.xy /= positionInLightSpace.w;
		positionInLightSpace.x = 0.5f * positionInLightSpace.x + 0.5f;
		positionInLightSpace.y = -0.5f * positionInLightSpace.y + 0.5f;
		
		float lightDepth = positionInLightSpace.z / positionInLightSpace.w;

		float2 texelpos = SMAP_SIZE * positionInLightSpace.xy;
		float2 lerps = frac(texelpos);

		float dx = 1.0f / SMAP_SIZE;

		float s0 = tex2D(shadowSampler, positionInLightSpace.xy).r + SHADOW_EPSILON < lightDepth ? 0.0f : 1.0f;
		float s1 = tex2D(shadowSampler, positionInLightSpace.xy + float2(dx, 0.0f)).r + SHADOW_EPSILON < lightDepth ? 0.0f : 1.0f;
		float s2 = tex2D(shadowSampler, positionInLightSpace.xy + float2(0.0f, dx)).r + SHADOW_EPSILON < lightDepth ? 0.0f : 1.0f;
		float s3 = tex2D(shadowSampler, positionInLightSpace.xy + float2(dx, dx)).r + SHADOW_EPSILON < lightDepth ? 0.0f : 1.0f;

		shadowCoeff = lerp(lerp(s0, s1, lerps.x), lerp(s2, s3, lerps.x), lerps.y);
	}

    //output the two lights
    return float4(diffuseLight.rgb, specularLight)*shadowCoeff;
}

technique Technique0 {
    pass Pass0 {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader  = compile ps_2_0 PixelShaderFunction(false);
    }
}

technique Technique1 {
    pass Pass0 {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader  = compile ps_2_0 PixelShaderFunction(true);
    }
}
//----------------------------------------------------
//--                                                --
//--             www.riemers.net                 --
//--         Series 4: Advanced terrain             --
//--                 Shader code                    --
//--                                                --
//----------------------------------------------------

//------- Constants --------
float4x4 xView;

float4x4 xReflectionView;



float4x4 xProjection;
float4x4 xWorld;
bool xEnableLighting;

Texture xWaterBumpMap;

float xWaveLength;
float xWaveHeight;

float3 xCamPos;

float xTime;
float xWindForce;
float3 xWindDirection;
float xTimeOfDay;

float xFogStart = 100;
float xFogEnd = 200;
float3 xFogColor = float3(0.5f, 0.5f, 0.5f);
float3 xLightPos = float3(0, 0, 0);
float4 xLightColor = float4(0, 0, 0, 0);

//------- Technique: Clipping Plane Fix --------

bool Clipping;
bool SelfIllumination;
float4 ClipPlane0;


//------- Technique: Clipping Plane Fix --------


//------- Texture Samplers --------
Texture xTexture;
Texture xIllumination;

sampler IllumSampler = sampler_state { texture = <xIllumination> ;  magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};

sampler TextureSampler = sampler_state { texture = <xTexture> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};Texture xTexture0;

sampler TextureSampler0 = sampler_state { texture = <xTexture0> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = wrap; AddressV = wrap;};Texture xTexture1;

sampler TextureSampler1 = sampler_state { texture = <xTexture1> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = clamp; AddressV = clamp;};Texture xTexture2;

sampler TextureSampler2 = sampler_state { texture = <xTexture2> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = clamp; AddressV = clamp;};Texture xTexture3;

sampler TextureSampler3 = sampler_state { texture = <xTexture3> ; magfilter = POINT; minfilter = POINT; mipfilter=POINT; AddressU = clamp; AddressV = clamp;};
Texture xReflectionMap;

Texture xSunGradient;
Texture xAmbientGradient;
Texture xTorchGradient;
Texture xRefractionMap;
float4 xTint;
float3 xLightDirection = float3(0, 1, 0);


sampler SunSampler = sampler_state { texture = <xSunGradient> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp;}; 

sampler AmbientSampler = sampler_state { texture = <xAmbientGradient> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=POINT; AddressU = clamp; AddressV = clamp;};

sampler TorchSampler = sampler_state { texture = <xTorchGradient> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp;};

sampler ReflectionSampler = sampler_state { texture = <xReflectionMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp;};

sampler RefractionSampler = sampler_state { texture = <xRefractionMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

sampler WaterBumpMapSampler = sampler_state { texture = <xWaterBumpMap> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = wrap; AddressV = wrap;};

///////////// Technique untextured
	struct UTVertexToPixel
	{
	float4 Position     : POSITION;
	float4 Color        : COLOR0;
	float4 clipDistances     : TEXCOORD5;
	};

	struct UTPixelToFrame
	{
	float4 Color : COLOR0;
	};

    UTVertexToPixel UTexturedVS( float4 inPos : POSITION,  float4 inColor : COLOR0)
	{
		UTVertexToPixel Output = (UTVertexToPixel)0;
		float4x4 preViewProjection = mul (xView, xProjection);
		float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);

		Output.Position = mul(inPos, preWorldViewProjection);
		Output.Color = inColor;


		if(Clipping)
		Output.clipDistances = dot(mul(xWorld,inPos), ClipPlane0); //MSS - Water Refactor added

		return Output;
	}



	UTPixelToFrame UTexturedPS(UTVertexToPixel PSIn)
	{
		UTPixelToFrame Output = (UTPixelToFrame)0;

		Output.Color = PSIn.Color;

	    clip(PSIn.clipDistances);  //MSS - Water Refactor added


		return Output;
	}

	technique Untextured_2_0
	{
		pass Pass0
		{
			VertexShader = compile vs_2_0 UTexturedVS();
			PixelShader = compile ps_2_0 UTexturedPS();
		}
	}

	technique Untextured
	{
		pass Pass0
		{   
			VertexShader = compile vs_2_0 UTexturedVS();
			PixelShader  = compile ps_2_0 UTexturedPS();
		}
	}




///////////////


//------- Technique: Textured --------
struct TVertexToPixel
{
float4 Position     : POSITION;
float4 Color        : COLOR0;
float LightingFactor: TEXCOORD0;
float2 TextureCoords: TEXCOORD1;
float4 clipDistances     : TEXCOORD5;
float Fog : TEXCOORD7;	
float4 TextureBounds: TEXCOORD6;
float3 Position3D    : TEXCOORD2;

};

struct TPixelToFrame
{
float4 Color : COLOR0;
};

TVertexToPixel TexturedVS( float4 inPos : POSITION,  float2 inTexCoords: TEXCOORD0, float4 inColor : COLOR0, float4 inTexSource : TEXCOORD1)
{
    TVertexToPixel Output = (TVertexToPixel)0;
    float4x4 preViewProjection = mul (xView, xProjection);
    float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);


    Output.Position = mul(inPos, preWorldViewProjection);
	Output.Position3D = mul(inPos, xWorld);
    Output.TextureCoords = inTexCoords;
    Output.clipDistances = dot(mul(inPos, xWorld), ClipPlane0); 
	Output.Color = inColor * xTint;


	Output.Fog = saturate((Output.Position.z - xFogStart) / (xFogEnd - xFogStart));
	Output.TextureBounds = inTexSource;
    return Output;
}


float2 ClampTexture(float2 uv, float4 bounds)
{	
	
	return float2(clamp(uv.x, bounds.x, bounds.z), clamp(uv.y, bounds.y, bounds.w));
}

TPixelToFrame TexturedPS(TVertexToPixel PSIn)
{
    TPixelToFrame Output = (TPixelToFrame)0;
	
    Output.Color = PSIn.Color;

    //Output.Color =  tex2D(SunSampler, float2(PSIn.Color.r * (1.0f - xTimeOfDay), 0.5f));
	//Output.Color.rgb += tex2D(TorchSampler, float2(PSIn.Color.b + (sin(xTime * 10.0f) + 1.0f) * 0.01f * PSIn.Color.b, 0.5f));
	//saturate(Output.Color.rgb);

	//Output.Color.rgb *=  tex2D(AmbientSampler, float2(PSIn.Color.g, 0.5f));
    
	float4 texColor = tex2D(TextureSampler, ClampTexture(PSIn.TextureCoords, PSIn.TextureBounds));
	float4 illumColor = tex2D(IllumSampler, ClampTexture(PSIn.TextureCoords, PSIn.TextureBounds));


	if(xEnableLighting)
	{
		float3 pos = PSIn.Position3D;
		float dist = (pow(pos.x - xLightPos.x, 2) + pow(pos.y - xLightPos.y, 2) + pow(pos.z - xLightPos.z, 2) + 0.001);
		Output.Color.rgb = saturate(Output.Color * (1.0f - xLightColor.b * 0.8f) + xLightColor/ dist);
	}

	Output.Color.rgba *= texColor;


	if(SelfIllumination)
		Output.Color.rgba = lerp(Output.Color.rgba, texColor, illumColor.r); 
	

	Output.Color.rgba = float4(lerp(Output.Color.rgb, xFogColor, PSIn.Fog) * Output.Color.a, Output.Color.a);

    if (Clipping)  clip(PSIn.clipDistances);  //MSS - Water Refactor added


    return Output;
}

technique Textured_2_0
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 TexturedVS();
        PixelShader = compile ps_2_0 TexturedPS();
    }
}

technique Textured
{
    pass Pass0
    {   
        VertexShader = compile vs_2_0 TexturedVS();
        PixelShader  = compile ps_2_0 TexturedPS();
    }
}



//------- Technique: Water --------
struct WVertexToPixel
{
     float4 Position                 : POSITION;
	 float2 TextureSamplingPos        : TEXCOORD5;
     float4 ReflectionMapSamplingPos    : TEXCOORD1;
     float2 BumpMapSamplingPos        : TEXCOORD2;
     float4 RefractionMapSamplingPos : TEXCOORD3;
     float4 Position3D                : TEXCOORD4;
	 float2 UnMovedTextureSamplingPos : TEXCOORD6;
	 float4 Color : COLOR0;
	 float Fog : TEXCOORD7;

};

struct WPixelToFrame
{
     float4 Color : COLOR0;
};

WVertexToPixel WaterVS(float4 inPos : POSITION, float2 inTex: TEXCOORD0, float4 inColor : COLOR0)
{    
     WVertexToPixel Output = (WVertexToPixel)0;

     float4x4 preViewProjection = mul (xView, xProjection);
     float4x4 preWorldViewProjection = mul (xWorld, preViewProjection);
     float4x4 preReflectionViewProjection = mul (xReflectionView, xProjection);
     float4x4 preWorldReflectionViewProjection = mul (xWorld, preReflectionViewProjection);
	 inPos.y += sin(xTime * 2.0f) * sin(inPos.x) * cos(inPos.z + xTime) * 0.05f * (1.0f - inColor.g) ;
	 inPos.x += cos(xTime * 2.0f) * sin(inPos.x) * cos(inPos.z + xTime) * 0.05f * (1.0f - inColor.g) ;
	 inPos.z += sin(xTime * 2.0f) * sin(inPos.x) * cos(inPos.z + xTime) * 0.05f * (1.0f - inColor.g) ;
     Output.Position = mul(inPos, preWorldViewProjection);

     Output.ReflectionMapSamplingPos = mul(inPos, preWorldReflectionViewProjection);

     Output.RefractionMapSamplingPos = mul(inPos, preWorldViewProjection);
     Output.Position3D = mul(inPos, xWorld);
	 Output.BumpMapSamplingPos = inTex/xWaveLength;

     float3 windDir = normalize(xWindDirection);    
     float3 perpDir = cross(xWindDirection, float3(0,1,0));
     float ydot = dot(inTex, xWindDirection.xz);
     float xdot = dot(inTex, perpDir.xz);
     float2 moveVector = float2(xdot, ydot);
     moveVector.y += xTime*xWindForce;    
	 float2 moveVector2 = float2(xdot, ydot);
     moveVector2.y += xTime*xWindForce * 0.1f;    
     
	 Output.BumpMapSamplingPos = moveVector/xWaveLength;   
	 Output.TextureSamplingPos = moveVector2/xWaveLength * 90; 
	 Output.UnMovedTextureSamplingPos = inTex;
	 Output.Color = inColor;
	 Output.Fog = saturate((Output.Position.z - xFogStart) / (xFogEnd - xFogStart));

     return Output;
}

WPixelToFrame WaterPS(WVertexToPixel PSIn)
{
    WPixelToFrame Output = (WPixelToFrame)0;   
	Output.Color = PSIn.Color;     

    float4 bumpColor = tex2D(WaterBumpMapSampler, PSIn.BumpMapSamplingPos);
    float2 perturbation = xWaveHeight*(bumpColor.rg - 0.5f)*2.0f;
    
    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = PSIn.ReflectionMapSamplingPos.x/PSIn.ReflectionMapSamplingPos.w/2.0f + 0.5f;
    ProjectedTexCoords.y = -PSIn.ReflectionMapSamplingPos.y/PSIn.ReflectionMapSamplingPos.w/2.0f + 0.5f;        
    float2 perturbatedTexCoords = ProjectedTexCoords + perturbation;
    float4 reflectiveColor = tex2D(ReflectionSampler, perturbatedTexCoords);
    
    float2 ProjectedRefrTexCoords;
    ProjectedRefrTexCoords.x = PSIn.RefractionMapSamplingPos.x/PSIn.RefractionMapSamplingPos.w/2.0f + 0.5f;
    ProjectedRefrTexCoords.y = -PSIn.RefractionMapSamplingPos.y/PSIn.RefractionMapSamplingPos.w/2.0f + 0.5f;    
    float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords + perturbation;    
    float4 refractiveColor = tex2D(RefractionSampler, perturbatedRefrTexCoords);


    float3 eyeVector = normalize(xCamPos - PSIn.Position3D);

    float3 normalVector = (bumpColor.rbg-0.5f);
    
    float fresnelTerm = dot(eyeVector, normalVector);   
    float4 combinedColor = lerp(reflectiveColor, refractiveColor, fresnelTerm);
    
    float4 dullColor = tex2D(TextureSampler, PSIn.TextureSamplingPos);
    float4 sloshColor = tex2D(TextureSampler1, PSIn.TextureSamplingPos + perturbation);
	//float4 puddleColor = tex2D(TextureSampler2, PSIn.UnMovedTextureSamplingPos);
	float r = PSIn.Color.r;


    Output.Color = lerp(combinedColor, dullColor, 0.3f * PSIn.Color.b * (1.0f - xTimeOfDay));    
    Output.Color = lerp(Output.Color, sloshColor, r * 0.7 * (1.0f - xTimeOfDay));


    //float3 reflectionVector = -reflect(xLightDirection, normalVector);
    //float specular = dot(normalize(reflectionVector), normalize(eyeVector));
    //specular = pow(abs(specular), 256);        
    //Output.Color.rgb += specular * (1.0f - xTimeOfDay);


    Output.Color.rgba = float4(lerp(Output.Color.rgb, xFogColor, PSIn.Fog) * Output.Color.a, Output.Color.a);

	float st = (sin(xTime + r * 20) + 1.0) * 0.2;
	if(r > st + 0.4)
	{
		Output.Color.rgba += float4(0.1, 0.1, 0.1, 0);
	}

    return Output;
}




technique Water
{
     pass Pass0
     {
         VertexShader = compile vs_1_1 WaterVS();
         PixelShader = compile ps_2_0 WaterPS();

     }
}
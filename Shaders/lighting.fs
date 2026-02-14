#version 330

in vec3 fragPosition;
in vec3 fragLocalPosition;
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;
in vec3 fragLocalNormal;

uniform sampler2D texture0;
uniform sampler2D triplanarParamsTex;
uniform vec4 colDiffuse;

out vec4 finalColor;

#define LIGHT_DIRECTIONAL 0
#define LIGHT_POINT 1

uniform vec4 ambient;
uniform vec3 viewPos;
uniform int tileSize;
uniform int totalLights;
uniform ivec2 screenSize;
uniform ivec2 tileCount;
uniform ivec2 lightDataTexSize;
uniform ivec2 tileHeaderTexSize;
uniform ivec2 tileIndexTexSize;
uniform sampler2D lightDataTex;
uniform sampler2D tileHeaderTex;
uniform sampler2D tileIndexTex;

vec4 FetchPacked(sampler2D tex, ivec2 texSize, int index)
{
    int x = index % texSize.x;
    int y = index / texSize.x;
    return texelFetch(tex, ivec2(x, y), 0);
}

vec4 FetchLightTexel(int lightIndex, int texelOffset)
{
    return FetchPacked(lightDataTex, lightDataTexSize, lightIndex * 4 + texelOffset);
}

vec4 SampleAlbedo()
{
    vec4 triplanarParams = texelFetch(triplanarParamsTex, ivec2(0, 0), 0);
    float mode = triplanarParams.x;
    if (mode < 0.5)
        return texture(texture0, fragTexCoord);

    float scale = max(triplanarParams.y, 0.0001);
    float blendSharpness = max(triplanarParams.z, 0.0001);
    bool useWorldSpace = triplanarParams.w >= 0.5;

    vec3 samplePosition = useWorldSpace ? fragPosition : fragLocalPosition;
    vec3 projectionNormal = useWorldSpace ? normalize(fragNormal) : normalize(fragLocalNormal);
    vec3 weights = abs(projectionNormal);
    weights = pow(weights, vec3(blendSharpness));
    float weightSum = max(weights.x + weights.y + weights.z, 0.0001);
    weights /= weightSum;

    vec2 uvX = samplePosition.zy * scale;
    vec2 uvY = samplePosition.xz * scale;
    vec2 uvZ = samplePosition.xy * scale;

    vec4 xProj = texture(texture0, uvX);
    vec4 yProj = texture(texture0, uvY);
    vec4 zProj = texture(texture0, uvZ);
    return xProj * weights.x + yProj * weights.y + zProj * weights.z;
}

void main()
{
    vec4 texelColor = SampleAlbedo();
    vec3 normal = normalize(fragNormal);
    vec3 lightEffect = ambient.rgb;
    ivec2 pixel = ivec2(clamp(gl_FragCoord.xy, vec2(0.0), vec2(screenSize) - vec2(1.0)));
    int topLeftY = (screenSize.y - 1) - pixel.y;
    ivec2 tile = ivec2(
        clamp(pixel.x / max(tileSize, 1), 0, max(tileCount.x - 1, 0)),
        clamp(topLeftY / max(tileSize, 1), 0, max(tileCount.y - 1, 0))
    );
    int tileIndex = tile.y * tileCount.x + tile.x;
    vec4 tileHeader = FetchPacked(tileHeaderTex, tileHeaderTexSize, tileIndex);
    int listStart = int(tileHeader.x + 0.5);
    int listCount = int(tileHeader.y + 0.5);

    for (int i = 0; i < listCount; i++)
    {
        int packedIndex = listStart + i;
        int lightIndex = int(FetchPacked(tileIndexTex, tileIndexTexSize, packedIndex).x + 0.5);
        if (lightIndex < 0 || lightIndex >= totalLights)
            continue;

        vec4 lightMeta = FetchLightTexel(lightIndex, 0);
        int lightType = int(lightMeta.x + 0.5);
        float lightRange = lightMeta.z;

        vec3 lightDir;
        float attenuation = 1.0;

        if (lightType == LIGHT_DIRECTIONAL)
        {
            vec3 direction = FetchLightTexel(lightIndex, 2).xyz;
            lightDir = -normalize(direction);
        }
        else if (lightType == LIGHT_POINT)
        {
            vec3 lightPos = FetchLightTexel(lightIndex, 1).xyz;
            vec3 toLight = lightPos - fragPosition;
            float dist = length(toLight);

            if (dist <= 0.0001 || dist > lightRange)
                continue;

            lightDir = toLight / dist;

            float ratio = clamp(dist / max(lightRange, 0.0001), 0.0, 1.0);
            attenuation = 1.0 - ratio;
            attenuation *= attenuation;
        }
        else
        {
            continue;
        }

        vec3 lightColor = FetchLightTexel(lightIndex, 3).rgb;

        // Diffuse
        float NdotL = max(dot(normal, lightDir), 0.0);
        vec3 diffuse = lightColor * NdotL * attenuation;

        lightEffect += diffuse;
    }

    finalColor = (texelColor * colDiffuse * fragColor) * vec4(lightEffect, 1.0);
    finalColor.a = texelColor.a * colDiffuse.a * fragColor.a;
}

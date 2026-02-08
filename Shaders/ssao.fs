#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;    // scene color (unused, but bound as texture0 by Blit)
uniform sampler2D depthTex;    // linear depth
uniform sampler2D noiseTex;    // 4x4 noise texture

uniform vec2 screenSize;
uniform float radius;
uniform float bias;
uniform float intensity;
uniform int sampleCount;
uniform float nearPlane;
uniform float farPlane;

// Hemisphere samples (max 64)
uniform vec3 samples[64];

out vec4 finalColor;

vec3 reconstructNormal(vec2 uv)
{
    vec2 texelSize = 1.0 / screenSize;
    float depthCenter = texture(depthTex, uv).r;
    float depthLeft   = texture(depthTex, uv - vec2(texelSize.x, 0.0)).r;
    float depthRight  = texture(depthTex, uv + vec2(texelSize.x, 0.0)).r;
    float depthUp     = texture(depthTex, uv - vec2(0.0, texelSize.y)).r;
    float depthDown   = texture(depthTex, uv + vec2(0.0, texelSize.y)).r;

    vec3 dx = vec3(texelSize.x * 2.0, 0.0, depthRight - depthLeft);
    vec3 dy = vec3(0.0, texelSize.y * 2.0, depthDown - depthUp);

    return normalize(cross(dx, dy));
}

void main()
{
    float depth = texture(depthTex, fragTexCoord).r;

    // Skip sky/background
    if (depth >= 0.999)
    {
        finalColor = vec4(1.0, 1.0, 1.0, 1.0);
        return;
    }

    vec2 noiseScale = screenSize / 4.0;
    vec3 randomVec = texture(noiseTex, fragTexCoord * noiseScale).rgb * 2.0 - 1.0;
    vec3 normal = reconstructNormal(fragTexCoord);

    // Gram-Schmidt to create TBN
    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN = mat3(tangent, bitangent, normal);

    float occlusion = 0.0;
    int actualSamples = min(sampleCount, 64);

    for (int i = 0; i < actualSamples; i++)
    {
        vec3 sampleDir = TBN * samples[i];
        vec2 sampleUV = fragTexCoord + sampleDir.xy * radius / screenSize;

        if (sampleUV.x < 0.0 || sampleUV.x > 1.0 || sampleUV.y < 0.0 || sampleUV.y > 1.0)
            continue;

        float sampleDepth = texture(depthTex, sampleUV).r;
        float depthDiff = depth - sampleDepth;

        // Range check: only count occlusion within the radius
        float rangeCheck = smoothstep(0.0, 1.0, radius / (abs(depthDiff * (farPlane - nearPlane)) + 0.001));
        occlusion += (depthDiff > bias / (farPlane - nearPlane) ? 1.0 : 0.0) * rangeCheck;
    }

    occlusion = 1.0 - (occlusion / float(actualSamples)) * intensity;
    finalColor = vec4(occlusion, occlusion, occlusion, 1.0);
}

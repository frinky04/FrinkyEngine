#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;    // scene color (unused, but bound as texture0 by Blit)
uniform sampler2D depthTex;    // linear depth (normalized 0..1)
uniform sampler2D noiseTex;    // 4x4 noise texture

uniform float tanHalfFov;
uniform float aspectRatio;
uniform float radius;
uniform float bias;
uniform float intensity;
uniform int sampleCount;
uniform float nearPlane;
uniform float farPlane;

// Hemisphere samples (max 64)
uniform vec3 samples[64];

out vec4 finalColor;

// Reconstruct view-space position from UV and normalized depth
vec3 viewPosFromUV(vec2 uv, float normDepth)
{
    float linearDepth = normDepth * (farPlane - nearPlane) + nearPlane;
    // UV 0..1 -> NDC -1..1
    vec2 ndc = uv * 2.0 - 1.0;
    float viewX = ndc.x * aspectRatio * tanHalfFov * linearDepth;
    float viewY = ndc.y * tanHalfFov * linearDepth;
    return vec3(viewX, viewY, -linearDepth);
}

void main()
{
    float normDepth = texture(depthTex, fragTexCoord).r;

    // Skip sky/background
    if (normDepth >= 0.999)
    {
        finalColor = vec4(1.0, 1.0, 1.0, 1.0);
        return;
    }

    vec3 viewPos = viewPosFromUV(fragTexCoord, normDepth);

    // Reconstruct normal from view-space position using screen-space derivatives
    vec3 normal = normalize(cross(dFdx(viewPos), dFdy(viewPos)));

    // Noise for random rotation in tangent plane
    vec2 noiseScale = vec2(textureSize(depthTex, 0)) / 4.0;
    vec3 randomVec = texture(noiseTex, fragTexCoord * noiseScale).rgb * 2.0 - 1.0;

    // Gram-Schmidt to create TBN from normal and random vector
    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN = mat3(tangent, bitangent, normal);

    float occlusion = 0.0;
    int actualSamples = min(sampleCount, 64);

    for (int i = 0; i < actualSamples; i++)
    {
        // Orient sample in view space via TBN
        vec3 samplePos = viewPos + TBN * samples[i] * radius;

        // Project sample position back to screen UV
        // viewPos has z negative, so linearDepth = -samplePos.z
        float sampleLinearDepth = -samplePos.z;
        if (sampleLinearDepth <= nearPlane) continue;

        vec2 sampleNDC = vec2(
            samplePos.x / (sampleLinearDepth * aspectRatio * tanHalfFov),
            samplePos.y / (sampleLinearDepth * tanHalfFov)
        );
        vec2 sampleUV = sampleNDC * 0.5 + 0.5;

        if (sampleUV.x < 0.0 || sampleUV.x > 1.0 || sampleUV.y < 0.0 || sampleUV.y > 1.0)
            continue;

        // Read actual depth at the sample's screen position
        float occluderNormDepth = texture(depthTex, sampleUV).r;
        float occluderLinearDepth = occluderNormDepth * (farPlane - nearPlane) + nearPlane;

        // Check if the occluder is in front of the sample (closer to camera)
        float depthDiff = sampleLinearDepth - occluderLinearDepth;

        // Range check in world-space units
        float rangeCheck = smoothstep(0.0, 1.0, radius / (abs(depthDiff) + 0.001));
        occlusion += (depthDiff > bias ? 1.0 : 0.0) * rangeCheck;
    }

    occlusion = 1.0 - (occlusion / float(actualSamples)) * intensity;
    finalColor = vec4(occlusion, occlusion, occlusion, 1.0);
}

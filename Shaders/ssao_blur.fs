#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;    // SSAO result
uniform sampler2D depthTex;    // linear depth (normalized 0..1)
uniform vec2 texelSize;
uniform vec2 direction;        // (1,0) for horizontal, (0,1) for vertical
uniform int blurSize;
uniform float nearPlane;
uniform float farPlane;

out vec4 finalColor;

float linearizeDepth(float normDepth)
{
    return normDepth * (farPlane - nearPlane) + nearPlane;
}

void main()
{
    float centerAo = texture(texture0, fragTexCoord).r;
    float centerDepth = linearizeDepth(texture(depthTex, fragTexCoord).r);

    float result = 0.0;
    float totalWeight = 0.0;

    int halfSize = blurSize;
    float sigma = float(halfSize) + 1.0;

    for (int i = -halfSize; i <= halfSize; i++)
    {
        vec2 offset = direction * float(i) * texelSize;
        vec2 sampleUV = fragTexCoord + offset;

        float sampleAo = texture(texture0, sampleUV).r;
        float sampleDepth = linearizeDepth(texture(depthTex, sampleUV).r);

        // Spatial weight: Gaussian falloff
        float spatialWeight = exp(-0.5 * float(i * i) / (sigma * sigma));

        // Edge weight: depth similarity (adaptive threshold based on distance)
        float depthDiff = abs(sampleDepth - centerDepth);
        float depthWeight = 1.0 - smoothstep(0.0, 0.1 * centerDepth, depthDiff);

        float weight = spatialWeight * depthWeight;
        result += sampleAo * weight;
        totalWeight += weight;
    }

    result /= max(totalWeight, 0.001);
    finalColor = vec4(result, result, result, 1.0);
}

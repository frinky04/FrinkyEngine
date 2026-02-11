#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;    // scene color
uniform sampler2D depthTex;    // linear depth (R channel, 0..1)
uniform vec3 fogColor;
uniform float fogStart;
uniform float fogEnd;
uniform float fogDensity;
uniform int fogMode;           // 0 = linear, 1 = exp, 2 = exp2
uniform float nearPlane;
uniform float farPlane;
uniform float tanHalfFov;
uniform float aspectRatio;

out vec4 finalColor;

void main()
{
    vec4 sceneColor = texture(texture0, fragTexCoord);
    float normalizedDepth = texture(depthTex, fragTexCoord).r;

    // Reconstruct linear depth in world units
    float linearDepth = normalizedDepth * (farPlane - nearPlane) + nearPlane;

    // Convert linear Z-depth to radial (Euclidean) distance for spherical fog
    vec2 ndc = fragTexCoord * 2.0 - 1.0;
    float viewX = ndc.x * aspectRatio * tanHalfFov;
    float viewY = ndc.y * tanHalfFov;
    float radialDistance = linearDepth * sqrt(viewX * viewX + viewY * viewY + 1.0);

    float fogFactor = 0.0;

    if (fogMode == 0) // Linear
    {
        fogFactor = clamp((fogEnd - radialDistance) / (fogEnd - fogStart), 0.0, 1.0);
    }
    else if (fogMode == 1) // Exponential
    {
        fogFactor = exp(-fogDensity * radialDistance);
        fogFactor = clamp(fogFactor, 0.0, 1.0);
    }
    else // Exponential squared
    {
        float f = fogDensity * radialDistance;
        fogFactor = exp(-f * f);
        fogFactor = clamp(fogFactor, 0.0, 1.0);
    }

    finalColor = vec4(mix(fogColor, sceneColor.rgb, fogFactor), sceneColor.a);
}

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

out vec4 finalColor;

void main()
{
    vec4 sceneColor = texture(texture0, fragTexCoord);
    float normalizedDepth = texture(depthTex, fragTexCoord).r;

    // Reconstruct linear depth in world units
    float linearDepth = normalizedDepth * (farPlane - nearPlane) + nearPlane;

    float fogFactor = 0.0;

    if (fogMode == 0) // Linear
    {
        fogFactor = clamp((fogEnd - linearDepth) / (fogEnd - fogStart), 0.0, 1.0);
    }
    else if (fogMode == 1) // Exponential
    {
        fogFactor = exp(-fogDensity * linearDepth);
        fogFactor = clamp(fogFactor, 0.0, 1.0);
    }
    else // Exponential squared
    {
        float f = fogDensity * linearDepth;
        fogFactor = exp(-f * f);
        fogFactor = clamp(fogFactor, 0.0, 1.0);
    }

    finalColor = vec4(mix(fogColor, sceneColor.rgb, fogFactor), sceneColor.a);
}

#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0; // unused, but Raylib binds scene color here
uniform sampler2D depthTex; // hardware depth texture from scene RT
uniform float nearPlane;
uniform float farPlane;

out vec4 finalColor;

void main()
{
    // Sample the hardware depth buffer value in [0,1]
    float rawDepth = texture(depthTex, fragTexCoord).r;

    // Convert to NDC [-1,1] (OpenGL convention)
    float ndcDepth = rawDepth * 2.0 - 1.0;

    // Linearize depth
    float linearDepth = (2.0 * nearPlane * farPlane) / (farPlane + nearPlane - ndcDepth * (farPlane - nearPlane));

    // Normalize to [0,1] range
    float normalizedDepth = (linearDepth - nearPlane) / (farPlane - nearPlane);

    finalColor = vec4(normalizedDepth, normalizedDepth, normalizedDepth, 1.0);
}

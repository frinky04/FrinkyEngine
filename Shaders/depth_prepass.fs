#version 330

uniform float nearPlane;
uniform float farPlane;

out vec4 finalColor;

void main()
{
    // gl_FragCoord.z is the hardware-rasterized depth buffer value in [0,1]
    // Convert to NDC [-1,1] (OpenGL convention)
    float ndcDepth = gl_FragCoord.z * 2.0 - 1.0;

    // Linearize depth
    float linearDepth = (2.0 * nearPlane * farPlane) / (farPlane + nearPlane - ndcDepth * (farPlane - nearPlane));

    // Normalize to [0,1] range
    float normalizedDepth = (linearDepth - nearPlane) / (farPlane - nearPlane);

    finalColor = vec4(normalizedDepth, normalizedDepth, normalizedDepth, 1.0);
}

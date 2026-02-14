#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;    // scene color
uniform float colorLevels;     // number of color levels per channel (e.g. 32.0)
uniform float ditherStrength;  // blend between original and dithered (0..1)

out vec4 finalColor;

void main()
{
    // 4x4 Bayer matrix (normalized to 0..1)
    const float bayerMatrix[16] = float[16](
         0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
        12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
         3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
        15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
    );

    int x = int(mod(gl_FragCoord.x, 4.0));
    int y = int(mod(gl_FragCoord.y, 4.0));
    float bayerValue = bayerMatrix[y * 4 + x];

    vec3 color = texture(texture0, fragTexCoord).rgb;
    float alpha = texture(texture0, fragTexCoord).a;

    // Quantize with dither offset
    float levels = max(colorLevels, 2.0);
    vec3 dithered = floor(color * levels + bayerValue - 0.5) / levels;
    dithered = clamp(dithered, 0.0, 1.0);

    // Blend between original and dithered based on strength
    vec3 result = mix(color, dithered, ditherStrength);

    finalColor = vec4(result, alpha);
}

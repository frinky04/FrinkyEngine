#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;    // scene color
uniform sampler2D paletteTex;  // Nx1 palette texture
uniform int paletteSize;       // number of colors in palette

out vec4 finalColor;

void main()
{
    vec3 color = texture(texture0, fragTexCoord).rgb;
    float alpha = texture(texture0, fragTexCoord).a;

    // Find nearest palette color by brute-force distance search
    float bestDist = 999999.0;
    vec3 bestColor = vec3(0.0);

    for (int i = 0; i < paletteSize; i++)
    {
        // Sample from center of each texel in the Nx1 texture
        float u = (float(i) + 0.5) / float(paletteSize);
        vec3 palColor = texture(paletteTex, vec2(u, 0.5)).rgb;

        vec3 diff = color - palColor;
        float dist = dot(diff, diff);

        if (dist < bestDist)
        {
            bestDist = dist;
            bestColor = palColor;
        }
    }

    finalColor = vec4(bestColor, alpha);
}

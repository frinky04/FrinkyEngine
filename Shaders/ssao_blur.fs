#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;    // SSAO result
uniform vec2 texelSize;
uniform int blurSize;

out vec4 finalColor;

void main()
{
    float result = 0.0;
    int halfSize = blurSize;
    float total = 0.0;

    float centerDepth = texture(texture0, fragTexCoord).r;

    for (int x = -halfSize; x <= halfSize; x++)
    {
        for (int y = -halfSize; y <= halfSize; y++)
        {
            vec2 offset = vec2(float(x), float(y)) * texelSize;
            float sampleVal = texture(texture0, fragTexCoord + offset).r;

            // Edge-preserving: weight by depth similarity
            float weight = 1.0 - smoothstep(0.0, 0.05, abs(sampleVal - centerDepth));
            result += sampleVal * weight;
            total += weight;
        }
    }

    result /= max(total, 0.001);
    finalColor = vec4(result, result, result, 1.0);
}

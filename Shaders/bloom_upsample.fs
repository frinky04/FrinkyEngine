#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform vec2 texelSize;
uniform float weight;

out vec4 finalColor;

void main()
{
    // 9-tap tent filter for smooth upsample, scaled by per-mip scatter weight
    vec4 a = texture(texture0, fragTexCoord + texelSize * vec2(-1.0, -1.0));
    vec4 b = texture(texture0, fragTexCoord + texelSize * vec2( 0.0, -1.0));
    vec4 c = texture(texture0, fragTexCoord + texelSize * vec2( 1.0, -1.0));
    vec4 d = texture(texture0, fragTexCoord + texelSize * vec2(-1.0,  0.0));
    vec4 e = texture(texture0, fragTexCoord);
    vec4 f = texture(texture0, fragTexCoord + texelSize * vec2( 1.0,  0.0));
    vec4 g = texture(texture0, fragTexCoord + texelSize * vec2(-1.0,  1.0));
    vec4 h = texture(texture0, fragTexCoord + texelSize * vec2( 0.0,  1.0));
    vec4 i = texture(texture0, fragTexCoord + texelSize * vec2( 1.0,  1.0));

    finalColor = e * 4.0;
    finalColor += (b + d + f + h) * 2.0;
    finalColor += (a + c + g + i);
    finalColor = (finalColor / 16.0) * weight;
}

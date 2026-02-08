#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform vec2 texelSize;

out vec4 finalColor;

void main()
{
    // 13-tap box filter for high quality downsample
    vec4 A = texture(texture0, fragTexCoord + texelSize * vec2(-1.0, -1.0));
    vec4 B = texture(texture0, fragTexCoord + texelSize * vec2( 0.0, -1.0));
    vec4 C = texture(texture0, fragTexCoord + texelSize * vec2( 1.0, -1.0));
    vec4 D = texture(texture0, fragTexCoord + texelSize * vec2(-0.5, -0.5));
    vec4 E = texture(texture0, fragTexCoord + texelSize * vec2( 0.5, -0.5));
    vec4 F = texture(texture0, fragTexCoord + texelSize * vec2(-1.0,  0.0));
    vec4 G = texture(texture0, fragTexCoord);
    vec4 H = texture(texture0, fragTexCoord + texelSize * vec2( 1.0,  0.0));
    vec4 I = texture(texture0, fragTexCoord + texelSize * vec2(-0.5,  0.5));
    vec4 J = texture(texture0, fragTexCoord + texelSize * vec2( 0.5,  0.5));
    vec4 K = texture(texture0, fragTexCoord + texelSize * vec2(-1.0,  1.0));
    vec4 L = texture(texture0, fragTexCoord + texelSize * vec2( 0.0,  1.0));
    vec4 M = texture(texture0, fragTexCoord + texelSize * vec2( 1.0,  1.0));

    vec4 result = (D + E + I + J) * 0.5;
    result += (A + B + F + G) * 0.125;
    result += (B + C + G + H) * 0.125;
    result += (F + G + K + L) * 0.125;
    result += (G + H + L + M) * 0.125;

    finalColor = result;
}

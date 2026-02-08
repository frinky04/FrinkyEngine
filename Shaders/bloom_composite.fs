#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;    // scene color
uniform sampler2D bloomTex;    // bloom texture
uniform float intensity;

out vec4 finalColor;

void main()
{
    vec4 sceneColor = texture(texture0, fragTexCoord);
    vec4 bloom = texture(bloomTex, fragTexCoord);

    finalColor = vec4(sceneColor.rgb + bloom.rgb * intensity, sceneColor.a);
}

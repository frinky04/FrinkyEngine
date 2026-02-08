#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;    // scene color
uniform sampler2D aoTex;       // blurred AO

out vec4 finalColor;

void main()
{
    vec4 sceneColor = texture(texture0, fragTexCoord);
    float ao = texture(aoTex, fragTexCoord).r;

    finalColor = vec4(sceneColor.rgb * ao, sceneColor.a);
}

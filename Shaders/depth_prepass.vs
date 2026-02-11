#version 330

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;
in mat4 instanceTransform;

uniform mat4 mvp;
uniform int useInstancing;

void main()
{
    gl_Position = useInstancing == 1
        ? mvp * instanceTransform * vec4(vertexPosition, 1.0)
        : mvp * vec4(vertexPosition, 1.0);
}

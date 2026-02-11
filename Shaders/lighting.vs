#version 330

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;
in mat4 instanceTransform;

uniform mat4 mvp;
uniform mat4 matModel;
uniform mat4 matNormal;
uniform int useInstancing;

out vec3 fragPosition;
out vec3 fragLocalPosition;
out vec2 fragTexCoord;
out vec4 fragColor;
out vec3 fragNormal;
out vec3 fragLocalNormal;

void main()
{
    mat4 modelMatrix = useInstancing == 1 ? instanceTransform : matModel;
    vec4 worldPosition = modelMatrix * vec4(vertexPosition, 1.0);

    fragPosition = worldPosition.xyz;
    fragLocalPosition = vertexPosition;
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    mat3 normalMatrix = useInstancing == 1
        ? transpose(inverse(mat3(modelMatrix)))
        : mat3(matNormal);
    fragNormal = normalize(normalMatrix * vertexNormal);
    fragLocalNormal = normalize(vertexNormal);

    gl_Position = useInstancing == 1
        ? mvp * worldPosition
        : mvp * vec4(vertexPosition, 1.0);
}

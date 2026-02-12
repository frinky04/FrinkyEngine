#version 330

#define MAX_BONE_NUM 128

in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexColor;
in vec4 vertexBoneIds;
in vec4 vertexBoneWeights;
in mat4 instanceTransform;

uniform mat4 mvp;
uniform mat4 matModel;
uniform mat4 matNormal;
uniform int useInstancing;
uniform int useSkinning;
uniform mat4 boneMatrices[MAX_BONE_NUM];

out vec3 fragPosition;
out vec3 fragLocalPosition;
out vec2 fragTexCoord;
out vec4 fragColor;
out vec3 fragNormal;
out vec3 fragLocalNormal;

void main()
{
    vec3 localPosition = vertexPosition;
    vec3 localNormal = vertexNormal;

    if (useSkinning == 1)
    {
        int boneIndex0 = int(vertexBoneIds.x);
        int boneIndex1 = int(vertexBoneIds.y);
        int boneIndex2 = int(vertexBoneIds.z);
        int boneIndex3 = int(vertexBoneIds.w);

        vec4 skinnedPosition =
            vertexBoneWeights.x * (boneMatrices[boneIndex0] * vec4(vertexPosition, 1.0)) +
            vertexBoneWeights.y * (boneMatrices[boneIndex1] * vec4(vertexPosition, 1.0)) +
            vertexBoneWeights.z * (boneMatrices[boneIndex2] * vec4(vertexPosition, 1.0)) +
            vertexBoneWeights.w * (boneMatrices[boneIndex3] * vec4(vertexPosition, 1.0));

        vec4 skinnedNormal =
            vertexBoneWeights.x * (boneMatrices[boneIndex0] * vec4(vertexNormal, 0.0)) +
            vertexBoneWeights.y * (boneMatrices[boneIndex1] * vec4(vertexNormal, 0.0)) +
            vertexBoneWeights.z * (boneMatrices[boneIndex2] * vec4(vertexNormal, 0.0)) +
            vertexBoneWeights.w * (boneMatrices[boneIndex3] * vec4(vertexNormal, 0.0));

        localPosition = skinnedPosition.xyz;
        localNormal = normalize(skinnedNormal.xyz);
    }

    mat4 modelMatrix = useInstancing == 1 ? instanceTransform : matModel;
    vec4 worldPosition = modelMatrix * vec4(localPosition, 1.0);

    fragPosition = worldPosition.xyz;
    fragLocalPosition = localPosition;
    fragTexCoord = vertexTexCoord;
    fragColor = vertexColor;
    mat3 normalMatrix = useInstancing == 1
        ? transpose(inverse(mat3(modelMatrix)))
        : mat3(matNormal);
    fragNormal = normalize(normalMatrix * localNormal);
    fragLocalNormal = normalize(localNormal);

    gl_Position = useInstancing == 1
        ? mvp * worldPosition
        : mvp * vec4(localPosition, 1.0);
}

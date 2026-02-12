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
uniform int useInstancing;
uniform int useSkinning;
uniform mat4 boneMatrices[MAX_BONE_NUM];

void main()
{
    vec3 localPosition = vertexPosition;

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
        localPosition = skinnedPosition.xyz;
    }

    gl_Position = useInstancing == 1
        ? mvp * instanceTransform * vec4(localPosition, 1.0)
        : mvp * vec4(localPosition, 1.0);
}

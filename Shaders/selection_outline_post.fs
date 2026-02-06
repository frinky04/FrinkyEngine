#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform sampler2D maskTexture;
uniform vec2 texelSize;
uniform vec4 outlineColor;
uniform float outlineWidth;

out vec4 finalColor;

float SampleMask(vec2 uv)
{
    return texture(maskTexture, uv).a;
}

void main()
{
    vec4 sceneColor = texture(texture0, fragTexCoord) * fragColor;
    float center = SampleMask(fragTexCoord);

    vec2 o = texelSize * max(1.0, outlineWidth);
    float maxNeighbor = 0.0;
    maxNeighbor = max(maxNeighbor, SampleMask(fragTexCoord + vec2( o.x,  0.0)));
    maxNeighbor = max(maxNeighbor, SampleMask(fragTexCoord + vec2(-o.x,  0.0)));
    maxNeighbor = max(maxNeighbor, SampleMask(fragTexCoord + vec2( 0.0,  o.y)));
    maxNeighbor = max(maxNeighbor, SampleMask(fragTexCoord + vec2( 0.0, -o.y)));
    maxNeighbor = max(maxNeighbor, SampleMask(fragTexCoord + vec2( o.x,  o.y)));
    maxNeighbor = max(maxNeighbor, SampleMask(fragTexCoord + vec2( o.x, -o.y)));
    maxNeighbor = max(maxNeighbor, SampleMask(fragTexCoord + vec2(-o.x,  o.y)));
    maxNeighbor = max(maxNeighbor, SampleMask(fragTexCoord + vec2(-o.x, -o.y)));

    float edge = step(0.001, maxNeighbor) * (1.0 - step(0.001, center));
    finalColor = mix(sceneColor, outlineColor, edge * outlineColor.a);
}

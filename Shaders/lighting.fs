#version 330

in vec3 fragPosition;
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;

uniform sampler2D texture0;
uniform vec4 colDiffuse;

out vec4 finalColor;

#define MAX_LIGHTS 4
#define LIGHT_DIRECTIONAL 0
#define LIGHT_POINT 1

struct Light {
    int enabled;
    int type;
    vec3 position;
    vec4 color;
};

uniform Light lights[MAX_LIGHTS];
uniform vec4 ambient;
uniform vec3 viewPos;

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);
    vec3 normal = normalize(fragNormal);
    vec3 viewDir = normalize(viewPos - fragPosition);

    vec3 lightEffect = ambient.rgb;

    for (int i = 0; i < MAX_LIGHTS; i++)
    {
        if (lights[i].enabled == 1)
        {
            vec3 lightDir;
            float attenuation = 1.0;

            if (lights[i].type == LIGHT_DIRECTIONAL)
            {
                lightDir = -normalize(lights[i].position);
            }
            else
            {
                lightDir = normalize(lights[i].position - fragPosition);
                float dist = length(lights[i].position - fragPosition);
                attenuation = 1.0 / (1.0 + 0.09 * dist + 0.032 * dist * dist);
            }

            // Diffuse
            float NdotL = max(dot(normal, lightDir), 0.0);
            vec3 diffuse = lights[i].color.rgb * NdotL * attenuation;

            // Specular
            vec3 reflectDir = reflect(-lightDir, normal);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
            vec3 specular = lights[i].color.rgb * spec * 0.5 * attenuation;

            lightEffect += diffuse + specular;
        }
    }

    finalColor = (texelColor * colDiffuse * fragColor) * vec4(lightEffect, 1.0);
    finalColor.a = texelColor.a * colDiffuse.a * fragColor.a;
}

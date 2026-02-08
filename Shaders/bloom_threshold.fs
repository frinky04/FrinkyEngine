#version 330

in vec2 fragTexCoord;

uniform sampler2D texture0;
uniform float threshold;
uniform float softKnee;

out vec4 finalColor;

void main()
{
    vec4 color = texture(texture0, fragTexCoord);
    float brightness = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));

    // Soft threshold with knee
    float knee = threshold * softKnee;
    float soft = brightness - threshold + knee;
    soft = clamp(soft, 0.0, 2.0 * knee);
    soft = soft * soft / (4.0 * knee + 0.00001);

    float contribution = max(soft, brightness - threshold);
    contribution /= max(brightness, 0.00001);

    finalColor = vec4(color.rgb * contribution, 1.0);
}

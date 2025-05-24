#version 330

// Input vertex attributes (from vertex shader)
in vec2 fragTexCoord;
in vec4 fragColor;

// Output fragment color
out vec4 finalColor;

// Parameters set by program
uniform vec4 teamColor1;
uniform vec4 teamColor2;
uniform vec4 teamColor3;

void main()
{
    finalColor = clamp((teamColor1 * fragColor.x + teamColor2 * fragColor.y + teamColor3 * fragColor.z), 0.0, 1.0);
    finalColor.w = 0.5;
}

// Note: This doesn't work.
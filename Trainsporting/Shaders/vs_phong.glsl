#version 330

in vec3 vPosition;
in vec3 vNormal;
in vec2 texcoord;

out vec3 v_norm;
out vec3 v_pos;
out vec2 f_texcoord;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
void main()
{
	 gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(vPosition, 1.0);
	 f_texcoord = texcoord;

	 mat3 normMatrix = transpose(inverse(mat3(modelMatrix)));
	 v_norm = normMatrix * vNormal;
	 v_pos = (modelMatrix * vec4(vPosition, 1.0)).xyz;
}


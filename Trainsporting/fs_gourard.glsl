#version 330

in vec4 specular_lighting[5]; 
in vec2 f_texcoord;
in vec4 ambient_lighting[5];
in vec4 diffuse_lighting[5];
in float attenuation[5];
out vec4 outputColor;
 
uniform sampler2D maintexture;
 
void main()
{
	// Texture information
	
	vec2 flipped_f_texcoord = vec2(f_texcoord.x, 1.0 - f_texcoord.y);
	vec4 texcolor = texture2D(maintexture, flipped_f_texcoord.xy);
	outputColor = vec4(0,0,0,1);
	 for(int i = 0; i < 5; i++){
		outputColor = outputColor + (vec4(0,0,0,1) + specular_lighting[i] + ambient_lighting[i]*texcolor + diffuse_lighting[i]*texcolor) * attenuation[i];
	}
}

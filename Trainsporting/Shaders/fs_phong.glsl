#version 330

// Holds information about a light
struct Light {
 vec3 position;
 vec3 color;
 float ambientIntensity;
 float diffuseIntensity;
 float specularIntensity;

 int type;
 vec3 direction;
 float coneAngle;

 float linearAttenuation;
 float quadraticAttenuation;
 float radius;
};

in vec3 v_norm;
in vec3 v_pos;
in vec2 f_texcoord;
out vec4 outputColor;

uniform int mode;

// Texture information
uniform sampler2D maintexture;

uniform mat4 viewMatrix;

// Material information
uniform vec3 material_ambient;
uniform vec3 material_diffuse;
uniform vec3 material_specular;
uniform float material_specExponent;

// Array of lights used in the shader
uniform Light lights[7];

void
main()
{
 outputColor = vec4(0,0,0,1);

 // Texture information
 vec2 flipped_texcoord = vec2(f_texcoord.x, 1.0 - f_texcoord.y);
 vec4 texcolor = texture2D(maintexture, flipped_texcoord.xy);

 vec3 normalized_normal = normalize(v_norm);
 
 // Loop through lights, adding the lighting from each one
 for(int i = 0; i < 7; i++)
 {
  
   // Skip lights with no effect
   if(lights[i].color == vec3(0,0,0))
   {
     continue;
   }
  
   vec3 lightvec = normalize(lights[i].position - v_pos);
   vec4 lightcolor = vec4(0,0,0,1);

   // Check spotlight angle
   bool inCone = false;
   if(lights[i].type == 1 && degrees(acos(dot(lightvec, lights[i].direction))) < lights[i].coneAngle)
   {
	 inCone = true;
   }
   if(lights[i].type != 1 || inCone)
   {

	  // Colors
	  vec4 light_ambient = lights[i].ambientIntensity * vec4(lights[i].color, 0.0);
	  vec4 light_diffuse = lights[i].diffuseIntensity * vec4(lights[i].color, 0.0);
	  vec4 light_specular = lights[i].specularIntensity * vec4(lights[i].color, 0.0);

	  // Ambient lighting
	  lightcolor = lightcolor + texcolor * light_ambient * vec4(material_ambient, 0.0);
 

	  // Diffuse lighting
	  float lambert_diffuse = max(dot(normalized_normal, lightvec), 0.0);

	  lightcolor = lightcolor + (light_diffuse * texcolor * vec4(material_diffuse, 0.0)) * lambert_diffuse;
  

	  // Specular lighting
	  vec3	viewvec = normalize(vec3(inverse(viewMatrix) * vec4(0,0,0,1)) - v_pos); 
	  float material_specularreflection;
	  if(mode == 0) // PHONG
	  {
		vec3 reflectionvec = (reflect(-lightvec, normalized_normal));
		material_specularreflection =  pow(max(dot(reflectionvec, viewvec), 0.0), material_specExponent);
	  }
	  else // BLINN
	  {
		vec3 halfvec = normalize(lightvec + viewvec);
		material_specularreflection = pow(max(dot(halfvec, normalized_normal), 0.0), material_specExponent * 4.0);
	  }

	  lightcolor = lightcolor + vec4(material_specular,0.0) *  light_specular  * material_specularreflection;
  

	  // Attenuation
	  float distancefactor = distance(lights[i].position, v_pos);
	  float attenuation = 1.0 / (1.0 + (distancefactor * lights[i].linearAttenuation) + (distancefactor * distancefactor * lights[i].quadraticAttenuation));
	  outputColor = outputColor + lightcolor * attenuation;
    }
  }
}
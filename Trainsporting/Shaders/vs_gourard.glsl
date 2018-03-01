#version 330

// Holds information about a light
struct Light {
 vec3 position;
 vec3 color;
 float ambientIntensity;
 float diffuseIntensity;

 int type;
 vec3 direction;
 float coneAngle;

 float linearAttenuation;
 float quadraticAttenuation;
 float radius;
};
in vec3 vPosition;
in vec3 vNormal;
in vec2 texcoord;

out vec4 specular_lighting[7];
out vec4 ambient_lighting[7];
out vec4 diffuse_lighting[7];
out float attenuation[7];
out vec2 f_texcoord;

uniform int mode;

// Texture information
uniform sampler2D maintexture;
uniform bool hasSpecularMap;
uniform sampler2D map_specular;

uniform mat4 modelview;
uniform mat4 model;
uniform mat4 view;

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
 gl_Position = modelview * vec4(vPosition, 1.0);
 f_texcoord = texcoord;

 mat3 normMatrix = transpose(inverse(mat3(model)));
 vec3 v_norm = normMatrix * vNormal;
 vec3 v_pos = (model * vec4(vPosition, 1.0)).xyz;



 vec3 normalized_normal = normalize(v_norm);
 
 // Loop through lights, adding the lighting from each one
 for(int i = 0; i < 7; i++){
  
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

  // Directional lighting
  if(lights[i].type == 2){
   lightvec = lights[i].direction;
  }

  // Colors
  vec4 light_ambient = lights[i].ambientIntensity * vec4(lights[i].color, 0.0);
  vec4 light_diffuse = lights[i].diffuseIntensity * vec4(lights[i].color, 0.0);

  // Ambient lighting
  ambient_lighting[i] = light_ambient * vec4(material_ambient, 0.0);

  // Diffuse lighting
  float lambertmaterial_diffuse = max(dot(normalized_normal, lightvec), 0.0);

  // Spotlight, limit light to specific angle
  if(lights[i].type != 1 || inCone){
  diffuse_lighting[i] = (light_diffuse * vec4(material_diffuse, 0.0)) * lambertmaterial_diffuse;
  }

  // Specular lighting
  vec3	viewvec = normalize(vec3(inverse(view) * vec4(0,0,0,1)) - v_pos); 
  float material_specularreflection;
  if(mode == 0) // PHONG
  {
	vec3 reflectionvec = (reflect(-lightvec, normalized_normal));
	material_specularreflection = max(dot(normalized_normal, lightvec), 0.0) * pow(max(dot(reflectionvec, viewvec), 0.0), material_specExponent);
  }
  else // BLINN
  {
	vec3 halfvec = normalize(lightvec + viewvec);
	material_specularreflection = max(dot(normalized_normal, lightvec), 0.0) * pow(max(dot(halfvec, normalized_normal), 0.0), material_specExponent * 4.0);
  }

  // Spotlight, specular reflections are also limited by angle
  if(lights[i].type != 1 || inCone){
   specular_lighting[i] =  vec4(material_specular * lights[i].color, 0.0) * material_specularreflection;
  }

  // Attenuation
  float distancefactor = distance(lights[i].position, v_pos);
  attenuation[i] = 1.0 / (1.0 + (distancefactor * lights[i].linearAttenuation) + (distancefactor * distancefactor * lights[i].quadraticAttenuation));
 }

}
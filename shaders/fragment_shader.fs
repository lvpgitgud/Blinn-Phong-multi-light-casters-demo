#version 330 core
out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;

struct DirLight {
    vec3 direction;
	
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct PointLight {
    vec3 position;
    
    float constant;
    float linear;
    float quadratic;
	
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct SpotLight {
    vec3 position;
    vec3 direction;
    float cutOff;
    float outerCutOff;
  
    float constant;
    float linear;
    float quadratic;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;       
};

#define NR_POINT_LIGHTS 4
uniform DirLight dirLight;
uniform PointLight pointLights[NR_POINT_LIGHTS];
uniform SpotLight spotLight;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_normal1;
uniform sampler2D texture_specular1;

uniform vec3 lightPos;
uniform vec3 viewPos;
uniform	bool directionalLightOn;
uniform	bool pointLightOn;
uniform	bool spotLightOn; 
uniform	bool blinn; 
		

// function prototypes
vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir);

void main()
{
    vec3 normal = texture(texture_normal1, fs_in.TexCoords).rgb;
    normal = normalize(normal * 2.0 - 1.0);

    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);

    vec3 result = vec3(0.0);

    // Directional light
    if (directionalLightOn){
        result += CalcDirLight(dirLight, normal, viewDir);
    }

    // Point lights
    if (pointLightOn){
        for (int i = 0; i < NR_POINT_LIGHTS; i++)
            result += CalcPointLight(pointLights[i], normal, viewDir);
    }

    // Spotlight
    if (spotLightOn){
        result += CalcSpotLight(spotLight, normal, viewDir);    
    }



    FragColor = vec4(result, 1.0);
}

// calculates the color when using a directional light.
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 viewDir) {

    vec3 lightDir = normalize(light.position - fs_in.TangentFragPos);

    float diff = max(dot(normal, lightDir), 0.0);

   //Blinn-Phong
   // vec3 halfwayDir = normalize(lightDir + viewDir);
   // float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
   float spec = 0.0;
   if(blinn) {
        vec3 halfwayDir = normalize(lightDir + viewDir);  
        spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);     
   }
   else {
       vec3 reflectDir = reflect(-lightDir, normal);
       spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
   }
   float distance    = length(light.position - fs_in.TangentFragPos);
   float attenuation = 1.0 / (light.constant + light.linear * distance + 
  			     light.quadratic * (distance * distance)); 

   vec3 albedo = texture(texture_diffuse1, fs_in.TexCoords).rgb;
   vec3 specMap = texture(texture_specular1, fs_in.TexCoords).rgb;

   vec3 ambient  = light.ambient * albedo;
   vec3 diffuse  = light.diffuse * diff * albedo;
   vec3 specular = light.specular * spec * specMap;

   ambient  *= attenuation;
   diffuse  *= attenuation;
   specular *= attenuation;

   return ambient + diffuse + specular;
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
    // Direction in tangent space
    vec3 lightDir = normalize(-light.direction);

    float diff = max(dot(normal, lightDir), 0.0);

    //vec3 halfwayDir = normalize(lightDir + viewDir);
    //float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    float spec = 0.0;
    if(blinn)
    {
        vec3 halfwayDir = normalize(lightDir + viewDir);  
        spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    }
    else
    {
        vec3 reflectDir = reflect(-lightDir, normal);
        spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
    }
    vec3 albedo   = texture(texture_diffuse1, fs_in.TexCoords).rgb;
    vec3 specMap  = texture(texture_specular1, fs_in.TexCoords).rgb;

    vec3 ambient  = light.ambient * albedo;
    vec3 diffuse  = light.diffuse * diff * albedo;
    vec3 specular = light.specular * spec * specMap;

    return ambient + diffuse + specular;
}

vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fs_in.FragPos);

    float diff = max(dot(normal, lightDir), 0.0);

    //vec3 halfwayDir = normalize(lightDir + viewDir);
    //float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    float spec = 0.0;
    if(blinn)
    {
        vec3 halfwayDir = normalize(lightDir + viewDir);  
        spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);
    }
    else
    {
        vec3 reflectDir = reflect(-lightDir, normal);
        spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
    }
    // Attenuation
    float distance = length(light.position - fs_in.TangentFragPos);
    float attenuation = 1.0 / (
        light.constant +
        light.linear * distance +
        light.quadratic * distance * distance
    );

    // Spotlight intensity
    float theta = dot(lightDir, normalize(-light.direction));
    float epsilon = light.cutOff - light.outerCutOff;
    float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);

    vec3 albedo   = texture(texture_diffuse1, fs_in.TexCoords).rgb;
    vec3 specMap  = texture(texture_specular1, fs_in.TexCoords).rgb;

    vec3 ambient  = light.ambient * albedo;
    vec3 diffuse  = light.diffuse * diff * albedo;
    vec3 specular = light.specular * spec * specMap;

    ambient  *= attenuation * intensity;
    diffuse  *= attenuation * intensity;
    specular *= attenuation * intensity;

    return ambient + diffuse + specular;
}

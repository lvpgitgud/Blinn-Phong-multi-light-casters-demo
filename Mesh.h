#ifndef MESH_H
#define MESH_H

#include <glad/glad.h>

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>

#include "shader.h"
#include <string>
#include <vector>

//////////////////////////////////////////////////////////////////////////
using namespace std;

#define	MAX_BONE_INFLUENCE 4
//////////////////////////////////////////////////////////////////////////

//--------------------------------------------------------------------------------------------------

// ============ VERTEX ============
struct Vertex {
	glm::vec3 Position;
	glm::vec3 Normal;
	glm::vec2 TexCoords;
	glm::vec3 Tangent;
	// bitangent
	glm::vec3 Bitangent;

	
	int m_BoneIDs[MAX_BONE_INFLUENCE];//bone indexes which will influence this vertex
	
	float m_Weights[MAX_BONE_INFLUENCE];//weights from each bone
};

// ============ TEXTURE ============
struct Texture {
	unsigned int id;
	string type;
	string path;
};

//--------------------------------------------------------------------------------------------------
class Mesh {
public:
	vector<Vertex> vertices;
	vector<unsigned int> indices;
	vector<Texture> textures;

	// ============ CONSTRUCTOR ============
	Mesh(vector<Vertex> vertices, vector<unsigned int> indices, vector<Texture> textures)
	{
		this->vertices = vertices;
		this->indices = indices;
		this->textures = textures;

		setupMesh();
	}

	// ============ DRAW MESH ============
	void Draw(Shader& shader) {
		unsigned int diffuseNr = 1;
		unsigned int specularNr = 1;
		unsigned int NormalNr = 1;
		unsigned int heightNr = 1;
		for (unsigned int i = 0; i < textures.size(); i++) {
			glActiveTexture(GL_TEXTURE0 + i);
			string number;
			string name = textures[i].type;
			if (name == "texture_diffuse")
				number = std::to_string(diffuseNr++);
			else if (name == "texture_specular")
				number = std::to_string(specularNr++);
			else if (name == "texture_normal")
				number = std::to_string(NormalNr++);
			else if (name == "texture_height")
				number = std::to_string(heightNr++);
			//set the sampler to the correct texture unit
			glUniform1i(glGetUniformLocation(shader.ID, (name + number).c_str()), i);

			glBindTexture(GL_TEXTURE_2D, textures[i].id);
		}
		//-----------------------------------------------------------------------------
		// Diffuse/specular/normal/height map is 
		// loaded as texture_[type][1-MAX_SAMPLER_NUMBER0
		// For instance:
		// Diffuse map: texture_diffuse1, texture_diffuse2, etc
		// Specular map: texture_specular1, texture_specular2, etc
		// Specular map: texture_normal1, texture_normal2, etc
		// this will show up as a uniform sampler2D texture in the shaders
		// see fragment_shader.fs for more details
		//-----------------------------------------------------------------------------

		glBindVertexArray(VAO);
		glDrawElements(GL_TRIANGLES, static_cast<unsigned int>(indices.size()), GL_UNSIGNED_INT, 0);
		glBindVertexArray(0);

		//reset the active tex unit to default
		glActiveTexture(GL_TEXTURE0);
	}
private:
	unsigned int VAO, VBO, EBO;

	// ============ MESH SETUP ============
	void setupMesh() {  
		glGenVertexArrays(1, &VAO);
		glGenBuffers(1, &VBO);
		glGenBuffers(1, &EBO);

		glBindVertexArray(VAO);

		glBindBuffer(GL_ARRAY_BUFFER, VBO);

		glBufferData(GL_ARRAY_BUFFER, vertices.size() * sizeof(Vertex), &vertices[0], GL_STATIC_DRAW);

		glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, EBO);
		glBufferData(GL_ELEMENT_ARRAY_BUFFER, indices.size() * sizeof(unsigned int),&indices[0],GL_STATIC_DRAW);

		glEnableVertexAttribArray(0);//positions
		glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)0);

		glEnableVertexAttribArray(1);//Normals
		glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, Normal));

		glEnableVertexAttribArray(2);//texture coordinates
		glVertexAttribPointer(2, 2, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, TexCoords));

		glEnableVertexAttribArray(3);//tangents
		glVertexAttribPointer(3, 3, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, Tangent));

		glEnableVertexAttribArray(4);//bitangents
		glVertexAttribPointer(4, 3, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, Bitangent));

		glEnableVertexAttribArray(5); //bone IDs
		glVertexAttribIPointer(5, 4, GL_INT, sizeof(Vertex), (void*)offsetof(Vertex, m_BoneIDs));

		glEnableVertexAttribArray(6); // bone weights
		glVertexAttribPointer(6, 4, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, m_Weights));

		glBindVertexArray(0);
	}
};

#endif
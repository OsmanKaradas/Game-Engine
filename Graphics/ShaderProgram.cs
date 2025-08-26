using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine.Graphics
{
    public class ShaderProgram
    {
        public int ID;
        public ShaderProgram(string vertexShaderFilePath, string fragmentShaderFilePath)
        {
            ID = CreateProgram();

            // vertex shader

            int vertexShader = CreateShader(ShaderType.VertexShader);

            // add the source code from "Default.vert" in the Shaders file
            ShaderSource(vertexShader, LoadShaderSource(vertexShaderFilePath));

            CompileShader(vertexShader);

            // fragment shader

            int fragmentShader = CreateShader(ShaderType.FragmentShader);

            // add the source code from "Default.frag" in the Shaders file
            ShaderSource(fragmentShader, LoadShaderSource(fragmentShaderFilePath));

            CompileShader(fragmentShader);

            AttachShader(ID, vertexShader);
            AttachShader(ID, fragmentShader);

            LinkProgram(ID);

            // Materials
            this.SetInt("material.diffuseTex", 0);
            this.SetInt("material.specularTex", 1);

            DeleteShader(vertexShader);
            DeleteShader(fragmentShader);
        }

        public void Unbind()
        {
            UseProgram(0);
        }
        public void Delete()
        {
            DeleteProgram(ID);
        }

        public static string LoadShaderSource(string fileName)
        {
            string shaderPath = Path.Combine(AppContext.BaseDirectory, "Shaders", fileName);

            try
            {
                return File.ReadAllText(shaderPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load shader source file: " + e.Message);
                return "";
            }
        }

        public void Render(Camera camera)
        {
            UseProgram(ID);

            Matrix4 view = camera.GetViewMatrix();
            Matrix4 projection = camera.GetProjectionMatrix();

            int viewLocation = GetUniformLocation(ID, "view");
            int projectionLocation = GetUniformLocation(ID, "projection");

            UniformMatrix4(viewLocation, false, ref view);
            UniformMatrix4(projectionLocation, false, ref projection); 
        }

        public void SetVector4(string name, Vector4 value)
        {
            int location = GetUniformLocation(ID, name);
            Uniform4(location, value);
        }

        public void SetVector3(string name, Vector3 value)
        {
            int location = GetUniformLocation(ID, name);
            Uniform3(location, value);
        }
        
        public void SetVector2(string name, Vector2 value)
        {
            int location = GetUniformLocation(ID, name);
            Uniform2(location, value);
        }

        public void SetFloat(string name, float value)
        {
            int location = GetUniformLocation(ID, name);
            Uniform1(location, value);
        }
        
        public void SetInt(string name, int value)
        {
            int location = GetUniformLocation(ID, name);
            Uniform1(location, value);
        }

        public void SetBool(string name, bool value)
        {
            int location = GetUniformLocation(ID, name);
            Uniform1(location, value ? 1 : 0);
        }
    }
}


using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine
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

            DeleteShader(vertexShader);
            DeleteShader(fragmentShader);
        }

        public void Bind()
        {
            UseProgram(ID);
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

        public void SetVector4(string name, Vector4 value)
        {
            int location = GetUniformLocation(ID, name);
            Uniform4(location, value);
        }
        public void SetBool(string name, bool value)
        {
            int location = GetUniformLocation(ID, name);
            Uniform1(location, value ? 1 : 0);
        }
    }
}


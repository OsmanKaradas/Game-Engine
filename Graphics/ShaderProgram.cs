using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine.Graphics
{
    public class ShaderProgram
    {
        public int ID;
        private static List<ShaderProgram> shaders = new List<ShaderProgram>();
        public ShaderProgram(string vertexShaderFilePath, string fragmentShaderFilePath, string? geometryShaderFilePath = null)
        {
            ID = CreateProgram();

            int vertexShader = CreateShader(ShaderType.VertexShader);
            ShaderSource(vertexShader, LoadShaderSource(vertexShaderFilePath));
            CompileShader(vertexShader);
            AttachShader(ID, vertexShader);

            int fragmentShader = CreateShader(ShaderType.FragmentShader);
            ShaderSource(fragmentShader, LoadShaderSource(fragmentShaderFilePath));
            CompileShader(fragmentShader);
            AttachShader(ID, fragmentShader);

            if (geometryShaderFilePath != null)
            {
                int geometryShader = CreateShader(ShaderType.GeometryShader);
                ShaderSource(geometryShader, LoadShaderSource(geometryShaderFilePath));
                CompileShader(geometryShader);
                AttachShader(ID, geometryShader);
            }

            LinkProgram(ID);

            DeleteShader(vertexShader);
            DeleteShader(fragmentShader);

            shaders.Add(this);
        }

        public void Unbind()
        {
            UseProgram(0);
        }
        public static void Delete()
        {
            foreach (ShaderProgram shader in shaders)
                DeleteProgram(shader.ID);
        }
        public static string LoadShaderSource(string filePath)
        {
            string shaderPath = Path.Combine(AppContext.BaseDirectory, "Shaders", filePath);

            try
            {
                return File.ReadAllText("Shaders/" + filePath);
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

        public void SetMatrix4(string name, Matrix4 value)
        {
            int location = GetUniformLocation(ID, name);
            UniformMatrix4(location, false, ref value);
        }
    }
}


using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;

namespace GameEngine.Graphics
{
    public class GeometryPass
    {
        public ShaderProgram shader;
        public FBO fbo;

        public int width, height;
        public GeometryPass(string shaderVert, string shaderFrag, int width, int height)
        {
            shader = new(shaderVert, shaderFrag);
            fbo = new(width, height);
            this.width = width; this.height = height;
        }

        public void Render(Camera camera)
        {
            Viewport(0, 0, width, height);
            fbo.Bind();
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            shader.Render(camera);
            GameObject.Render(shader);
            fbo.Unbind();
        }
    }
}


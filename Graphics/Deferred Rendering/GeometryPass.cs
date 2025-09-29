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
        public FBO msaaFBO;        // Multisample FBO
        public FBO fbo;    // Single-sample resolved FBO
        public int width, height;

        public GeometryPass(string shaderVert, string shaderFrag, int width, int height)
        {
            shader = new ShaderProgram(shaderVert, shaderFrag);
            this.width = width;
            this.height = height;

            msaaFBO = new FBO(width, height, 4);

            fbo = new FBO(width, height);
        }

        public void Render(Camera camera)
        {
            // --- 1. Render scene into MSAA FBO ---
            Viewport(0, 0, width, height);
            msaaFBO.Bind();
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 projection = camera.GetProjectionMatrix();
            Matrix4 view = camera.GetViewMatrix();

            UseProgram(shader.ID);
            shader.SetMatrix4("projection", projection);
            shader.SetMatrix4("view", view);

            GameObject.Render(shader);

            msaaFBO.Unbind();

            // --- 2. Resolve MSAA FBO into single-sample FBO ---
            BindFramebuffer(FramebufferTarget.ReadFramebuffer, msaaFBO.ID);
            BindFramebuffer(FramebufferTarget.DrawFramebuffer, fbo.ID);

            for (int i = 0; i < 4; i++) // Color attachments: Position, Normal, Albedo, Material
            {
                ReadBuffer(ReadBufferMode.ColorAttachment0 + i);
                DrawBuffer(DrawBufferMode.ColorAttachment0 + i);
                BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            }

            // Depth attachment
            BlitFramebuffer(0, 0, width, height, 0, 0, width, height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);

            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
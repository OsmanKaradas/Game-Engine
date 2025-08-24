using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine
{
    public class FBO
    {
        public int ID;
        public int gAlbedo, gNormal, gMaterial;
        public int gDepth;
        VAO vao = null!;
        VBO vbo = null!;
        public FBO(int width, int height)
        {
            ID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            // Albedo (RGB8)
            gAlbedo = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gAlbedo);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, width, height, 0,
                        PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                    TextureTarget.Texture2D, gAlbedo, 0);

            // Normal (RGB16F) - view-space normal
            gNormal = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, width, height, 0,
                        PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1,
                                    TextureTarget.Texture2D, gNormal, 0);

            // Material (RG16F): R = roughness, G = metalness (pack whatever you like)
            gMaterial = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gMaterial);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rg16f, width, height, 0,
                        PixelFormat.Rg, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2,
                                    TextureTarget.Texture2D, gMaterial, 0);

            // Depth (renderbuffer)
            gDepth = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, gDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, OpenTK.Graphics.OpenGL4.RenderbufferStorage.DepthComponent24, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                                    RenderbufferTarget.Renderbuffer, gDepth);

            // Set draw buffers
            DrawBuffersEnum[] attachments = {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2
            };
            GL.DrawBuffers(attachments.Length, attachments);

            // Check completeness
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception($"GBuffer incomplete: {status}");
        }

        public void Bind(VAO vao, VBO vbo, IBO ibo)
        {
            BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            float[] vertices = {
                // POS      // UV
               -1f, -1f,    0f, 0f,
                1f, -1f,    1f, 0f,
                1f,  1f,    1f, 1f,
               -1f,  1f,    0f, 1f,
            };

            uint[] indices = {
                0, 1, 2,
                0, 2, 3
            };

            vao.Bind();
            vbo.Bind();
            BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            ibo.Bind();
            BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            EnableVertexAttribArray(0);
            VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            EnableVertexAttribArray(1);
            VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            vao.Unbind();
        }

        public void Unbind()
        {
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Delete()
        {
            DeleteFramebuffer(ID);
        }
    }
}


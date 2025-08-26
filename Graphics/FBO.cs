using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine.Graphics
{
    public class FBO
    {
        public int ID;
        public int gPosition, gNormal, gMaterial;
        public int gDepth;
        public FBO(int width, int height)
        {
            ID = GenFramebuffer();
            BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            // POSITION
            gPosition = GenTexture();
            BindTexture(TextureTarget.Texture2D, gPosition);
            TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.Float, 0);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, gPosition, 0);

            // NORMAL
            gNormal = GenTexture();
            BindTexture(TextureTarget.Texture2D, gNormal);
            TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, gNormal, 0);

            // MATERIAL
            gMaterial = GenTexture();
            BindTexture(TextureTarget.Texture2D, gMaterial);
            TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, 0);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, gMaterial, 0);

            // DEPTH
            gDepth = GenRenderbuffer();
            BindRenderbuffer(RenderbufferTarget.Renderbuffer, gDepth);
            RenderbufferStorage(RenderbufferTarget.Renderbuffer, OpenTK.Graphics.OpenGL4.RenderbufferStorage.DepthComponent24, width, height);
            FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, gDepth);

            DrawBuffersEnum[] attachments = {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2
            };
            DrawBuffers(attachments.Length, attachments);

            // Check completeness
            var status = CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception($"GBuffer incomplete: {status}");
                    Bind();
            Unbind();
        }

        public void Bind()
        {
            BindFramebuffer(FramebufferTarget.Framebuffer, ID);
        }

        public void Unbind()
        {
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Delete()
        {
            DeleteTexture(gPosition);
            DeleteTexture(gNormal);
            DeleteTexture(gMaterial);
            DeleteRenderbuffer(gDepth);
            DeleteFramebuffer(ID);
        }
    }
}


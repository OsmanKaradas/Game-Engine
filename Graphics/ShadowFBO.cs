using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine.Graphics
{
    public class ShadowFBO
    {
        public int ID;
        public int depthMap, gDepth;
        public ShadowFBO()
        {
            ID = GenFramebuffer();
            BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            // POSITION
            depthMap = GenTexture();
            BindTexture(TextureTarget.Texture2D, depthMap);
            TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, 2048, 2048, 0, PixelFormat.DepthComponent, PixelType.Float, 0);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureWrapMode.Repeat);

            // DEPTH
            BindFramebuffer(FramebufferTarget.Framebuffer, depthMap);
            FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMap, 0);
            DrawBuffer(DrawBufferMode.None);
            ReadBuffer(ReadBufferMode.None);
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            
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
            DeleteTexture(depthMap);
            DeleteFramebuffer(ID);
        }
    }
}


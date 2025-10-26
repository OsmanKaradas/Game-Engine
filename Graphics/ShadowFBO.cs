using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;

namespace GameEngine.Graphics
{
    public class ShadowFBO
    {
        public int ID;
        public int depthMap = 0;
        public int depthCubeMap = 0;
        public int width = 2048;
        public int height = 2048;
        public ShadowFBO()
        {
            ID = GenFramebuffer();
            BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            depthMap = GenTexture();
            BindTexture(TextureTarget.Texture2D, depthMap);
            TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, 0);

            // PARAMETER
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            float[] borderColor = { 1f, 1f, 1f, 1f };
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            Bind();
            FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMap, 0);

            DrawBuffer(DrawBufferMode.None);
            ReadBuffer(ReadBufferMode.None);

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
    
    public class ShadowFBOCubeMap
    {
        public int ID;
        public int depthCubeMap = 0;
        public int width = 2048;
        public int height = 2048;
        public ShadowFBOCubeMap()
        {
            ID = GenFramebuffer();
            BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            depthCubeMap = GenTexture();
            BindTexture(TextureTarget.TextureCubeMap, depthCubeMap);
            for(int i = 0; i < 6; i++)
                TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, 0);

            // PARAMETER
            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToBorder);

            float[] borderColor = { 1f, 1f, 1f, 1f };
            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureBorderColor, borderColor);

            Bind();
            FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthCubeMap, 0);

            DrawBuffer(DrawBufferMode.None);
            ReadBuffer(ReadBufferMode.None);

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
            DeleteTexture(depthCubeMap);
            DeleteFramebuffer(ID);
        }
    }
}


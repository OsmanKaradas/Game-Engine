using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Common;

namespace GameEngine.Graphics
{
    public class ShadowCubeMapFBO
    {
        public int ID;
        public int depthCubeMap;
        public int width = 2048;
        public int height = 2048;
        public ShadowCubeMapFBO()
        {
            ID = GenFramebuffer();
            BindFramebuffer(FramebufferTarget.Framebuffer, ID);

            depthCubeMap = GenTexture();
            BindTexture(TextureTarget.TextureCubeMap, depthCubeMap);
            for (int i = 0; i < 6; i++)
            {
                TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, 0);          
            }

            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);                
            TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            
            Bind();
            FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthCubeMap, 0);
            DrawBuffer(DrawBufferMode.None);
            ReadBuffer(ReadBufferMode.None);
            Unbind();
            
            // Check completeness
            var status = CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception($"GBuffer incomplete: {status}");
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


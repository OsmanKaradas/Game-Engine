using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;
using StbImageSharp;

namespace GameEngine
{
    public class Texture
    {
        public int ID;

        public Texture(string filePath)
        {
            ID = GenTexture();
            ActiveTexture(TextureUnit.Texture0);
            BindTexture(TextureTarget.Texture2D, ID);

            // texture parameters
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            StbImage.stbi_set_flip_vertically_on_load(1);
            ImageResult dirtTexture = ImageResult.FromStream(File.OpenRead("Textures/" + filePath), ColorComponents.RedGreenBlueAlpha);

            TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, dirtTexture.Width, dirtTexture.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, dirtTexture.Data);

            Unbind();
        }

        public void Bind()
        {
            BindTexture(TextureTarget.Texture2D, ID);
        }

        public void Unbind()
        {
            BindTexture(TextureTarget.Texture2D, 0);
            Enable(EnableCap.DepthTest);
        }

        public void Delete()
        {
            DeleteTexture(ID);
        }
    }
}


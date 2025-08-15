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
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            StbImage.stbi_set_flip_vertically_on_load(0);
            ImageResult texture = ImageResult.FromStream(File.OpenRead("Textures/" + filePath), ColorComponents.RedGreenBlueAlpha);

            TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, texture.Width, texture.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, texture.Data);

            Unbind();
        }

        public void Bind()
        {
            BindTexture(TextureTarget.Texture2D, ID);
        }

        public void Unbind()
        {
            BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Delete()
        {
            DeleteTexture(ID);
        }
    }
}


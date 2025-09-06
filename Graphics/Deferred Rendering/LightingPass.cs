using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;
using GameEngine.World;

namespace GameEngine.Graphics
{
    public class LightingPass
    {
        public ShaderProgram shader;
        public Quad quad;

        public LightingPass(string shaderVert, string shaderFrag, Quad quad)
        {
            shader = new(shaderVert, shaderFrag);

            UseProgram(shader.ID);
            shader.SetInt("gPosition", 0);
            shader.SetInt("gNormal", 1);
            shader.SetInt("gMaterial", 2);
            shader.SetInt("gDepth", 3);
            shader.SetInt("depthMap", 4);

            this.quad = quad;
        }

        public void Render(FBO fbo, ShadowFBO shadowFBO, Matrix4 lightSpaceMatrix)
        {
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ActiveTexture(TextureUnit.Texture0);
            BindTexture(TextureTarget.Texture2D, fbo.gPosition);
            ActiveTexture(TextureUnit.Texture1);
            BindTexture(TextureTarget.Texture2D, fbo.gNormal);
            ActiveTexture(TextureUnit.Texture2);
            BindTexture(TextureTarget.Texture2D, fbo.gMaterial);
            ActiveTexture(TextureUnit.Texture3);
            BindTexture(TextureTarget.Texture2D, fbo.gDepth);
            ActiveTexture(TextureUnit.Texture4);
            BindTexture(TextureTarget.Texture2D, shadowFBO.depthMap);

            UseProgram(shader.ID);
            UniformMatrix4(GetUniformLocation(shader.ID, "lightSpaceMatrix"), false, ref lightSpaceMatrix);

            Light.RenderLights();

            quad.Render();
        }
    }
}


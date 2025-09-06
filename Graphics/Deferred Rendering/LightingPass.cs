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
            shader.SetInt("depthCubeMap", 5);

            this.quad = quad;
        }

        public void Render(GeometryPass geometryPass, ShadowPass shadowPass)
        {
            Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            ActiveTexture(TextureUnit.Texture0);
            BindTexture(TextureTarget.Texture2D, geometryPass.fbo.gPosition);
            ActiveTexture(TextureUnit.Texture1);
            BindTexture(TextureTarget.Texture2D, geometryPass.fbo.gNormal);
            ActiveTexture(TextureUnit.Texture2);
            BindTexture(TextureTarget.Texture2D, geometryPass.fbo.gMaterial);
            ActiveTexture(TextureUnit.Texture3);
            BindTexture(TextureTarget.Texture2D, geometryPass.fbo.gDepth);
            ActiveTexture(TextureUnit.Texture4);
            BindTexture(TextureTarget.Texture2D, shadowPass.fbo.depthMap);
            ActiveTexture(TextureUnit.Texture5);
            BindTexture(TextureTarget.TextureCubeMap, shadowPass.cubeMapFBO.depthCubeMap);

            UseProgram(shader.ID);
            shader.SetMatrix4("lightSpaceMatrix", shadowPass.lightSpaceMatrix);
            shader.SetFloat("nearPlane", shadowPass.nearPlane);
            shader.SetFloat("farPlane", shadowPass.farPlane);
            
            Light.RenderLights();

            quad.Render();
        }
    }
}


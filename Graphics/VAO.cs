using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine.Graphics
{
    public class VAO
    {
        public int ID;

        public VAO()
        {
            ID = GenVertexArray();
            BindVertexArray(ID);
        }

        public void LinkToVAO(VBO vbo)
        {
            Bind();
            vbo.Bind();
            Unbind();
        }

        public void Bind()
        {
            BindVertexArray(ID);
        }
        public void Unbind()
        {
            BindVertexArray(0);
        }
        public void Delete()
        {
            DeleteVertexArray(ID);
        }
    }
}


using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine
{
    public class VAO
    {
        public int ID;

        public VAO()
        {
            ID = GenVertexArray();
            BindVertexArray(ID);
        }

        public void LinkToVAO(int location, int size, VBO vbo)
        {
            Bind();
            vbo.Bind();
            VertexAttribPointer(location, size, VertexAttribPointerType.Float, false, 0, 0);
            EnableVertexAttribArray(location);
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


using System;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL4.GL;

namespace GameEngine
{
    public class IBO
    {
        public int ID;

        public IBO(List<uint> data)
        {
            ID = GenBuffer();
            Bind();
            BufferData(BufferTarget.ElementArrayBuffer, data.Count * sizeof(uint), data.ToArray(), BufferUsageHint.StaticDraw);
        }

        public void Bind()
        {
            BindBuffer(BufferTarget.ElementArrayBuffer, ID);
        }

        public void Unbind()
        {
            BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Delete()
        {
            DeleteBuffer(ID);
        }
    }
}


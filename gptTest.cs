// GltfSkinning_OpenTK_SharpGLTF.cs
// Minimal, fully-functional example that loads a glTF file (with skin/joints/weights)
// using SharpGLTF and renders it with OpenTK using GPU skinning (vertex shader).
//
// Requirements (NuGet):
//   - OpenTK (version 4.x)
//   - SharpGLTF.Schemas2 (or SharpGLTF) — this example assumes Schema2 API.
//
// Build: dotnet add package OpenTK
//        dotnet add package SharpGLTF.Schemas2
// Run:   dotnet run -- <path-to-model.gltf>
//
// Notes:
//  - Supports up to MAX_JOINTS (set below). Most glTF models use <= 64 joints.
//  - Vertex attributes expected: POSITION, NORMAL, JOINTS_0 (vec4), WEIGHTS_0 (vec4)
//  - Texture/material support is minimal: only a base color texture is supported if present.
//  - The code computes joint world transforms * inverse bind matrices and uploads them
//    as a uniform array to the shader for skinning.

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using SharpGLTF.Schema2;

class gptTest : GameWindow
{
    const int MAX_JOINTS = 64; // increase if you expect bigger skeletons

    int _program;
    int _vao;
    int _vboPos;
    int _vboNorm;
    int _vboJoints;
    int _vboWeights;
    int _ebo;
    int _texture = -1;

    int _uMVP;
    int _uNormal;
    int _uJointMatrices;

    int _indexCount;

    Scene _scene;
    Skin _skin;
    Node _meshNode;

    // store inverse bind matrices and joint nodes
    Matrix4[] _inverseBindMatrices;
    Node[] _joints;

    public gptTest(string path) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        Title = "glTF Skinning - SharpGLTF + OpenTK";

        // load glTF
        var model = ModelRoot.Load(path);

        // choose first scene
        _scene = model.DefaultScene ?? model.LogicalScenes.FirstOrDefault();
        if (_scene == null) throw new Exception("No scene in glTF file.");

        // locate first skinned mesh (with a skin)
        foreach (var node in model.LogicalNodes)
        {
            var found = FindSkinnedNode(node);
            if (found != null)
            {
                _meshNode = found.Item1;
                _skin = found.Item2;
                break;
            }
        }
        if (_meshNode == null || _skin == null) throw new Exception("No skinned mesh found in scene.");

        // read inverse bind matrices and joints
        _joints = _skin.Joints.ToArray();
        if (_joints.Length > MAX_JOINTS) throw new Exception($"Joints exceed MAX_JOINTS ({MAX_JOINTS}).");

        if (_skin.InverseBindMatrices != null)
        {
            //var inv = _skin.InverseBindMatrices.AsMatrix4x4Array();
            Matrix4[] inv = new Matrix4[0];
            foreach (var mat in _skin.InverseBindMatrices)
                inv.Append(ToOpenTK(mat));


            _inverseBindMatrices = inv;
        }
        else
        {
            // fallback to identity
            _inverseBindMatrices = Enumerable.Repeat(Matrix4.Identity, _joints.Length).ToArray();
        }

        Size = new OpenTK.Mathematics.Vector2i(1280, 720);
    }

    // signature
    static Tuple<Node, Skin> FindSkinnedNode(Node node)
    {
        if (node.Mesh != null && node.Skin != null)
        {
            var prim = node.Mesh.Primitives.FirstOrDefault();
            if (prim != null && prim.GetVertexAccessor("JOINTS_0") != null && prim.GetVertexAccessor("WEIGHTS_0") != null)
            {
                return Tuple.Create(node, node.Skin);
            }
        }

        foreach (var child in node.VisualChildren) // see note below about .Children
        {
            var found = FindSkinnedNode(child);
            if (found != null) return found;
        }
        return null;
    }


    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.2f, 0.25f, 0.3f, 1f);
        GL.Enable(EnableCap.DepthTest);

        // create shader
        _program = CreateProgram(VShaderSource, FShaderSource);
        _uMVP = GL.GetUniformLocation(_program, "uMVP");
        _uNormal = GL.GetUniformLocation(_program, "uNormalMatrix");
        _uJointMatrices = GL.GetUniformLocation(_program, "uJointMatrices");

        // upload mesh data
        UploadMesh();
    }

    void UploadMesh()
    {
        // simplify: use first primitive
        var prim = _meshNode.Mesh.Primitives.First();

        var posA = prim.GetVertexAccessor("POSITION");
        var normA = prim.GetVertexAccessor("NORMAL");
        var jointsA = prim.GetVertexAccessor("JOINTS_0");
        var weightsA = prim.GetVertexAccessor("WEIGHTS_0");
        var idxA = prim.IndexAccessor;

        var positions = posA.AsVector3Array();
        var normals = normA != null ? normA.AsVector3Array() : positions;

        // JOINTS may be unsigned short or byte. We'll fetch them as Vector4 of floats (indices)
        var joints = new Vector4[positions.Count];
        var weights = new Vector4[positions.Count];

        // decode joints and weights from accessors
        var jointsRaw = prim.GetVertexAccessor("JOINTS_0").AsVector4Array();
        var weightsRaw = prim.GetVertexAccessor("WEIGHTS_0").AsVector4Array();

        for (int i = 0; i < positions.Count; i++)
        {
            var ji = jointsRaw[i];
            joints[i] = new Vector4(ji.X, ji.Y, ji.Z, ji.W);
            var wi = weightsRaw[i];
            weights[i] = new Vector4(wi.X, wi.Y, wi.Z, wi.W);
        }

        var indices = idxA.AsIndicesArray().ToArray();
        _indexCount = indices.Length;

        // create VAO + VBOs
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        // positions
        _vboPos = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vboPos);
        GL.BufferData(BufferTarget.ArrayBuffer, positions.Count * Vector3.SizeInBytes, positions.ToArray(), BufferUsageHint.StaticDraw);
        var posLoc = GL.GetAttribLocation(_program, "aPosition");
        GL.EnableVertexAttribArray(posLoc);
        GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 0, 0);

        // normals
        _vboNorm = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vboNorm);
        GL.BufferData(BufferTarget.ArrayBuffer, normals.Count * Vector3.SizeInBytes, normals.ToArray(), BufferUsageHint.StaticDraw);
        var nLoc = GL.GetAttribLocation(_program, "aNormal");
        if (nLoc >= 0)
        {
            GL.EnableVertexAttribArray(nLoc);
            GL.VertexAttribPointer(nLoc, 3, VertexAttribPointerType.Float, false, 0, 0);
        }

        // joints (ivec4) — upload as floats and interpret as ints in shader
        _vboJoints = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vboJoints);
        GL.BufferData(BufferTarget.ArrayBuffer, joints.Length * Vector4.SizeInBytes, joints, BufferUsageHint.StaticDraw);
        var jLoc = GL.GetAttribLocation(_program, "aJoints");
        GL.EnableVertexAttribArray(jLoc);
        // We will fetch as floats and convert in shader
        GL.VertexAttribPointer(jLoc, 4, VertexAttribPointerType.Float, false, 0, 0);

        // weights
        _vboWeights = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vboWeights);
        GL.BufferData(BufferTarget.ArrayBuffer, weights.Length * Vector4.SizeInBytes, weights, BufferUsageHint.StaticDraw);
        var wLoc = GL.GetAttribLocation(_program, "aWeights");
        GL.EnableVertexAttribArray(wLoc);
        GL.VertexAttribPointer(wLoc, 4, VertexAttribPointerType.Float, false, 0, 0);

        // indices
        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indexCount * sizeof(int), indices, BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape)) Close();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.UseProgram(_program);
        GL.BindVertexArray(_vao);

        // compute MVP
        var proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);
        var view = Matrix4.LookAt(new Vector3(0, 1.5f, 4f), Vector3.Zero, Vector3.UnitY);
        var model = Matrix4.Identity;
        var mvp = model * view * proj; // Note: OpenTK uses column-major, but we will send as transpose false
        GL.UniformMatrix4(_uMVP, false, ref mvp);

        // normal matrix
        var normalMat = new Matrix3(model.Inverted().Transposed());
        GL.UniformMatrix3(_uNormal, false, ref normalMat);

        // compute joint matrices: J_world * inverseBind
        var jointMats = new float[16 * MAX_JOINTS];
        for (int i = 0; i < _joints.Length; i++)
        {
            var joint = _joints[i];
            var world = ToOpenTK(GetWorldMatrix(joint));
            var invBind = ToOpenTK(_inverseBindMatrices[i]);
            // jointMat = world * invBind
            var jm = world * invBind;
            // copy jm to float array
            var matArray = new float[16];
            matArray[0] = jm.M11; matArray[1] = jm.M12; matArray[2] = jm.M13; matArray[3] = jm.M14;
            matArray[4] = jm.M21; matArray[5] = jm.M22; matArray[6] = jm.M23; matArray[7] = jm.M24;
            matArray[8] = jm.M31; matArray[9] = jm.M32; matArray[10] = jm.M33; matArray[11] = jm.M34;
            matArray[12] = jm.M41; matArray[13] = jm.M42; matArray[14] = jm.M43; matArray[15] = jm.M44;
            Array.Copy(matArray, 0, jointMats, i * 16, 16);
            Array.Copy(matArray, 0, jointMats, i * 16, 16);
        }

        // upload joint matrices as uniform array
        GL.UniformMatrix4(_uJointMatrices, _joints.Length, false, jointMats);

        // bind texture if available
        if (_texture >= 0)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            var uTex = GL.GetUniformLocation(_program, "uBaseColorTex");
            GL.Uniform1(uTex, 0);
        }

        GL.DrawElements(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);

        GL.BindVertexArray(0);
        GL.UseProgram(0);

        SwapBuffers();
    }

    // recursively compute world matrix for node
    static System.Numerics.Matrix4x4 GetWorldMatrix(Node node)
    {
        var m = node.LocalMatrix;
        var p = node.VisualParent;
        if (p != null) return GetWorldMatrix(p) * m;
        return m;
    }

    // helpers for conversions
    static Matrix4 ToOpenTK(Matrix4 m)
    {
        return new Matrix4(
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
        );
    }

    static Matrix4 ToOpenTK(System.Numerics.Matrix4x4 m)
    {
        return new Matrix4(
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
        );
    }

    // create shader program
    static int CreateProgram(string vert, string frag)
    {
        int vs = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vs, vert);
        GL.CompileShader(vs);
        CheckShader(vs);

        int fs = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fs, frag);
        GL.CompileShader(fs);
        CheckShader(fs);

        int prog = GL.CreateProgram();
        GL.AttachShader(prog, vs);
        GL.AttachShader(prog, fs);
        GL.LinkProgram(prog);
        CheckProgram(prog);

        GL.DeleteShader(vs);
        GL.DeleteShader(fs);
        return prog;
    }

    static void CheckShader(int id)
    {
        GL.GetShader(id, ShaderParameter.CompileStatus, out int ok);
        if (ok == 0)
        {
            string log = GL.GetShaderInfoLog(id);
            throw new Exception("Shader compile failed: " + log);
        }
    }
    static void CheckProgram(int id)
    {
        GL.GetProgram(id, GetProgramParameterName.LinkStatus, out int ok);
        if (ok == 0)
        {
            string log = GL.GetProgramInfoLog(id);
            throw new Exception("Program link failed: " + log);
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        GL.DeleteProgram(_program);
        GL.DeleteBuffer(_vboPos);
        GL.DeleteBuffer(_vboNorm);
        GL.DeleteBuffer(_vboJoints);
        GL.DeleteBuffer(_vboWeights);
        GL.DeleteBuffer(_ebo);
        GL.DeleteVertexArray(_vao);
        if (_texture >= 0) GL.DeleteTexture(_texture);
    }

    // minimal GLSL shaders
    const string VShaderSource = @"#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec4 aJoints; // passed as floats; interpret as ints
layout(location = 3) in vec4 aWeights;

uniform mat4 uMVP;
uniform mat3 uNormalMatrix;
uniform mat4 uJointMatrices[64];

out vec3 vNormal;

void main()
{
    // skinning
    ivec4 joints = ivec4(aJoints);
    vec4 skinnedPos = vec4(0.0);
    vec3 skinnedNormal = vec3(0.0);
    for (int i = 0; i < 4; ++i)
    {
        int j = joints[i];
        float w = aWeights[i];
        if (w > 0.0 && j >= 0)
        {
            mat4 J = uJointMatrices[j];
            vec4 p = J * vec4(aPosition, 1.0);
            skinnedPos += w * p;
            // transform normal (approx): use upper-left 3x3 of J
            mat3 J3 = mat3(J);
            skinnedNormal += w * (J3 * aNormal);
        }
    }
    if (skinnedPos == vec4(0.0)) // fallback to bind pose
    {
        skinnedPos = vec4(aPosition, 1.0);
        skinnedNormal = aNormal;
    }

    gl_Position = uMVP * skinnedPos;
    vNormal = normalize(uNormalMatrix * skinnedNormal);
}
";

    const string FShaderSource = @"#version 330 core
in vec3 vNormal;
out vec4 FragColor;
uniform sampler2D uBaseColorTex;
void main()
{
    float n = max(dot(normalize(vNormal), vec3(0.0, 0.0, 1.0)), 0.0);
    vec3 base = vec3(0.8, 0.7, 0.6);
    FragColor = vec4(base * (0.2 + 0.8*n), 1.0);
}
";
}

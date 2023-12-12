// using Silk.NET.Maths;

// namespace DanmakuEngine.Graphics;

// public class Sprite : Drawable
// {
//     protected Texture texture;

//     public Vector2D<float> Position { get; set; }

//     public Vector2D<float> Size { get; set; }

//     /// <summary>
//     /// Rotation in radians.
//     /// 0 for rgiht, pi/2 for up, pi for left, 3pi/2 for down
//     /// </summary>
//     public float Rotation
//     {
//         get
//         {
//             return _rotation;
//         }
//         set
//         {
//             _rotation = value % MathF.Tau;
//         }
//     }

//     private float _rotation;

//     public Sprite(Texture texture) 
//     {
//         this.texture = texture;
//     }

//     public virtual void Blit(GL _gl)
//     {
//         // TODO: Implement
//         // // 设置模型矩阵
//         // Matrix4x4 model = Matrix4x4.CreateScale(Size.X, Size.Y, 1.0f) *
//         //                 Matrix4x4.CreateRotationZ(Rotation) *
//         //                 Matrix4x4.CreateTranslation(Position.X, Position.Y, 0.0f);

//         // // 将模型矩阵传递给着色器
//         // int modelLocation = _gl.GetUniformLocation(Program, "model");
//         // _gl.UniformMatrix4(modelLocation, 1, false, ref model);

//         // // 绑定纹理并绘制
//         // _gl.BindTexture(TextureTarget.Texture2D, texture.Id);
//         // _gl.DrawArrays(PrimitiveType.Quads, 0, 4);
//     }
// }

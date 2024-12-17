using System;

namespace GameData
{
    public struct FRect : IStruct
    {
        public static readonly FRect SEmpty = new FRect();

        public float Left;

        // Left point of rectangle
        public float Top;

        // Top point of rectangle
        public float Right;

        // Right point of rectangle
        public float Bottom;

        // Bottom point of rectangle
        public FRect(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public FRect(double left, double top, double right, double bottom)
        {
            Left = (float)left;
            Top = (float)top;
            Right = (float)right;
            Bottom = (float)bottom;
        }

        public int StructSize => 4*sizeof(float);
        public int StructAlign => 4;
        public string StructMember => "frect_t";

        public string[] StructCode()
        {
            const string code = """
                                  struct frect_t
                                  {
                                      explicit frect_t(f32 left = 0.0f, f32 top = 0.0f, f32 right = 0.0f, f32 bottom = 0.0f)
                                          : m_left(left), m_top(top), m_right(right), m_bottom(bottom)
                                      {
                                      }

                                      inline f32 left() const { return m_left; }
                                      inline f32 top() const { return m_top; }
                                      inline f32 right() const { return m_right; }
                                      inline f32 bottom() const { return m_bottom; }

                                  private:
                                      f32 m_left;
                                      f32 m_top;
                                      f32 m_right;
                                      f32 m_bottom;
                                  };

                                """;
            return code.Split("\n");
        }

        public void StructWrite(IGameDataWriter writer)
        {
            writer.Write(Left);
            writer.Write(Top);
            writer.Write(Right);
            writer.Write(Bottom);
        }
    }
}

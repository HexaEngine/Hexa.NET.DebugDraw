namespace Hexa.NET.DebugDraw
{
    using System.Numerics;

    public class DebugDrawStyle
    {
        private readonly Vector4[] colors = new Vector4[(int)DebugDrawCol.Count];
        private readonly Stack<(DebugDrawCol col, Vector4 color)> colorStack = [];
        private readonly Stack<(DebugDrawStyleVar styleVar, StyleVarValue value)> styleVarStack = [];

        public DebugDrawStyle()
        {
            colors[(int)DebugDrawCol.Grid] = new(1, 1, 1, 0.8f);
            colors[(int)DebugDrawCol.GridAxisX] = new(1, 0, 0, 0.8f);
            colors[(int)DebugDrawCol.GridAxisY] = new(0, 1, 0, 0.8f);
            colors[(int)DebugDrawCol.GridAxisZ] = new(0, 0, 1, 0.8f);
        }

        public Vector4[] Colors => colors;

        public float GridSize = 100;
        public float GridSpacing = 1;
        public float GridAxisSize = 1;

        public float GetStyleVar(DebugDrawStyleVar var)
        {
            return var switch
            {
                DebugDrawStyleVar.GridSize => GridSize,
                DebugDrawStyleVar.GridSpacing => GridSpacing,
                DebugDrawStyleVar.GridAxisSize => GridAxisSize,
                _ => throw new ArgumentOutOfRangeException(nameof(var))
            };
        }

        public uint GetColorU32(DebugDrawCol col)
        {
            return DebugDraw.ColorConvertFloat4ToU32(colors[(int)col]);
        }

        public void PushStyleColor(DebugDrawCol col, Vector4 color)
        {
            colorStack.Push((col, colors[(int)col]));
            colors[(int)col] = color;
        }

        public void PopStyleColor()
        {
            if (colorStack.Count > 0)
            {
                var (col, color) = colorStack.Pop();
                colors[(int)col] = color;
            }
        }

        private void SetAndPush<T>(DebugDrawStyleVar var, ref T target, T value)
        {
            var previousValue = StyleVarValue.From(target);
            target = value;
            styleVarStack.Push((var, previousValue));
        }

        public void PushStyleVar(DebugDrawStyleVar var, float value)
        {
            switch (var)
            {
                case DebugDrawStyleVar.GridSize:
                    SetAndPush(var, ref GridSize, value);
                    break;

                case DebugDrawStyleVar.GridSpacing:
                    SetAndPush(var, ref GridSpacing, value);
                    break;

                case DebugDrawStyleVar.GridAxisSize:
                    SetAndPush(var, ref GridAxisSize, value);
                    break;

                // Handle other style variables here
                default:
                    throw new ArgumentOutOfRangeException(nameof(var));
            }
        }

        public void PopStyleVar()
        {
            if (styleVarStack.Count > 0)
            {
                var (var, previousValue) = styleVarStack.Pop();
                switch (var)
                {
                    case DebugDrawStyleVar.GridSize:
                        GridSize = previousValue.FloatValue;
                        break;

                    case DebugDrawStyleVar.GridSpacing:
                        GridSpacing = previousValue.FloatValue;
                        break;

                    case DebugDrawStyleVar.GridAxisSize:
                        GridAxisSize = previousValue.FloatValue;
                        break;
                    // Handle other style variables here
                    default:
                        throw new ArgumentOutOfRangeException(nameof(var));
                }
            }
        }
    }
}
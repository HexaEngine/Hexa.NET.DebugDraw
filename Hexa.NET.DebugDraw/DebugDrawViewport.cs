namespace Hexa.NET.DebugDraw
{
    using System.Numerics;

    public struct DebugDrawViewport
    {
        /// <summary>
        /// Position of the pixel coordinate of the upper-left corner of the viewport.
        /// </summary>
        public float X;

        /// <summary>
        /// Position of the pixel coordinate of the upper-left corner of the viewport.
        /// </summary>
        public float Y;

        /// <summary>
        /// Width dimension of the viewport.
        /// </summary>
        public float Width;

        /// <summary>
        /// Height dimension of the viewport.
        /// </summary>
        public float Height;

        /// <summary>
        /// Gets a <see cref="Vector2"/> representing the offset of the viewport.
        /// </summary>
        public readonly Vector2 Offset => new(X, Y);

        /// <summary>
        /// Gets a <see cref="Vector2"/> representing the size of the viewport.
        /// </summary>
        public readonly Vector2 Size => new(Width, Height);

        public DebugDrawViewport(Vector2 offset, Vector2 size)
        {
            X = offset.X;
            Y = offset.Y;
            Width = size.X;
            Height = size.Y;
        }
    }
}
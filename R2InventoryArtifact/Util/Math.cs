

using System;

namespace R2InventoryArtifact.Util.Math
{
    [Serializable] public struct IntRect 
    {
        public int Width, Height; 

        public IntRect(int width, int height)
        {
            Width = width; 
            Height = height; 
        }

        public bool Contains(GridPosition pos)
        {
            return Contains(pos.Row, pos.Col); 
        }

        public bool Contains(int row, int col)
        {
            return col >= 0 && col < Width && row >= 0 && row < Height; 
        }
    }

    [Serializable] public struct GridPosition
    {
        public int Row, Col; 

        public GridPosition(int Col, int Row)
        {
            this.Row = Row; 
            this.Col = Col; 
        }

        public override string ToString()
        {
            return $"(Row:{Row},Col:{Col})";
        }

        public static GridPosition operator +(GridPosition a, GridPosition b)
        {
            return new(a.Col + b.Col, a.Row + b.Row); 
        }

        public static GridPosition operator -(GridPosition a, GridPosition b)
        {
            return new(a.Col - b.Col, a.Row - b.Row); 
        }

        public static bool operator ==(GridPosition a, GridPosition b)
        {
            return a.Col == b.Col && a.Row == b.Row; 
        }

        public static bool operator !=(GridPosition a, GridPosition b)
        {
            return !(a.Col == b.Col && a.Row == b.Row);
        }

        public override bool Equals(object obj)
        {
            return this == (GridPosition) obj; 
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Col, Row); 
        }
    }
}
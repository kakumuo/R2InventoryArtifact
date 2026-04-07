

using System;
using UnityEngine;

namespace R2InventoryArtifact.Util
{
    public static class R2InventoryArtifactUtil
    {
        public static Sprite LoadSprite(string path)
        {
            if (System.IO.File.Exists(path))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                Texture2D tex = new(256, 256, TextureFormat.ARGB32, false, false);
                tex.LoadImage(bytes);
                tex.filterMode = FilterMode.Point;
                return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 1, SpriteMeshType.Tight, Vector4.zero, true);
            }
            return null;
        }
    }

    [Serializable]
    public struct IntRect
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

    [Serializable]
    public struct GridPosition
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
            return this == (GridPosition)obj;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Col, Row);
        }
    }
}
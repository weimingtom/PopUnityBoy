﻿namespace GarboDev
{
    using System;

    public interface IRenderer
    {
        void Initialize(object data);
        void Reset();
        void RenderLine(int line);
        void ShowFrame();
    }
}

using System;
using System.Collections.Generic;
using StickerPool.Data;
using StickerPool.View;
using UnityEngine;

namespace StickerPool.Controller
{
    /// <summary>Reuses sticker prefab instances so replaying a level creates no new objects.</summary>
    public sealed class StickerObjectPool
    {
        readonly Transform parent;
        readonly Action<StickerView> onCreated;
        readonly Dictionary<string, Stack<StickerView>> free = new Dictionary<string, Stack<StickerView>>();
        readonly List<StickerView> active = new List<StickerView>();

        public StickerObjectPool(Transform parent, Action<StickerView> onCreated)
        {
            this.parent = parent;
            this.onCreated = onCreated;
        }

        public StickerView Get(StickerData data)
        {
            if (!free.TryGetValue(data.Id, out var stack))
            {
                stack = new Stack<StickerView>();
                free[data.Id] = stack;
            }

            StickerView view;
            if (stack.Count > 0)
            {
                view = stack.Pop();
            }
            else
            {
                var instance = UnityEngine.Object.Instantiate(data.Prefab, parent);
                instance.name = data.Prefab.name;
                view = instance.GetComponent<StickerView>();
                onCreated?.Invoke(view);
            }

            active.Add(view);
            return view;
        }

        public void Release(StickerView view)
        {
            if (!active.Remove(view)) return;
            view.Deactivate();
            free[view.Data.Id].Push(view);
        }

        public void ReleaseAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var view = active[i];
                view.Deactivate();
                free[view.Data.Id].Push(view);
            }

            active.Clear();
        }
    }
}

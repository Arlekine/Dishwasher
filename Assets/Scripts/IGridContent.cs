using System;
using UnityEngine;

public interface IGridContent
{
    Action<IGridContent> onContentRemoved { get; set; }

    bool IsCompatibleWithDishwasher { get; }

    int Width { get; }
    int Height { get; }

    void SetPosition(Vector3 position);
    
    void SetParent(Transform newParent);
    
    void SetDefaultParent();
}
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))][SelectionBase]
public class Dish : MonoBehaviour, IGridContent
{
    public Action<IGridContent> onContentRemoved { get; set; }

    public bool IsCompatibleWithDishwasher => _isCompatibleWithDishwasher;
    public int Width => _gridWidth;
    public int Height => _gridHeight;
    
    [SerializeField] private int _gridWidth;
    [SerializeField] private int _gridHeight;
    [SerializeField] private bool _isCompatibleWithDishwasher = true;

    [SerializeField] private Vector3 _dishwaherRotation;
    [SerializeField] private float _rotationTime;
    [SerializeField] private float _returningTime;
    
    [Range(0, 10)]
    [SerializeField] private float _maxFreeSpeed = 3f;

    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private Color _incorrectColor = Color.red;

    private List<Color> _initialColors = new List<Color>();
    private Rigidbody _rigidbody;
    private Sequence _currentMoveSeq;
    private Transform _defaultParent;

    private void Start()
    {
        _defaultParent = transform.parent;
        _rigidbody = GetComponent<Rigidbody>();
        foreach (var material in _renderer.materials)
        {
            _initialColors.Add(material.color);
        }
    }

    public void Free()
    {
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        

        var maxPossibleSpeed = _rigidbody.velocity.normalized * _maxFreeSpeed;
        _rigidbody.velocity = _rigidbody.velocity.magnitude > maxPossibleSpeed.magnitude ? maxPossibleSpeed : _rigidbody.velocity;
    }

    public void Select()
    {
        onContentRemoved?.Invoke(this);

        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        
        _currentMoveSeq?.Kill();
        _currentMoveSeq = DOTween.Sequence();

        _currentMoveSeq.Append(transform.DOLocalRotate(_dishwaherRotation, _rotationTime));
    }

    public void SetIncorrect(bool isIncorrect)
    {
        for (int i = 0; i < _renderer.materials.Length; i++)
        {
            _renderer.materials[i].color = isIncorrect ? _incorrectColor : _initialColors[i];
        }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetParent(Transform newParent)
    {
        transform.parent = newParent;
    }

    public void SetDefaultParent()
    {
        transform.parent = _defaultParent;
    }
}

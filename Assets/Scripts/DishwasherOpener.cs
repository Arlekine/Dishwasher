using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DishwasherOpener : MonoBehaviour
{
    [SerializeField] private Transform _cap;
    [SerializeField] private Transform _topGrid;
    [SerializeField] private Transform _downGrid;

    [Space]
    [SerializeField] private Vector3 _openedCapRotation;
    [SerializeField] private Vector3 _closedCapRotation;
    [SerializeField] private float _capTweenTime;
    
    [Space]
    [SerializeField] private Vector3 _topGridOpenedPosition;
    [SerializeField] private Vector3 _topGridClosedPosition; 
    [SerializeField] private float _topGridTweenTime;
    
    [Space]
    [SerializeField] private Vector3 _downGridOpenedPosition;
    [SerializeField] private Vector3 _downGridClosedPosition;
    [SerializeField] private float _downGridTweenTime;

    private Sequence _currentSequence;
    
    [EditorButton]
    public Tween Open()
    {
        _currentSequence?.Kill();
        _currentSequence = DOTween.Sequence();

        _currentSequence.Append(_cap.DOLocalRotate(_openedCapRotation, _capTweenTime));

        _currentSequence.Append(_topGrid.DOLocalMove(_topGridOpenedPosition, _topGridTweenTime).SetEase(Ease.OutBounce));
        _currentSequence.Join(_downGrid.DOLocalMove(_downGridOpenedPosition, _downGridTweenTime).SetEase(Ease.OutBounce));

        return _currentSequence;
    }

    [EditorButton]
    public Tween Close()
    {
        _currentSequence?.Kill();
        _currentSequence = DOTween.Sequence();

        _currentSequence.Append(_topGrid.DOLocalMove(_topGridClosedPosition, _topGridTweenTime).SetEase(Ease.OutBounce));
        _currentSequence.Join(_downGrid.DOLocalMove(_downGridClosedPosition, _downGridTweenTime).SetEase(Ease.OutBounce));
        
        _currentSequence.Append(_cap.DOLocalRotate(_closedCapRotation, _capTweenTime));
        
        return _currentSequence;
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lean.Touch;
using UnityEngine;

public class DishSelector : MonoBehaviour
{
    [SerializeField] private DishwasherGrid _dishwasherGrid;
    [SerializeField] private LayerMask _dishLayer;
    [SerializeField] private  LayerMask _floorLayer;
    [SerializeField] private  LayerMask _gridLayer;
    [SerializeField] private  float _floorHeight;
    [SerializeField] private  Camera _raycastCamera;

    [SerializeField] private Transform _dishMoveTopBorder;
    [SerializeField] private Transform _dishMoveDownBorder;

    private Dish _currentDish;
    
    
    private Vector2 _xMoveBorder;
    private Vector2 _zMoveBorder;

    private void Awake()
    {
        LeanTouch.OnFingerDown += TakeDish;
        LeanTouch.OnFingerUp += DropDish;
        
        _xMoveBorder = new Vector2(_dishMoveDownBorder.position.x,  _dishMoveTopBorder.position.x);
        _zMoveBorder = new Vector2(_dishMoveTopBorder.position.z,  _dishMoveDownBorder.position.z);
    }

    private void OnDestroy()
    {
        LeanTouch.OnFingerDown -= TakeDish;
        LeanTouch.OnFingerUp -= DropDish;
    }

    private void Update()
    {
        if (_currentDish != null)
        {
            var pos = _currentDish.transform.position;
            var fingers = LeanTouch.GetFingers(false, false);
            
            RaycastHit hit;
            
            if (fingers.Count != 0)
            {
                Ray positionRay = _raycastCamera.ScreenPointToRay(fingers[0].ScreenPosition);

                if (Physics.Raycast(positionRay, out hit, 1000f, layerMask: _floorLayer))
                {
                    var cameraFloorDistance = hit.distance;
                    var cameraHeight = _raycastCamera.transform.position.y;
                    var cameraToBodyDistance =
                        cameraFloorDistance - (cameraFloorDistance * _floorHeight) / cameraHeight;

                    pos = _raycastCamera.ScreenToWorldPoint(new Vector3(fingers[0].ScreenPosition.x,
                        fingers[0].ScreenPosition.y, cameraToBodyDistance));
                }
                
                Vector3 gridPoint;
                var gridRay = _raycastCamera.ScreenPointToRay(fingers[0].ScreenPosition);

                if (Physics.Raycast(gridRay, out hit, 1000f, layerMask: _gridLayer))
                {
                    gridPoint = hit.point;
                    var hoverPosWithPlacing = _dishwasherGrid.GetContentHoverPosition(gridPoint, _currentDish);
                    _currentDish.SetIncorrect(!hoverPosWithPlacing.isPlaceble);
                    
                    pos = hoverPosWithPlacing.hoverPos;
                }
            }
            
            pos.x = Mathf.Clamp(pos.x, _xMoveBorder.x, _xMoveBorder.y);
            pos.z = Mathf.Clamp(pos.z, _zMoveBorder.x, _zMoveBorder.y);

            _currentDish.transform.position = pos;
        }
    }

    private void TakeDish(LeanFinger finger)
    {
        RaycastHit hit;
        Ray ray = _raycastCamera.ScreenPointToRay(finger.ScreenPosition);

        if (Physics.Raycast(ray, out hit, 1000f, layerMask: _dishLayer))
        {
            var dish = hit.collider.GetComponent<Dish>();
            if (dish != null)
            {
                _currentDish = dish;
                _currentDish.Select();
            }
        }
    }

    private void DropDish(LeanFinger finger)
    {
        if (_currentDish != null)
        {
            RaycastHit hit;
            Ray ray = _raycastCamera.ScreenPointToRay(finger.ScreenPosition);

            if (Physics.Raycast(ray, out hit, 1000f, layerMask: _gridLayer))
            {
                bool isPlaced = _dishwasherGrid.PlaceContent(hit.point, _currentDish);

                if (isPlaced)
                {
                    _currentDish.SetIncorrect(false);
                    _currentDish = null;
                    return;
                }
            }
            
            _currentDish.Free();
            _currentDish.SetIncorrect(false);
            _currentDish = null;
        }
    }

    
}
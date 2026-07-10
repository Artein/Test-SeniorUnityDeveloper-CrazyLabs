using System.Collections.Generic;
using Game.Foundation.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Gameplay
{
    internal interface IRunSteeringPointerPressGuard
    {
        bool CanBeginRunSteering(PointerInput pointerInput);
    }

    internal sealed class UnityEventSystemRunSteeringPointerPressGuard : IRunSteeringPointerPressGuard
    {
        private readonly List<RaycastResult> _raycastResults = new();

        public bool CanBeginRunSteering(PointerInput pointerInput)
        {
            var eventSystem = EventSystem.current;

            if (eventSystem == null)
                return true;

            _raycastResults.Clear();

            var pointerEventData = new PointerEventData(eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                clickCount = 1,
                eligibleForClick = true,
                pointerId = pointerInput.PointerId,
                position = pointerInput.ScreenPosition,
                useDragThreshold = true
            };

            eventSystem.RaycastAll(pointerEventData, _raycastResults);

            for (var resultIndex = 0; resultIndex < _raycastResults.Count; resultIndex += 1)
            {
                var hitObject = _raycastResults[resultIndex].gameObject;

                if (hitObject == null)
                    continue;

                if (HasInteractableSelectable(hitObject) || HasPointerHandler(hitObject))
                    return false;
            }

            return true;
        }

        private bool HasInteractableSelectable(GameObject hitObject)
        {
            var selectables = hitObject.GetComponentsInParent<Selectable>(true);

            for (var selectableIndex = 0; selectableIndex < selectables.Length; selectableIndex += 1)
            {
                var selectable = selectables[selectableIndex];

                if (selectable != null && selectable.IsActive() && selectable.IsInteractable())
                    return true;
            }

            return false;
        }

        private bool HasPointerHandler(GameObject hitObject)
        {
            return ExecuteEvents.GetEventHandler<IPointerClickHandler>(hitObject) != null
                   || ExecuteEvents.GetEventHandler<IPointerDownHandler>(hitObject) != null
                   || ExecuteEvents.GetEventHandler<IBeginDragHandler>(hitObject) != null
                   || ExecuteEvents.GetEventHandler<IDragHandler>(hitObject) != null
                   || ExecuteEvents.GetEventHandler<IScrollHandler>(hitObject) != null;
        }
    }
}

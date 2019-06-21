using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Text;
using System;

namespace SimpleUI
{



  public abstract class PointerInputModule : BaseInputModule
  {

        public BaseInput GetInput () {
            if (inputOverride != null) {
                // Debug.Log("overrideen" + inputOverride);
                return inputOverride;
            }
            return input;
        }

    protected Dictionary<int, PointerEventData> m_PointerData = new Dictionary<int, PointerEventData>();
    private readonly PointerInputModule.MouseState m_MouseState = new PointerInputModule.MouseState();
    /// <summary>
    ///   <para>Id of the cached left mouse pointer event.</para>
    /// </summary>
    public const int kMouseLeftId = -1;
    /// <summary>
    ///   <para>Id of the cached right mouse pointer event.</para>
    /// </summary>
    public const int kMouseRightId = -2;
    /// <summary>
    ///   <para>Id of the cached middle mouse pointer event.</para>
    /// </summary>
    public const int kMouseMiddleId = -3;
    /// <summary>
    ///   <para>Touch id for when simulating touches on a non touch device.</para>
    /// </summary>
    public const int kFakeTouchesId = -4;

    protected bool GetPointerData(int id, out PointerEventData data, bool create)
    {
      if (this.m_PointerData.TryGetValue(id, out data) || !create)
        return false;
      data = new PointerEventData(this.eventSystem)
      {
        pointerId = id
      };
      this.m_PointerData.Add(id, data);
      return true;
    }

    /// <summary>
    ///   <para>Remove the PointerEventData from the cache.</para>
    /// </summary>
    /// <param name="data"></param>
    protected void RemovePointerData(PointerEventData data)
    {
      this.m_PointerData.Remove(data.pointerId);
    }

    protected PointerEventData GetTouchPointerEventData(Touch input, out bool pressed, out bool released)
    {
      PointerEventData data;
      bool pointerData = this.GetPointerData(input.fingerId, out data, true);
      data.Reset();
      pressed = pointerData || input.phase == TouchPhase.Began;
      released = input.phase == TouchPhase.Canceled || input.phase == TouchPhase.Ended;
      if (pointerData)
        data.position = input.position;
      data.delta = !pressed ? input.position - data.position : Vector2.zero;
      data.position = input.position;
      data.button = PointerEventData.InputButton.Left;
      this.eventSystem.RaycastAll(data, this.m_RaycastResultCache);
      RaycastResult firstRaycast = BaseInputModule.FindFirstRaycast(this.m_RaycastResultCache);
      data.pointerCurrentRaycast = firstRaycast;
      this.m_RaycastResultCache.Clear();
      return data;
    }

    /// <summary>
    ///   <para>Copy one PointerEventData to another.</para>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    protected void CopyFromTo(PointerEventData from, PointerEventData to)
    {
      to.position = from.position;
      to.delta = from.delta;
      to.scrollDelta = from.scrollDelta;
      to.pointerCurrentRaycast = from.pointerCurrentRaycast;
      to.pointerEnter = from.pointerEnter;
    }

    /// <summary>
    ///   <para>Given a mouse button return the current state for the frame.</para>
    /// </summary>
    /// <param name="buttonId">Mouse Button id.</param>
    protected PointerEventData.FramePressState StateForMouseButton(int buttonId)
    {
      bool mouseButtonDown = GetInput().GetMouseButtonDown(buttonId);
      bool mouseButtonUp = GetInput().GetMouseButtonUp(buttonId);
      if (mouseButtonDown && mouseButtonUp)
        return PointerEventData.FramePressState.PressedAndReleased;
      if (mouseButtonDown)
        return PointerEventData.FramePressState.Pressed;
      return mouseButtonUp ? PointerEventData.FramePressState.Released : PointerEventData.FramePressState.NotChanged;
    }

    /// <summary>
    ///   <para>Return the current MouseState.</para>
    /// </summary>
    /// <param name="id"></param>
    protected virtual PointerInputModule.MouseState GetMousePointerEventData()
    {
      return this.GetMousePointerEventData(0);
    }

    /// <summary>
    ///   <para>Return the current MouseState.</para>
    /// </summary>
    /// <param name="id"></param>
    protected virtual PointerInputModule.MouseState GetMousePointerEventData(int id)
    {
      PointerEventData data1;
      bool pointerData = this.GetPointerData(-1, out data1, true);
      data1.Reset();
      if (pointerData)
        data1.position = (Vector2) GetInput().mousePosition;
      Vector2 mousePosition = (Vector2) GetInput().mousePosition;
      data1.delta = mousePosition - data1.position;
      data1.position = mousePosition;

      data1.scrollDelta = GetInput().mouseScrollDelta;
      
      data1.button = PointerEventData.InputButton.Left;
      this.eventSystem.RaycastAll(data1, this.m_RaycastResultCache);
      RaycastResult firstRaycast = BaseInputModule.FindFirstRaycast(this.m_RaycastResultCache);
      data1.pointerCurrentRaycast = firstRaycast;
      this.m_RaycastResultCache.Clear();
      PointerEventData data2;
      this.GetPointerData(-2, out data2, true);
      this.CopyFromTo(data1, data2);
      data2.button = PointerEventData.InputButton.Right;
      PointerEventData data3;
      this.GetPointerData(-3, out data3, true);
      this.CopyFromTo(data1, data3);
      data3.button = PointerEventData.InputButton.Middle;
      this.m_MouseState.SetButtonState(PointerEventData.InputButton.Left, this.StateForMouseButton(0), data1);
      this.m_MouseState.SetButtonState(PointerEventData.InputButton.Right, this.StateForMouseButton(1), data2);
      this.m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, this.StateForMouseButton(2), data3);
      return this.m_MouseState;
    }

    /// <summary>
    ///   <para>Return the last PointerEventData for the given touch / mouse id.</para>
    /// </summary>
    /// <param name="id"></param>
    protected PointerEventData GetLastPointerEventData(int id)
    {
      PointerEventData data;
      this.GetPointerData(id, out data, false);
      return data;
    }

    private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
    {
      if (!useDragThreshold)
        return true;
      return (double) (pressPos - currentPos).sqrMagnitude >= (double) threshold * (double) threshold;
    }

    /// <summary>
    ///   <para>Process movement for the current frame with the given pointer event.</para>
    /// </summary>
    /// <param name="pointerEvent"></param>
    protected virtual void ProcessMove(PointerEventData pointerEvent)
    {
      GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
      this.HandlePointerExitAndEnter(pointerEvent, gameObject);
    }

    /// <summary>
    ///   <para>Process the drag for the current frame with the given pointer event.</para>
    /// </summary>
    /// <param name="pointerEvent"></param>
    protected virtual void ProcessDrag(PointerEventData pointerEvent)
    {
      bool flag = pointerEvent.IsPointerMoving();
      if (flag && (UnityEngine.Object) pointerEvent.pointerDrag != (UnityEngine.Object) null && (!pointerEvent.dragging && PointerInputModule.ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, (float) this.eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold)))
      {
        ExecuteEvents.Execute<IBeginDragHandler>(pointerEvent.pointerDrag, (BaseEventData) pointerEvent, ExecuteEvents.beginDragHandler);
        pointerEvent.dragging = true;
      }
      if (!pointerEvent.dragging || !flag || !((UnityEngine.Object) pointerEvent.pointerDrag != (UnityEngine.Object) null))
        return;
      if ((UnityEngine.Object) pointerEvent.pointerPress != (UnityEngine.Object) pointerEvent.pointerDrag)
      {
        ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, (BaseEventData) pointerEvent, ExecuteEvents.pointerUpHandler);
        pointerEvent.eligibleForClick = false;
        pointerEvent.pointerPress = (GameObject) null;
        pointerEvent.rawPointerPress = (GameObject) null;
      }
      ExecuteEvents.Execute<IDragHandler>(pointerEvent.pointerDrag, (BaseEventData) pointerEvent, ExecuteEvents.dragHandler);
    }

    public override bool IsPointerOverGameObject(int pointerId)
    {
      PointerEventData pointerEventData = this.GetLastPointerEventData(pointerId);
      if (pointerEventData != null)
        return (UnityEngine.Object) pointerEventData.pointerEnter != (UnityEngine.Object) null;
      return false;
    }

    /// <summary>
    ///   <para>Clear all pointers and deselect any selected objects in the EventSystem.</para>
    /// </summary>
    protected void ClearSelection()
    {
      BaseEventData baseEventData = this.GetBaseEventData();
      using (Dictionary<int, PointerEventData>.ValueCollection.Enumerator enumerator = this.m_PointerData.Values.GetEnumerator())
      {
        while (enumerator.MoveNext())
          this.HandlePointerExitAndEnter(enumerator.Current, (GameObject) null);
      }
      this.m_PointerData.Clear();
      this.eventSystem.SetSelectedGameObject((GameObject) null, baseEventData);
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder("<b>Pointer Input Module of type: </b>" + (object) this.GetType());
      stringBuilder.AppendLine();
      using (Dictionary<int, PointerEventData>.Enumerator enumerator = this.m_PointerData.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          KeyValuePair<int, PointerEventData> current = enumerator.Current;
          if (current.Value != null)
          {
            stringBuilder.AppendLine("<B>Pointer:</b> " + (object) current.Key);
            stringBuilder.AppendLine(current.Value.ToString());
          }
        }
      }
      return stringBuilder.ToString();
    }

    /// <summary>
    ///   <para>Deselect the current selected GameObject if the currently pointed-at GameObject is different.</para>
    /// </summary>
    /// <param name="currentOverGo">The GameObject the pointer is currently over.</param>
    /// <param name="pointerEvent">Current event data.</param>
    protected void DeselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
    {
      if (!((UnityEngine.Object) ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo) != (UnityEngine.Object) this.eventSystem.currentSelectedGameObject))
        return;
      this.eventSystem.SetSelectedGameObject((GameObject) null, pointerEvent);
    }

    protected class ButtonState
    {
      private PointerEventData.InputButton m_Button;
      private PointerInputModule.MouseButtonEventData m_EventData;

      public PointerInputModule.MouseButtonEventData eventData
      {
        get
        {
          return this.m_EventData;
        }
        set
        {
          this.m_EventData = value;
        }
      }

      public PointerEventData.InputButton button
      {
        get
        {
          return this.m_Button;
        }
        set
        {
          this.m_Button = value;
        }
      }
    }

    protected class MouseState
    {
      private List<PointerInputModule.ButtonState> m_TrackedButtons = new List<PointerInputModule.ButtonState>();

      public bool AnyPressesThisFrame()
      {
        for (int index = 0; index < this.m_TrackedButtons.Count; ++index)
        {
          if (this.m_TrackedButtons[index].eventData.PressedThisFrame())
            return true;
        }
        return false;
      }

      public bool AnyReleasesThisFrame()
      {
        for (int index = 0; index < this.m_TrackedButtons.Count; ++index)
        {
          if (this.m_TrackedButtons[index].eventData.ReleasedThisFrame())
            return true;
        }
        return false;
      }

      public PointerInputModule.ButtonState GetButtonState(PointerEventData.InputButton button)
      {
        PointerInputModule.ButtonState buttonState = (PointerInputModule.ButtonState) null;
        for (int index = 0; index < this.m_TrackedButtons.Count; ++index)
        {
          if (this.m_TrackedButtons[index].button == button)
          {
            buttonState = this.m_TrackedButtons[index];
            break;
          }
        }
        if (buttonState == null)
        {
          buttonState = new PointerInputModule.ButtonState()
          {
            button = button,
            eventData = new PointerInputModule.MouseButtonEventData()
          };
          this.m_TrackedButtons.Add(buttonState);
        }
        return buttonState;
      }

      public void SetButtonState(PointerEventData.InputButton button, PointerEventData.FramePressState stateForMouseButton, PointerEventData data)
      {
        PointerInputModule.ButtonState buttonState = this.GetButtonState(button);
        buttonState.eventData.buttonState = stateForMouseButton;
        buttonState.eventData.buttonData = data;
      }
    }

    /// <summary>
    ///   <para>Information about a mouse button event.</para>
    /// </summary>
    public class MouseButtonEventData
    {
      /// <summary>
      ///   <para>The state of the button this frame.</para>
      /// </summary>
      public PointerEventData.FramePressState buttonState;
      /// <summary>
      ///   <para>Pointer data associated with the mouse event.</para>
      /// </summary>
      public PointerEventData buttonData;

      /// <summary>
      ///   <para>Was the button pressed this frame?</para>
      /// </summary>
      public bool PressedThisFrame()
      {
        if (this.buttonState != PointerEventData.FramePressState.Pressed)
          return this.buttonState == PointerEventData.FramePressState.PressedAndReleased;
        return true;
      }

      /// <summary>
      ///   <para>Was the button released this frame?</para>
      /// </summary>
      public bool ReleasedThisFrame()
      {
        if (this.buttonState != PointerEventData.FramePressState.Released)
          return this.buttonState == PointerEventData.FramePressState.PressedAndReleased;
        return true;
      }
    }
  }


  [AddComponentMenu("Event/Standalone Input Module")]
  public class StandaloneInputModule : PointerInputModule
  {
    [Header("USING CUSTOM")] // just to make usre its ours
    [SerializeField]
    private string m_HorizontalAxis = "Horizontal";
    [SerializeField]
    private string m_VerticalAxis = "Vertical";
    [SerializeField]
    private string m_SubmitButton = "Submit";
    [SerializeField]
    private string m_CancelButton = "Cancel";
    [SerializeField]
    private float m_InputActionsPerSecond = 10f;
    [SerializeField]
    private float m_RepeatDelay = 0.5f;
    private float m_PrevActionTime;
    private Vector2 m_LastMoveVector;
    private int m_ConsecutiveMoveCount;
    private Vector2 m_LastMousePosition;
    private Vector2 m_MousePosition;
    [SerializeField]
    [FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
    private bool m_ForceModuleActive;

    
    /// <summary>
    ///   <para>Force this module to be active.</para>
    /// </summary>
    public bool forceModuleActive
    {
      get
      {
        return this.m_ForceModuleActive;
      }
      set
      {
        this.m_ForceModuleActive = value;
      }
    }

    /// <summary>
    ///   <para>Number of keyboard / controller inputs allowed per second.</para>
    /// </summary>
    public float inputActionsPerSecond
    {
      get
      {
        return this.m_InputActionsPerSecond;
      }
      set
      {
        this.m_InputActionsPerSecond = value;
      }
    }

    /// <summary>
    ///   <para>Delay in seconds before the input actions per second repeat rate takes effect.</para>
    /// </summary>
    public float repeatDelay
    {
      get
      {
        return this.m_RepeatDelay;
      }
      set
      {
        this.m_RepeatDelay = value;
      }
    }

    /// <summary>
    ///   <para>Input manager name for the horizontal axis button.</para>
    /// </summary>
    public string horizontalAxis
    {
      get
      {
        return this.m_HorizontalAxis;
      }
      set
      {
        this.m_HorizontalAxis = value;
      }
    }

    /// <summary>
    ///   <para>Input manager name for the vertical axis.</para>
    /// </summary>
    public string verticalAxis
    {
      get
      {
        return this.m_VerticalAxis;
      }
      set
      {
        this.m_VerticalAxis = value;
      }
    }

    /// <summary>
    ///   <para>Maximum number of input events handled per second.</para>
    /// </summary>
    public string submitButton
    {
      get
      {
        return this.m_SubmitButton;
      }
      set
      {
        this.m_SubmitButton = value;
      }
    }

    /// <summary>
    ///   <para>Input manager name for the 'cancel' button.</para>
    /// </summary>
    public string cancelButton
    {
      get
      {
        return this.m_CancelButton;
      }
      set
      {
        this.m_CancelButton = value;
      }
    }

    protected StandaloneInputModule()
    {
    }

    /// <summary>
    ///   <para>See BaseInputModule.</para>
    /// </summary>
    public override void UpdateModule()
    {
      this.m_LastMousePosition = this.m_MousePosition;
      this.m_MousePosition = (Vector2) GetInput().mousePosition;
    }

    /// <summary>
    ///   <para>See BaseInputModule.</para>
    /// </summary>
    /// <returns>
    ///   <para>Supported.</para>
    /// </returns>
    public override bool IsModuleSupported()
    {
      if (!this.m_ForceModuleActive && !Input.mousePresent)
        return Input.touchSupported;
      return true;
    }

    /// <summary>
    ///   <para>See BaseInputModule.</para>
    /// </summary>
    /// <returns>
    ///   <para>Should activate.</para>
    /// </returns>
    public override bool ShouldActivateModule()
    {
      if (!base.ShouldActivateModule())
        return false;
      bool flag = this.m_ForceModuleActive | 
        GetInput().GetButtonDown(this.m_SubmitButton) | 
        GetInput().GetButtonDown(this.m_CancelButton) | 
        !Mathf.Approximately(GetInput().GetAxisRaw(this.m_HorizontalAxis), 0.0f) | 
        !Mathf.Approximately(GetInput().GetAxisRaw(this.m_VerticalAxis), 0.0f) | 
        (double) (this.m_MousePosition - this.m_LastMousePosition).sqrMagnitude > 0.0 | 
        GetInput().GetMouseButtonDown(0);




      if (Input.touchCount > 0)
        flag = true;
      return flag;
    }

    /// <summary>
    ///   <para>See BaseInputModule.</para>
    /// </summary>
    public override void ActivateModule()
    {
      base.ActivateModule();
      this.m_MousePosition = (Vector2) GetInput().mousePosition;
      this.m_LastMousePosition = (Vector2) GetInput().mousePosition;
      GameObject selectedGameObject = this.eventSystem.currentSelectedGameObject;
      if ((UnityEngine.Object) selectedGameObject == (UnityEngine.Object) null)
        selectedGameObject = this.eventSystem.firstSelectedGameObject;
      this.eventSystem.SetSelectedGameObject(selectedGameObject, this.GetBaseEventData());
    }

    /// <summary>
    ///   <para>See BaseInputModule.</para>
    /// </summary>
    public override void DeactivateModule()
    {
      base.DeactivateModule();
      this.ClearSelection();
    }

    /// <summary>
    ///   <para>See BaseInputModule.</para>
    /// </summary>
    public override void Process()
    {
      bool selectedObject = this.SendUpdateEventToSelectedObject();
      if (this.eventSystem.sendNavigationEvents)
      {
        if (!selectedObject)
          selectedObject |= this.SendMoveEventToSelectedObject();
        if (!selectedObject)
          this.SendSubmitEventToSelectedObject();
      }
      if (this.ProcessTouchEvents())
        return;
      this.ProcessMouseEvent();
    }

    private bool ProcessTouchEvents()
    {
      for (int index = 0; index < Input.touchCount; ++index)
      {
        Touch touch = Input.GetTouch(index);
        if (touch.type != TouchType.Indirect)
        {
          bool pressed;
          bool released;
          PointerEventData pointerEventData = this.GetTouchPointerEventData(touch, out pressed, out released);
          this.ProcessTouchPress(pointerEventData, pressed, released);
          if (!released)
          {
            this.ProcessMove(pointerEventData);
            this.ProcessDrag(pointerEventData);
          }
          else
            this.RemovePointerData(pointerEventData);
        }
      }
      return Input.touchCount > 0;
    }

    private void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
    {
      GameObject gameObject1 = pointerEvent.pointerCurrentRaycast.gameObject;
      if (pressed)
      {
        pointerEvent.eligibleForClick = true;
        pointerEvent.delta = Vector2.zero;
        pointerEvent.dragging = false;
        pointerEvent.useDragThreshold = true;
        pointerEvent.pressPosition = pointerEvent.position;
        pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
        this.DeselectIfSelectionChanged(gameObject1, (BaseEventData) pointerEvent);
        if ((UnityEngine.Object) pointerEvent.pointerEnter != (UnityEngine.Object) gameObject1)
        {
          this.HandlePointerExitAndEnter(pointerEvent, gameObject1);
          pointerEvent.pointerEnter = gameObject1;
        }
        GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject1, (BaseEventData) pointerEvent, ExecuteEvents.pointerDownHandler);
        if ((UnityEngine.Object) gameObject2 == (UnityEngine.Object) null)
          gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject1);
        float unscaledTime = Time.unscaledTime;
        if ((UnityEngine.Object) gameObject2 == (UnityEngine.Object) pointerEvent.lastPress)
        {
          if ((double) (unscaledTime - pointerEvent.clickTime) < 0.300000011920929)
            ++pointerEvent.clickCount;
          else
            pointerEvent.clickCount = 1;
          pointerEvent.clickTime = unscaledTime;
        }
        else
          pointerEvent.clickCount = 1;
        pointerEvent.pointerPress = gameObject2;
        pointerEvent.rawPointerPress = gameObject1;
        pointerEvent.clickTime = unscaledTime;
        pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject1);
        if ((UnityEngine.Object) pointerEvent.pointerDrag != (UnityEngine.Object) null)
          ExecuteEvents.Execute<IInitializePotentialDragHandler>(pointerEvent.pointerDrag, (BaseEventData) pointerEvent, ExecuteEvents.initializePotentialDrag);
      }
      if (!released)
        return;
      ExecuteEvents.Execute<IPointerUpHandler>(pointerEvent.pointerPress, (BaseEventData) pointerEvent, ExecuteEvents.pointerUpHandler);
      GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject1);
      if ((UnityEngine.Object) pointerEvent.pointerPress == (UnityEngine.Object) eventHandler && pointerEvent.eligibleForClick)
        ExecuteEvents.Execute<IPointerClickHandler>(pointerEvent.pointerPress, (BaseEventData) pointerEvent, ExecuteEvents.pointerClickHandler);
      else if ((UnityEngine.Object) pointerEvent.pointerDrag != (UnityEngine.Object) null && pointerEvent.dragging)
        ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject1, (BaseEventData) pointerEvent, ExecuteEvents.dropHandler);
      pointerEvent.eligibleForClick = false;
      pointerEvent.pointerPress = (GameObject) null;
      pointerEvent.rawPointerPress = (GameObject) null;
      if ((UnityEngine.Object) pointerEvent.pointerDrag != (UnityEngine.Object) null && pointerEvent.dragging)
        ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, (BaseEventData) pointerEvent, ExecuteEvents.endDragHandler);
      pointerEvent.dragging = false;
      pointerEvent.pointerDrag = (GameObject) null;
      if ((UnityEngine.Object) pointerEvent.pointerDrag != (UnityEngine.Object) null)
        ExecuteEvents.Execute<IEndDragHandler>(pointerEvent.pointerDrag, (BaseEventData) pointerEvent, ExecuteEvents.endDragHandler);
      pointerEvent.pointerDrag = (GameObject) null;
      ExecuteEvents.ExecuteHierarchy<IPointerExitHandler>(pointerEvent.pointerEnter, (BaseEventData) pointerEvent, ExecuteEvents.pointerExitHandler);
      pointerEvent.pointerEnter = (GameObject) null;
    }

    /// <summary>
    ///   <para>Calculate and send a submit event to the current selected object.</para>
    /// </summary>
    /// <returns>
    ///   <para>If the submit event was used by the selected object.</para>
    /// </returns>
    protected bool SendSubmitEventToSelectedObject()
    {
      if ((UnityEngine.Object) this.eventSystem.currentSelectedGameObject == (UnityEngine.Object) null)
        return false;
      BaseEventData baseEventData = this.GetBaseEventData();
      if (GetInput().GetButtonDown(this.m_SubmitButton))
        ExecuteEvents.Execute<ISubmitHandler>(this.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.submitHandler);
      if (GetInput().GetButtonDown(this.m_CancelButton))
        ExecuteEvents.Execute<ICancelHandler>(this.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.cancelHandler);
      return baseEventData.used;
    }

    private Vector2 GetRawMoveVector()
    {
      Vector2 zero = Vector2.zero;
      zero.x = GetInput().GetAxisRaw(this.m_HorizontalAxis);

      // Debug.LogError("checkign: " + this.m_VerticalAxis);

      zero.y = GetInput().GetAxisRaw(this.m_VerticalAxis);
      Debug.LogError("raw move " + zero);

      if (GetInput().GetButtonDown(this.m_HorizontalAxis))
      {
        if ((double) zero.x < 0.0)
          zero.x = -1f;
        if ((double) zero.x > 0.0)
          zero.x = 1f;
      }
      if (GetInput().GetButtonDown(this.m_VerticalAxis))
      {
        if ((double) zero.y < 0.0)
          zero.y = -1f;
        if ((double) zero.y > 0.0)
          zero.y = 1f;
      }
      return zero;
    }

    /// <summary>
    ///   <para>Calculate and send a move event to the current selected object.</para>
    /// </summary>
    /// <returns>
    ///   <para>If the move event was used by the selected object.</para>
    /// </returns>
    protected bool SendMoveEventToSelectedObject()
    {
      float unscaledTime = Time.unscaledTime;

      Vector2 rawMoveVector = this.GetRawMoveVector();
      if (Mathf.Approximately(rawMoveVector.x, 0.0f) && Mathf.Approximately(rawMoveVector.y, 0.0f))
      {
        this.m_ConsecutiveMoveCount = 0;
        return false;
      }
      
      bool flag1 = GetInput().GetButtonDown(this.m_HorizontalAxis) || GetInput().GetButtonDown(this.m_VerticalAxis);
      bool flag2 = (double) Vector2.Dot(rawMoveVector, this.m_LastMoveVector) > 0.0;
      if (!flag1)
        flag1 = !flag2 || this.m_ConsecutiveMoveCount != 1 ? (double) unscaledTime > (double) this.m_PrevActionTime + 1.0 / (double) this.m_InputActionsPerSecond : (double) unscaledTime > (double) this.m_PrevActionTime + (double) this.m_RepeatDelay;
      if (!flag1)
        return false;

      Debug.LogError("ehhh moving " + rawMoveVector);
        
      AxisEventData axisEventData = this.GetAxisEventData(rawMoveVector.x, rawMoveVector.y, 0.6f);
      if (axisEventData.moveDir != MoveDirection.None)
      {

        Debug.LogError("moving " + rawMoveVector);
        ExecuteEvents.Execute<IMoveHandler>(this.eventSystem.currentSelectedGameObject, (BaseEventData) axisEventData, ExecuteEvents.moveHandler);
        if (!flag2)
          this.m_ConsecutiveMoveCount = 0;
        ++this.m_ConsecutiveMoveCount;
        this.m_PrevActionTime = unscaledTime;
        this.m_LastMoveVector = rawMoveVector;
      }
      else
        this.m_ConsecutiveMoveCount = 0;
      return axisEventData.used;
    }

    /// <summary>
    ///   <para>Iterate through all the different mouse events.</para>
    /// </summary>
    /// <param name="id">The mouse pointer Event data id to get.</param>
    protected void ProcessMouseEvent()
    {
      this.ProcessMouseEvent(0);
    }

    /// <summary>
    ///   <para>Iterate through all the different mouse events.</para>
    /// </summary>
    /// <param name="id">The mouse pointer Event data id to get.</param>
    protected void ProcessMouseEvent(int id)
    {
      PointerInputModule.MouseState pointerEventData = this.GetMousePointerEventData(id);
      PointerInputModule.MouseButtonEventData eventData = pointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
      this.ProcessMousePress(eventData);
      this.ProcessMove(eventData.buttonData);
      this.ProcessDrag(eventData.buttonData);
      this.ProcessMousePress(pointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData);
      this.ProcessDrag(pointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
      this.ProcessMousePress(pointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
      this.ProcessDrag(pointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
      
      if (Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
        return;


      // Debug.LogError("Scrolling!!" + eventData.buttonData.scrollDelta.sqrMagnitude);
      // AxisEventData axisEventData = this.GetAxisEventData(eventData.buttonData.scrollDelta.x, eventData.buttonData.scrollDelta.y, 0.6f);
      

      // ExecuteEvents.Execute<IMoveHandler>(this.eventSystem.currentSelectedGameObject, (BaseEventData) axisEventData, ExecuteEvents.moveHandler);
      
      ExecuteEvents.ExecuteHierarchy<IScrollHandler>(ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject), (BaseEventData) eventData.buttonData, ExecuteEvents.scrollHandler);
    }

    /// <summary>
    ///   <para>Send a update event to the currently selected object.</para>
    /// </summary>
    /// <returns>
    ///   <para>If the update event was used by the selected object.</para>
    /// </returns>
    protected bool SendUpdateEventToSelectedObject()
    {
      if ((UnityEngine.Object) this.eventSystem.currentSelectedGameObject == (UnityEngine.Object) null)
        return false;
      BaseEventData baseEventData = this.GetBaseEventData();
      ExecuteEvents.Execute<IUpdateSelectedHandler>(this.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
      return baseEventData.used;
    }

    protected void ProcessMousePress(PointerInputModule.MouseButtonEventData data)
    {
      PointerEventData buttonData = data.buttonData;
      GameObject gameObject1 = buttonData.pointerCurrentRaycast.gameObject;
      if (data.PressedThisFrame())
      {
        buttonData.eligibleForClick = true;
        buttonData.delta = Vector2.zero;
        buttonData.dragging = false;
        buttonData.useDragThreshold = true;
        buttonData.pressPosition = buttonData.position;
        buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
        this.DeselectIfSelectionChanged(gameObject1, (BaseEventData) buttonData);
        GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy<IPointerDownHandler>(gameObject1, (BaseEventData) buttonData, ExecuteEvents.pointerDownHandler);
        if ((UnityEngine.Object) gameObject2 == (UnityEngine.Object) null)
          gameObject2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject1);
        float unscaledTime = Time.unscaledTime;
        if ((UnityEngine.Object) gameObject2 == (UnityEngine.Object) buttonData.lastPress)
        {
          if ((double) (unscaledTime - buttonData.clickTime) < 0.300000011920929)
            ++buttonData.clickCount;
          else
            buttonData.clickCount = 1;
          buttonData.clickTime = unscaledTime;
        }
        else
          buttonData.clickCount = 1;
        buttonData.pointerPress = gameObject2;
        buttonData.rawPointerPress = gameObject1;
        buttonData.clickTime = unscaledTime;
        buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject1);
        if ((UnityEngine.Object) buttonData.pointerDrag != (UnityEngine.Object) null)
          ExecuteEvents.Execute<IInitializePotentialDragHandler>(buttonData.pointerDrag, (BaseEventData) buttonData, ExecuteEvents.initializePotentialDrag);
      }
      if (!data.ReleasedThisFrame())
        return;
      ExecuteEvents.Execute<IPointerUpHandler>(buttonData.pointerPress, (BaseEventData) buttonData, ExecuteEvents.pointerUpHandler);
      GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject1);
      if ((UnityEngine.Object) buttonData.pointerPress == (UnityEngine.Object) eventHandler && buttonData.eligibleForClick)
        ExecuteEvents.Execute<IPointerClickHandler>(buttonData.pointerPress, (BaseEventData) buttonData, ExecuteEvents.pointerClickHandler);
      else if ((UnityEngine.Object) buttonData.pointerDrag != (UnityEngine.Object) null && buttonData.dragging)
        ExecuteEvents.ExecuteHierarchy<IDropHandler>(gameObject1, (BaseEventData) buttonData, ExecuteEvents.dropHandler);
      buttonData.eligibleForClick = false;
      buttonData.pointerPress = (GameObject) null;
      buttonData.rawPointerPress = (GameObject) null;
      if ((UnityEngine.Object) buttonData.pointerDrag != (UnityEngine.Object) null && buttonData.dragging)
        ExecuteEvents.Execute<IEndDragHandler>(buttonData.pointerDrag, (BaseEventData) buttonData, ExecuteEvents.endDragHandler);
      buttonData.dragging = false;
      buttonData.pointerDrag = (GameObject) null;
      if (!((UnityEngine.Object) gameObject1 != (UnityEngine.Object) buttonData.pointerEnter))
        return;
      this.HandlePointerExitAndEnter(buttonData, (GameObject) null);
      this.HandlePointerExitAndEnter(buttonData, gameObject1);
    }
  }








    //     private float m_NextAction;

    //     private Vector2 m_LastMousePosition;
    //     private Vector2 m_MousePosition;


    //     [SerializeField]
    //     private string m_HorizontalAxis = "Horizontal";

    //     /// <summary>
    //     /// Name of the vertical axis for movement (if axis events are used).
    //     /// </summary>
    //     [SerializeField]
    //     private string m_VerticalAxis = "Vertical";

    //     /// <summary>
    //     /// Name of the submit button.
    //     /// </summary>
    //     [SerializeField]
    //     private string m_SubmitButton = "Submit";

    //     /// <summary>
    //     /// Name of the submit button.
    //     /// </summary>
    //     [SerializeField]
    //     private string m_CancelButton = "Cancel";

    //     [SerializeField]
    //     private float m_InputActionsPerSecond = 10;

    //     [SerializeField]
    //     private bool m_AllowActivationOnMobileDevice;

    //     public bool allowActivationOnMobileDevice
    //     {
    //         get { return m_AllowActivationOnMobileDevice; }
    //         set { m_AllowActivationOnMobileDevice = value; }
    //     }

    //     public float inputActionsPerSecond
    //     {
    //         get { return m_InputActionsPerSecond; }
    //         set { m_InputActionsPerSecond = value; }
    //     }

    //     /// <summary>
    //     /// Name of the horizontal axis for movement (if axis events are used).
    //     /// </summary>
    //     public string horizontalAxis
    //     {
    //         get { return m_HorizontalAxis; }
    //         set { m_HorizontalAxis = value; }
    //     }

    //     /// <summary>
    //     /// Name of the vertical axis for movement (if axis events are used).
    //     /// </summary>
    //     public string verticalAxis
    //     {
    //         get { return m_VerticalAxis; }
    //         set { m_VerticalAxis = value; }
    //     }

    //     public string submitButton
    //     {
    //         get { return m_SubmitButton; }
    //         set { m_SubmitButton = value; }
    //     }

    //     public string cancelButton
    //     {
    //         get { return m_CancelButton; }
    //         set { m_CancelButton = value; }
    //     }

    //     BaseInput GetInput() {
    //         if (inputOverride != null) {
    //             // Debug.Log("overrideen" + inputOverride);
    //             return inputOverride;
    //         }
    //         return input;
    //     }

    //     public override void UpdateModule()
    //     {
    //         m_LastMousePosition = m_MousePosition;
    //         m_MousePosition = GetInput().mousePosition;
    //     }

    //     public override bool IsModuleSupported()
    //     {
    //         // Check for mouse presence instead of whether touch is supported,
    //         // as you can connect mouse to a tablet and in that case we'd want
    //         // to use StandaloneInputModule for non-touch input events.
    //         return m_AllowActivationOnMobileDevice || GetInput().mousePresent;
    //     }

    //     public override bool ShouldActivateModule()
    //     {
    //         if (!base.ShouldActivateModule())
    //             return false;

    //         var shouldActivate = GetInput().GetButtonDown(m_SubmitButton);
    //         shouldActivate |= GetInput().GetButtonDown(m_CancelButton);
    //         shouldActivate |= !Mathf.Approximately(GetInput().GetAxisRaw(m_HorizontalAxis), 0.0f);
    //         shouldActivate |= !Mathf.Approximately(GetInput().GetAxisRaw(m_VerticalAxis), 0.0f);
    //         shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
    //         shouldActivate |= GetInput().GetMouseButtonDown(0);
    //         return shouldActivate;
    //     }

    //     public override void ActivateModule()
    //     {
    //         base.ActivateModule();
    //         m_MousePosition = GetInput().mousePosition;
    //         m_LastMousePosition = GetInput().mousePosition;

    //         var toSelect = eventSystem.currentSelectedGameObject;
    //         // if (toSelect == null)
    //         //     toSelect = eventSystem.lastSelectedGameObject;
    //         if (toSelect == null)
    //             toSelect = eventSystem.firstSelectedGameObject;

    //         eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
    //     }

    //     public override void DeactivateModule()
    //     {
    //         base.DeactivateModule();
    //         ClearSelection();
    //     }

    //     public override void Process()
    //     {
    //         bool usedEvent = SendUpdateEventToSelectedObject();

    //         if (eventSystem.sendNavigationEvents)
    //         {
    //             if (!usedEvent)
    //                 usedEvent |= SendMoveEventToSelectedObject();

                
    //             if (!usedEvent)
    //                 SendSubmitEventToSelectedObject();
    //         }

    //         ProcessMouseEvent();
    //     }

    //     /// <summary>
    //     /// Process submit keys.
    //     /// </summary>
    //     private bool SendSubmitEventToSelectedObject()
    //     {
    //         if (eventSystem.currentSelectedGameObject == null)
    //             return false;

    //         var data = GetBaseEventData();
    //         if (GetInput().GetButtonDown(m_SubmitButton))
    //             ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

    //         if (GetInput().GetButtonDown(m_CancelButton))
    //             ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
    //         return data.used;
    //     }

    //     private bool AllowMoveEventProcessing(float time)
    //     {
    //         bool allow = GetInput().GetButtonDown(m_HorizontalAxis);

    //         allow |= GetInput().GetButtonDown(m_VerticalAxis);
            
    //         allow |= (time > m_NextAction);
            
    //         return allow;
    //     }

    //     private Vector2 GetRawMoveVector()
    //     {
    //         Vector2 move = Vector2.zero;
    //         move.x = GetInput().GetAxisRaw(m_HorizontalAxis);

    //         move.y = GetInput().GetAxisRaw(m_VerticalAxis);

    //         Debug.LogError(move + " :: vert axis");

    //         if (GetInput().GetButtonDown(m_HorizontalAxis))
    //         {
    //             if (move.x < 0)
    //                 move.x = -1f;
    //             if (move.x > 0)
    //                 move.x = 1f;
    //         }
    //         if (GetInput().GetButtonDown(m_VerticalAxis))
    //         {
    //             if (move.y < 0)
    //                 move.y = -1f;
    //             if (move.y > 0)
    //                 move.y = 1f;
    //         }
    //         return move;
    //     }

    //     /// <summary>
    //     /// Process keyboard events.
    //     /// </summary>
    //     private bool SendMoveEventToSelectedObject()
    //     {
    //         float time = Time.unscaledTime;

    //         if (!AllowMoveEventProcessing(time)) {
    //             return false;
    //         }

    //         Vector2 movement = GetRawMoveVector();
    //         var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);
            
            
    //         if (!Mathf.Approximately(axisEventData.moveVector.x, 0f)
    //             || !Mathf.Approximately(axisEventData.moveVector.y, 0f))
    //         {
            
    //             ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);
    //         }
    //         m_NextAction = time + 1f / m_InputActionsPerSecond;
    //         return axisEventData.used;
    //     }

    //     /// <summary>
    //     /// Process all mouse events.
    //     /// </summary>
    //     private void ProcessMouseEvent()
    //     {
    //         MouseState mouseData = GetMousePointerEventData();

    //         bool pressed = mouseData.AnyPressesThisFrame();
    //         bool released = mouseData.AnyReleasesThisFrame();

    //         MouseButtonEventData leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

    //         if (!UseMouse(pressed, released, leftButtonData.buttonData))
    //             return;

    //         // Process the first mouse button fully
    //         ProcessMousePress(leftButtonData);
    //         ProcessMove(leftButtonData.buttonData);
    //         ProcessDrag(leftButtonData.buttonData);

    //         // Now process right / middle clicks
    //         ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
    //         ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
    //         ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
    //         ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

    //         if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
    //         {
    //             var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
    //             ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
    //         }
    //     }

    //     private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
    //     {
    //         if (pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling())
    //             return true;

    //         return false;
    //     }

    //     private bool SendUpdateEventToSelectedObject()
    //     {
    //         if (eventSystem.currentSelectedGameObject == null)
    //             return false;

    //         var data = GetBaseEventData();
    //         ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
    //         return data.used;
    //     }

    //     /// <summary>
    //     /// Process the current mouse press.
    //     /// </summary>
    //     private void ProcessMousePress(MouseButtonEventData data)
    //     {
    //         var pointerEvent = data.buttonData;
    //         var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

    //         // PointerDown notification
    //         if (data.PressedThisFrame())
    //         {
    //             pointerEvent.eligibleForClick = true;
    //             pointerEvent.delta = Vector2.zero;
    //             pointerEvent.dragging = false;
    //             pointerEvent.useDragThreshold = true;
    //             pointerEvent.pressPosition = pointerEvent.position;
    //             pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

    //             DeselectIfSelectionChanged(currentOverGo, pointerEvent);

    //             // search for the control that will receive the press
    //             // if we can't find a press handler set the press
    //             // handler to be what would receive a click.
    //             var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

    //             // didnt find a press handler... search for a click handler
    //             if (newPressed == null)
    //                 newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

    //             // Debug.Log("Pressed: " + newPressed);

    //             float time = Time.unscaledTime;

    //             if (newPressed == pointerEvent.lastPress)
    //             {
    //                 var diffTime = time - pointerEvent.clickTime;
    //                 if (diffTime < 0.3f)
    //                     ++pointerEvent.clickCount;
    //                 else
    //                     pointerEvent.clickCount = 1;

    //                 pointerEvent.clickTime = time;
    //             }
    //             else
    //             {
    //                 pointerEvent.clickCount = 1;
    //             }

    //             pointerEvent.pointerPress = newPressed;
    //             pointerEvent.rawPointerPress = currentOverGo;

    //             pointerEvent.clickTime = time;

    //             // Save the drag handler as well
    //             pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

    //             if (pointerEvent.pointerDrag != null)
    //                 ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
    //         }

    //         // PointerUp notification
    //         if (data.ReleasedThisFrame())
    //         {
    //             // Debug.Log("Executing pressup on: " + pointer.pointerPress);
    //             ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

    //             // Debug.Log("KeyCode: " + pointer.eventData.keyCode);

    //             // see if we mouse up on the same element that we clicked on...
    //             var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

    //             // PointerClick and Drop events
    //             if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
    //             {
    //                 ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
    //             }
    //             else if (pointerEvent.pointerDrag != null)
    //             {
    //                 ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
    //             }

    //             pointerEvent.eligibleForClick = false;
    //             pointerEvent.pointerPress = null;
    //             pointerEvent.rawPointerPress = null;

    //             if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
    //                 ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

    //             pointerEvent.dragging = false;
    //             pointerEvent.pointerDrag = null;

    //             // redo pointer enter / exit to refresh state
    //             // so that if we moused over somethign that ignored it before
    //             // due to having pressed on something else
    //             // it now gets it.
    //             if (currentOverGo != pointerEvent.pointerEnter)
    //             {
    //                 HandlePointerExitAndEnter(pointerEvent, null);
    //                 HandlePointerExitAndEnter(pointerEvent, currentOverGo);
    //             }
    //         }
    //     }
    // }
}
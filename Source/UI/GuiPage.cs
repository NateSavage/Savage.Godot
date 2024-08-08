using Godot;

namespace Savage.UI;

/// <summary> 
/// A top level UI control, Pages are special in that when one is open it's the main focus of the application. <br/>
/// Pages are essentially singletons where only one instance of a page may be open at a time. <br/>
/// You can open and close GUIPages using the <see cref="Gui"/> class.
/// </summary>
public abstract partial class GuiPage : Control {
    
    /// <summary> Called when graphics effects are started and the UI is beginning to appear. </summary>
    public event Action OpenStarted;

    /// <summary> Called when graphics effects are finished and menu object is fully visible. </summary>
    public event Action OpenFinished;

    /// <summary> Called when graphics effects are started and the UI is beginning to disappear. </summary>
    public event Action CloseStarted;

    /// <summary> Called when graphics effects are finished and ui object is fully hidden or destroyed. </summary>
    public event Action CloseFinished;

    protected void RaiseOpenStartedEvent() {
        OpenStarted?.Invoke();
    }

    protected void RaiseOpenFinishedEvent() {
        OpenFinished?.Invoke();
    }

    protected void RaiseCloseStartedEvent() {
        CloseStarted?.Invoke();
    }

    protected void RaiseCloseFinishedEvent() {
        CloseFinished?.Invoke();
    }

    public override void _Ready() {
        Open();
    }

    public virtual void Open() => RaiseOpenFinishedEvent();

    /// <summary> Hides or destroys this <see cref="GuiPage"/>, may not fully finish this frame because of playing animations or other graphics effects. </summary>
    /// <remarks> Default implementation marks object for immediate removal. </remarks>
    public virtual void Close() => RaiseCloseFinishedEvent();

    /// <summary> Immediately removes this <see cref="GuiPage"/>. </summary>
    /// <remarks> Default implementation marks object for immediate removal. </remarks>
    public virtual void Destroy() => RaiseCloseFinishedEvent();
}


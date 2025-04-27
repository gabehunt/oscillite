namespace Oscillite.Models
{
    public class DragState
    {
        public bool IsDraggingChannel;
        public int DragChannelIndex;
        public SharpDX.Point LastMousePosition;

        public bool IsDraggingAxis;
        public int DraggedAxisIndex;
        public float DragStartY;
        public float DragStartOffset;
        public float DragZoomStartMin;
        public float DragZoomStartMax;
        public float DragAxisStartOffset;
        public float DragAxisStartVoltage;
        public ViewportTransform DragAxisTransform;

        public int? DraggingTimeRuler;
        public float TimeRulerDragStartX;
        public float TimeRulerStartTime;

        public int? DraggingPhaseRuler;
        public float PhaseRulerStartTime;

        public (int channelIndex, int rulerIndex)? DraggingVoltageRuler;
        public float RulerDragStartY;
    }

}

using Oscillite;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System;

public class CursorSummaryOverlay
{
    public List<CursorRow> Rows { get; } = new List<CursorRow>();
    public bool IsDragging => isDragging;

    public RawVector2 Position = new RawVector2(200, 100); // default start location
    private bool isDragging = false;
    private RawVector2 dragOffset;
    private float width = 250;
    private float titleHeight = 28f;
    private bool isCollapsed = false;
    private RawRectangleF collapseButtonBounds;
    private List<RawRectangleF> value1Boxes = new List<RawRectangleF>();
    private List<RawRectangleF> value2Boxes = new List<RawRectangleF>();
    private int? editingRowIndex = null;
    private int? editingValueIndex = null;
    private string editingText = "";
    private WaveformViewer viewer;

    public CursorSummaryOverlay(WaveformViewer viewer)
    {
        this.viewer = viewer;
    }

    public void SetDefaults()
    {
        Rows.Clear();
    }

    public void SetCursorValues(string label, float? val1, float? val2, string unit, Color4 color)
    {
        var existing = Rows.FirstOrDefault(r => r.Label == label);
        if (existing != null)
        {
            existing.Value1 = val1;
            existing.Value2 = val2;
            existing.Unit = unit;
        }
        else
        {
            Rows.Add(new CursorRow
            {
                Label = label,
                Value1 = val1,
                Value2 = val2,
                Unit = unit,
                LabelColor = color
            });
        }
    }

    public void Draw(RenderTarget rt, SharpDX.Direct2D1.Factory d2dFactory, TextFormat textFormat)
    {
        value1Boxes.Clear();
        value2Boxes.Clear();

        float rowHeight = 24f;
        float height = titleHeight + (isCollapsed ? 0 : Rows.Count * (rowHeight + 2) + 10);

        var panelBounds = new RawRectangleF(Position.X, Position.Y, Position.X + width, Position.Y + height);

        // Background
        using (var bgBrush = new SolidColorBrush(rt, new RawColor4(0, 0, 0, 0.85f)))
        {
            rt.FillRectangle(panelBounds, bgBrush);
        }

        // Title bar background
        using (var titleBrush = new SolidColorBrush(rt, new RawColor4(0.15f, 0.15f, 0.15f, 0.85f)))
        {
            rt.FillRectangle(new RawRectangleF(Position.X, Position.Y, Position.X + width, Position.Y + titleHeight), titleBrush);
        }

        // Title
        using (var white = new SolidColorBrush(rt, new RawColor4(1, 1, 1, 1)))
        {
            rt.DrawText("Cursors", textFormat,
                new RawRectangleF(Position.X + 10, Position.Y + 4, Position.X + 200, Position.Y + titleHeight),
                white);
        }

        // Collapse button (draw on right side)
        string symbol = isCollapsed ? "+" : "–";

        float buttonSize = 20;
        float buttonTop = Position.Y + (titleHeight - buttonSize) / 2;

        collapseButtonBounds = GetCollapseButtonBounds();

        using (var white = new SolidColorBrush(rt, new RawColor4(1, 1, 1, 1)))
        {
            rt.DrawText(symbol, textFormat, collapseButtonBounds, white);
        }

        float padding = 8;
        float contentWidth = width - padding * 2;

        float labelWidth = contentWidth * 0.20f;
        float value1Width = contentWidth * 0.22f;
        float value2Width = contentWidth * 0.22f;
        float deltaWidth = contentWidth * 0.36f;

        float xLabel = Position.X + padding;
        float x1 = xLabel + labelWidth;
        float x2 = x1 + value1Width;
        float xDelta = x2 + value2Width;

        float y = Position.Y + titleHeight + 6;

        for (int i = 0; i < Rows.Count; i++)
        {
            var row = Rows[i];

            // Always track boxes even if collapsed
            value1Boxes.Add(new RawRectangleF(x1, y, x2 - 4, y + rowHeight));
            value2Boxes.Add(new RawRectangleF(x2, y, xDelta - 4, y + rowHeight));

            if (!isCollapsed)
            {
                using (var brush = new SolidColorBrush(rt, row.LabelColor))
                {
                    rt.DrawText(row.Label, textFormat, new RawRectangleF(xLabel, y, x1 - 4, y + rowHeight), brush);
                    rt.DrawText(row.GetValue1Text(), textFormat, new RawRectangleF(x1, y, x2 - 4, y + rowHeight), brush);
                    rt.DrawText(row.GetValue2Text(), textFormat, new RawRectangleF(x2, y, xDelta - 4, y + rowHeight), brush);
                    rt.DrawText($"Δ {row.GetDeltaText()}", textFormat, new RawRectangleF(xDelta, y, xDelta + deltaWidth - 4, y + rowHeight), brush);

                    if (editingRowIndex == i)
                    {
                        var editX = (editingValueIndex == 0) ? x1 : x2;
                        var editWidth = (editingValueIndex == 0) ? (x2 - x1) : (xDelta - x2);

                        using (var editBrush = new SolidColorBrush(rt, new RawColor4(1, 1, 1, 1)))
                        {
                            rt.DrawText(editingText + "_", textFormat,
                                new RawRectangleF(editX, y, editX + editWidth, y + rowHeight),
                                editBrush);
                        }
                    }
                }
            }

            y += rowHeight + 2;
        }
    }

    public bool HitTest(SharpDX.Point p)
    {
        float rowHeight = 24f;
        float totalHeight = titleHeight + (isCollapsed ? 0 : Rows.Count * (rowHeight + 2) + 10);

        return p.X >= Position.X && p.X <= Position.X + width &&
               p.Y >= Position.Y && p.Y <= Position.Y + totalHeight;
    }

    private RawRectangleF GetCollapseButtonBounds()
    {
        float buttonSize = 20;
        float margin = 6;
        float buttonTop = Position.Y + (titleHeight - buttonSize) / 2;

        return new RawRectangleF(
            Position.X + width - buttonSize - margin,
            buttonTop,
            Position.X + width - margin,
            buttonTop + buttonSize
        );
    }

    public void OnMouseDown(Point p)
    {
        if (HitTest(p)) // ✅ Fine for click-to-start
        {
            var pt = new RawVector2(p.X, p.Y);

            var bounds = GetCollapseButtonBounds();
            if (pt.X >= bounds.Left - 5 && pt.X <= bounds.Right + 5 &&
                pt.Y >= bounds.Top - 5 && pt.Y <= bounds.Bottom + 5)
            {
                isCollapsed = !isCollapsed;
                viewer.UpdateCursorPanel(); // <-- 🛠 Update immediately after expanding or collapsing
                return;
            }

            isDragging = true;
            dragOffset = new RawVector2(p.X - Position.X, p.Y - Position.Y);
        }

        // Allow edits ONLY when not collapsed
        if (!isCollapsed)
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                var pt = new RawVector2(p.X, p.Y);
                if (IsPointInRect(pt, value1Boxes[i]))
                {
                    StartEditing(i, 0);
                    return;
                }
                if (IsPointInRect(pt, value2Boxes[i]))
                {
                    StartEditing(i, 1);
                    return;
                }
            }
        }
    }

private bool IsPointInRect(RawVector2 pt, RawRectangleF rect)
    {
        return pt.X >= rect.Left && pt.X <= rect.Right &&
               pt.Y >= rect.Top && pt.Y <= rect.Bottom;
    }


    public void OnMouseMove(Point p)
    {
        if (isDragging)
        {
            Position = new RawVector2(p.X - dragOffset.X, p.Y - dragOffset.Y);
        }
    }

    public void OnMouseUp(Point p)
    {
        isDragging = false;
    }

    private void StartEditing(int row, int valIndex)
    {
        editingRowIndex = row;
        editingValueIndex = valIndex;
        editingText = "";
    }

    public void OnKeyPressed(Keys key, bool shift, bool ctrl)
    {
        if (editingRowIndex == null || editingValueIndex == null)
            return;

        if (key == Keys.Enter)
        {
            CommitEdit();
            viewer.Invalidate();
            return;
        }
        if (key == Keys.Escape)
        {
            editingRowIndex = editingValueIndex = null;
            editingText = "";
            viewer.Invalidate();
            return;
        }

        if (key == Keys.Back && editingText.Length > 0)
        {
            editingText = editingText.Substring(0, editingText.Length - 1);
            viewer.Invalidate();
        }
        else if ((key >= Keys.D0 && key <= Keys.D9) ||
         (key >= Keys.NumPad0 && key <= Keys.NumPad9) ||
         key == Keys.OemPeriod || key == Keys.Decimal ||
         key == Keys.Subtract || key == Keys.OemMinus)
        {
            if (key == Keys.OemPeriod || key == Keys.Decimal)
            {
                editingText += ".";
                viewer.Invalidate();
            }
            else if (key == Keys.Subtract || key == Keys.OemMinus)
            {
                editingText += "-";
                viewer.Invalidate();
            }
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                editingText += (char)key; // works for top row numbers
                viewer.Invalidate();
            }
            else if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                editingText += (char)('0' + (key - Keys.NumPad0));
                viewer.Invalidate();
            }
            else
            {
                editingText += ((char)key).ToString();
                viewer.Invalidate();
            }
        }
    }

    private void CommitEdit()
    {
        if (!float.TryParse(editingText, out float parsed)) return;

        var row = Rows[editingRowIndex.Value];
        int valIndex = editingValueIndex.Value;

        if (row.Label == "Time")
        {
            var ruler = valIndex == 0 ? viewer.TimeRulers[0] : viewer.TimeRulers[1];

            if (viewer.ShowPhaseRulers)
            {
                // Check if cursor is inside phase region
                float norm = ruler.Time;
                if (norm >= viewer.PhaseRuler1.Time && norm <= viewer.PhaseRuler2.Time)
                {
                    // User typed degrees --> convert degrees back to normalized time
                    float degrees = parsed;
                    float localNorm = degrees / 720f; // 720° = full cycle
                    ruler.Time = viewer.ClampTime(viewer.PhaseRuler1.Time + localNorm * (viewer.PhaseRuler2.Time - viewer.PhaseRuler1.Time));
                }
                else
                {
                    // Fall back to normal seconds
                    ruler.Time = viewer.ClampTime(parsed / viewer.CurrentFileTimespan);
                }
            }
            else
            {
                ruler.Time = viewer.ClampTime(parsed / viewer.CurrentFileTimespan);
            }
        }

        else
        {
            int channelIndex = viewer.Channels.FindIndex(c => c.Name == row.Label);
            if (channelIndex >= 0)
            {
                viewer.Channels[channelIndex].Rulers[valIndex].Voltage = parsed;
            }
        }

        editingRowIndex = editingValueIndex = null;
        editingText = "";
        viewer.UpdateCursorPanel();
        viewer.Invalidate();
    }


}

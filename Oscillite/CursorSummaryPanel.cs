using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Oscillite
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class CursorSummaryPanel : UserControl
    {
        private Panel titleBar;
        private Label titleLabel;
        private Button collapseButton;
        private TableLayoutPanel table;
        private bool isCollapsed = false;
        private Point dragOffset;
        private int expandedHeight = 150;

        public CursorSummaryPanel()
        {
            this.Size = new Size(250, expandedHeight);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            titleBar = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(50, 50, 50),
                Cursor = Cursors.SizeAll
            };

            titleLabel = new Label
            {
                Text = "Cursors",
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0),
                Width = 180,
                ForeColor = Color.White
            };
            titleLabel.MouseDown += TitleBar_MouseDown;
            titleLabel.MouseMove += TitleBar_MouseMove;

            collapseButton = new Button
            {
                Text = "—",
                Dock = DockStyle.Right,
                Width = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };
            collapseButton.FlatAppearance.BorderSize = 0;
            collapseButton.Click += (s, e) => ToggleCollapse();

            titleBar.Controls.Add(titleLabel);
            titleBar.Controls.Add(collapseButton);
            this.Controls.Add(titleBar);

            table = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                ColumnCount = 4,
                AutoSize = true,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Padding = new Padding(0, 5, 0, 0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            this.Controls.Add(table);

            // Enable drag
            titleBar.MouseDown += TitleBar_MouseDown;
            titleBar.MouseMove += TitleBar_MouseMove;
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragOffset = new Point(e.X, e.Y);
            }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.Location = new Point(this.Left + e.X - dragOffset.X, this.Top + e.Y - dragOffset.Y);
        }
        private Color GetColorForLabel(string label)
        {
            label = label.ToLower();

            if (label.Contains("blue")) return Color.DeepSkyBlue;
            if (label.Contains("red")) return Color.Red;
            if (label.Contains("green")) return Color.LimeGreen;
            if (label.Contains("yellow")) return Color.Gold;
            if (label.Contains("time")) return Color.White;

            return Color.White;
        }
        public void SetCursorValues(string label, float? val1, float? val2, string unit)
        {
            int row = -1;
            for (int i = 0; i < table.RowCount; i++)
            {
                if ((table.GetControlFromPosition(0, i) as Label)?.Text == label)
                {
                    row = i;
                    break;
                }
            }

            if (row == -1)
            {
                row = table.RowCount++;
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                table.Controls.Add(new Label { Text = label, ForeColor = Color.White, Anchor = AnchorStyles.Left }, 0, row);
                table.Controls.Add(new Label(), 1, row);
                table.Controls.Add(new Label(), 2, row);
                table.Controls.Add(new Label(), 3, row);
            }



            var c1 = table.GetControlFromPosition(1, row) as Label;
            var c2 = table.GetControlFromPosition(2, row) as Label;
            var delta = table.GetControlFromPosition(3, row) as Label;

            c1.Text = val1?.ToString("F2") ?? "—";
            c1.ForeColor = GetColorForLabel(label);
            c2.Text = val2?.ToString("F2") ?? "—";
            c2.ForeColor = GetColorForLabel(label);
            delta.Text = (val1.HasValue && val2.HasValue)
                ? Math.Abs(val2.Value - val1.Value).ToString("F2") + " " + unit
                : "—";
            delta.ForeColor = GetColorForLabel(label);
        }

        private void ToggleCollapse()
        {
            isCollapsed = !isCollapsed;
            table.Visible = !isCollapsed;
            this.Height = isCollapsed ? titleBar.Height : expandedHeight;
            collapseButton.Text = isCollapsed ? "+" : "—";
        }
    }

}

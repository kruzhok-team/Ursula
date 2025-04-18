using Godot;
using System;
using Ursula.Log.Model;

namespace Ursula.Log.View
{
	public partial class LogItemView : LogItemViewModel
    {
        [Export]
        Control imgWarning { get; set; }

        [Export]
        Control imgError { get; set; }

        [Export]
		Label LabelLog { get; set; }

		public void SetTextLog(string text, int type = 0)
		{
            imgWarning.Visible = type == 0;
            imgError.Visible = type == 1;
            LabelLog.Text = text;
            SetVisibleView(true);
        }

        public void SetVisibleView(bool value)
        {
            Visible = value;
        }

    }
}


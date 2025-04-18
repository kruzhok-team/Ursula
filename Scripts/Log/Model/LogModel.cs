using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using Ursula.Core.DI;


namespace Ursula.Log.Model
{
	public partial class LogModel : Control, IInjectable
    {
        public static int LOG_LINE_COUNT_VISIBLE = 20;
        public static int LOG_MAX_LINE_COUNT = 1000;

        public event EventHandler ViewVisible_EventHandler;
        public event EventHandler SetMessage_EventHandler;

        public bool isVisibleLog = true;

        private readonly Queue<string> _logQueue = new Queue<string>();

        void IInjectable.OnDependenciesInjected()
        {
        }

        public LogModel SetVisibleView(bool value)
        {
            Visible = value;
            isVisibleLog = value;
            InvokeMenuVisibleEvent();
            return this;
        }

        public LogModel SetLogMessage(string message)
        {
            _logQueue.Enqueue(message);

            if (_logQueue.Count > LOG_MAX_LINE_COUNT)
            {
                _logQueue.Dequeue();
            }

            InvokeSetShowMessageEvent();
            return this;
        }

        public LogModel SetClearLogs()
        {
            _logQueue.Clear();
            return this;
        }

        public string GetLogs()
        {
            return string.Join("\n", _logQueue);
        }

        private void InvokeMenuVisibleEvent()
        {
            var handler = ViewVisible_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeSetShowMessageEvent()
        {
            var handler = SetMessage_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}

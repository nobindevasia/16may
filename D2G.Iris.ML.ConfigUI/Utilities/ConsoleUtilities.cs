using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D2G.Iris.ML.ConfigUI.Utilities
{
    public static class ConsoleUtilities
    {
        public class TextBoxWriter : TextWriter
        {
            private readonly RichTextBox _textBox;
            private readonly SynchronizationContext _synchronizationContext;

            public TextBoxWriter(RichTextBox textBox)
            {
                _textBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
                _synchronizationContext = SynchronizationContext.Current
                    ?? throw new InvalidOperationException("SynchronizationContext.Current is null. This class must be instantiated on the UI thread.");
            }


            public override void Write(string value)
            {
                if (string.IsNullOrEmpty(value)) return;

                _synchronizationContext.Post(_ =>
                {
                    _textBox.AppendText(value);
                    _textBox.ScrollToCaret();
                }, null);
            }

            public override void WriteLine(string value)
            {
                Write(value + Environment.NewLine);
            }

            public override Encoding Encoding => Encoding.UTF8;
        }

        public static TextWriter RedirectConsoleOutput(RichTextBox textBox)
        {
            var originalConsoleOut = Console.Out;
            Console.SetOut(new TextBoxWriter(textBox));
            return originalConsoleOut;
        }


        public static void RestoreConsoleOutput(TextWriter originalOutput)
        {
            if (originalOutput != null)
            {
                Console.SetOut(originalOutput);
            }
        }

        public static void AppendText(RichTextBox textBox, string text, Color color)
        {
            if (textBox == null) throw new ArgumentNullException(nameof(textBox));
            if (text == null) return;

            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() => AppendText(textBox, text, color)));
                return;
            }

            textBox.SelectionStart = textBox.TextLength;
            textBox.SelectionLength = 0;
            textBox.SelectionColor = color;
            textBox.AppendText(text);
            textBox.SelectionColor = textBox.ForeColor;
            textBox.ScrollToCaret();
        }


        public static void ClearConsole(RichTextBox textBox)
        {
            if (textBox == null) throw new ArgumentNullException(nameof(textBox));

            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() => textBox.Clear()));
            }
            else
            {
                textBox.Clear();
            }
        }

        public static void LogMessage(RichTextBox textBox, string message, LogLevel logLevel = LogLevel.Info)
        {
            if (textBox == null) throw new ArgumentNullException(nameof(textBox));
            if (string.IsNullOrWhiteSpace(message)) return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string prefix = $"[{timestamp}] [{logLevel}] ";

            Color color = logLevel switch
            {
                LogLevel.Info => Color.Black,         
                LogLevel.Warning => Color.DarkOrange, 
                LogLevel.Error => Color.Red,          
                LogLevel.Success => Color.DarkGreen,  
                LogLevel.Debug => Color.Navy,        
                _ => Color.Black
            };

            AppendText(textBox, prefix + message + Environment.NewLine, color);
        }


        public static void AddSeparator(RichTextBox textBox, char character = '-', Color? color = null)
        {
            if (textBox == null) throw new ArgumentNullException(nameof(textBox));

            string separator = new string(character, 80) + Environment.NewLine;
            AppendText(textBox, separator, color ?? Color.DarkGray);
        }

        public static void AddSection(RichTextBox textBox, string title, string content,
            Color? titleColor = null, Color? contentColor = null)
        {
            if (textBox == null) throw new ArgumentNullException(nameof(textBox));

            AddSeparator(textBox);
            AppendText(textBox, $"[{title}]{Environment.NewLine}", titleColor ?? Color.White);

            var lines = content?.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                        ?? Array.Empty<string>();
            foreach (var line in lines)
            {
                AppendText(textBox, "  " + line + Environment.NewLine, contentColor ?? Color.White);
            }
        }

        public static async Task RunWithProgressAsync(RichTextBox textBox, Func<Task> task,
            string startMessage = "Operation started...",
            string completeMessage = "Operation completed successfully.",
            string errorMessage = "Operation failed:")
        {
            if (textBox == null) throw new ArgumentNullException(nameof(textBox));
            if (task == null) throw new ArgumentNullException(nameof(task));

            LogMessage(textBox, startMessage, LogLevel.Info);

            try
            {
                await task();
                LogMessage(textBox, completeMessage, LogLevel.Success);
            }
            catch (Exception ex)
            {
                LogMessage(textBox, $"{errorMessage} {ex.Message}", LogLevel.Error);
                if (ex.InnerException != null)
                {
                    LogMessage(textBox, $"Inner exception: {ex.InnerException.Message}", LogLevel.Error);
                }
                throw;
            }
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success,
        Debug
    }
}

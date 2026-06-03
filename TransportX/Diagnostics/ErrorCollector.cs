using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.SDL;

namespace TransportX.Diagnostics
{
    public class ErrorCollector : IErrorCollector
    {
        private static readonly MessageBox.Button OKButton = new("OK", MessageBoxButtonFlags.ReturnkeyDefault | MessageBoxButtonFlags.EscapekeyDefault);
        private static readonly MessageBox.Button DetailButton = new("詳細...", default);


        private readonly List<Error> ErrorsKey = [];

        public IReadOnlyList<Error> Errors => ErrorsKey;
        public bool HasFatalError => Errors.Any(error => error.Level == ErrorLevel.Fatal);

        public event EventHandler<ErrorEventArgs>? Reported;

        public ErrorCollector()
        {
        }

        public void Report(Error error)
        {
            ErrorsKey.Add(error);
            Reported?.Invoke(this, new ErrorEventArgs(error));

            ReadOnlySpan<MessageBox.Button> buttons = error.Exception is null ? [OKButton] : [OKButton, DetailButton];

            MessageBox.Show($"{error}\n\nスタックトレース:\n{error.Exception?.StackTrace ?? error.StackTrace.ToString()}",
                buttons, "読込中にエラーが発生しました", MessageBoxFlags.Error, out MessageBox.Button result);

            if (result == DetailButton) ReportInnerException(error.Exception!, 1);


            void ReportInnerException(Exception exception, int count)
            {
                MessageBox.Show(exception.ToString(), $"読込中にエラーが発生しました - 内部例外 {count}", MessageBoxFlags.Error);
                if (exception.InnerException is not null) ReportInnerException(exception.InnerException, count + 1);
            }
        }
    }
}

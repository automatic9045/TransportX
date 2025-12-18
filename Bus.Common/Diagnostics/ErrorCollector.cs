using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bus.Common.Diagnostics
{
    public class ErrorCollector : IErrorCollector
    {
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

            MessageBox.Show($"{error}\n\nスタックトレース:\n{error.Exception?.StackTrace ?? error.StackTrace.ToString()}",
                "読込中にエラーが発生しました", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransportX.Diagnostics
{
    public interface IErrorCollector
    {
        public static IErrorCollector Default() => new DefaultErrorCollector();


        IReadOnlyList<Error> Errors { get; }
        bool HasFatalError { get; }

        event EventHandler<ErrorEventArgs>? Reported;

        void Report(Error error);

        void ReportRange(IEnumerable<Error> errors)
        {
            foreach (Error error in errors) Report(error);
        }


        private sealed class DefaultErrorCollector : IErrorCollector
        {
            private readonly List<Error> ErrorsKey = [];

            public IReadOnlyList<Error> Errors => ErrorsKey;
            public bool HasFatalError => Errors.Any(error => error.Level == ErrorLevel.Fatal);

            public event EventHandler<ErrorEventArgs>? Reported;

            public DefaultErrorCollector()
            {
            }

            public void Report(Error error)
            {
                ErrorsKey.Add(error);
                Reported?.Invoke(this, new ErrorEventArgs(error));
            }
        }
    }
}

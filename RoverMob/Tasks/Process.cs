using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assisticant.Fields;

namespace RoverMob.Tasks
{
    public class Process
    {
        private AsyncSemaphore _lock = new AsyncSemaphore();
        private Observable<bool> _busy = new Observable<bool>();
        private Observable<Exception> _exception = new Observable<Exception>();

        public Exception Exception
        {
            get
            {
                lock (_exception)
                {
                    return _exception;
                }
            }
        }

        public bool Busy
        {
            get
            {
                lock (_busy)
                {
                    return _busy;
                }
            }
        }

        public Task JoinAsync()
        {
            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
            Perform(() => completion.SetResult(true));
            return completion.Task;
        }

        protected async void Perform(Func<Task> request)
        {
            await _lock.WaitAsync();
            try
            {
                lock (_busy)
                {
                    _busy.Value = true;
                }
                await request();
                lock (_exception)
                {
                    _exception.Value = null;
                }
            }
            catch (Exception x)
            {
                lock (_exception)
                {
                    _exception.Value = x;
                }
            }
            finally
            {
                _lock.Release();
                lock (_busy)
                {
                    _busy.Value = false;
                }
            }
        }

        protected void Perform(Action request)
        {
            Perform(() => Task.Run(request));
        }
    }
}

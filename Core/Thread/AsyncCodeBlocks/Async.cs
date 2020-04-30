using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Core
{
    /// <summary>
    /// Delegate that will be executed after completion of execution
    /// of the asynchronous code blocks
    /// </summary>
    /// <param name="objAsync"></param>
    public delegate void AsyncCodeBlockExecutionCompleteCallback(Async objAsync);

    /// <summary>
    /// Class for wrapping part of method code into an object
    /// that will be executed on Managed IOCP based ThreadPool.
    /// By default Async code blocks uses AppDomain wide Managed
    /// IOCP based ThreadPool. If required developers can specify
    /// a different instance of Managed IOCP ThreadPool
    /// </summary>
    public class Async
    {
        #region Public Constructor(s)

        /// <summary>
        /// Creates an instance of Async class, that executes
        /// the wrapped code block on the default AppDomain wide
        /// Managed IOCP based ThreadPool
        /// </summary>
        /// <param name="ad">
        /// Anonymous delegate wrapping the code block to execute
        /// </param>
        public Async(AsyncDelegate ad)
        {
            Initialize(ad, null, null, null);
        }

        /// <summary>
        /// Creates an instance of Async class, that executes
        /// the wrapped code block on the default AppDomain wide
        /// Managed IOCP based ThreadPool
        /// </summary>
        /// <param name="ad">
        /// Anonymous delegate wrapping the code block to execute
        /// </param>
        /// <param name="executionCompleteCallback">
        /// Delegate handler that will be called when the execution of the
        /// code block wrapped by this instance is completed. Dependent
        /// Async objects will be scheduled for execution after the
        /// completion callback has executed
        /// </param>
        public Async(AsyncDelegate ad, AsyncCodeBlockExecutionCompleteCallback executionCompleteCallback)
        {
            Initialize(ad, null, null, executionCompleteCallback);
        }

        /// <summary>
        /// Creates an instance of Async class, that executes
        /// the wrapped code block on the developer supplied
        /// Managed IOCP based ThreadPool
        /// </summary>
        /// <param name="ad">
        /// Anonymous delegate wrapping the code block to execute
        /// </param>
        /// <param name="tp">Managed IOCP based ThreadPool object</param>
        public Async(AsyncDelegate ad, ThreadPool tp)
        {
            Initialize(ad, tp, null, null);
        }

        /// <summary>
        /// Creates an instance of Async class, that executes
        /// the wrapped code block on the developer supplied
        /// Managed IOCP based ThreadPool
        /// </summary>
        /// <param name="ad">
        /// Anonymous delegate wrapping the code block to execute
        /// </param>
        /// <param name="tp">Managed IOCP based ThreadPool object</param>
        /// <param name="executionCompleteCallback">
        /// Delegate handler that will be called when the execution of the
        /// code block wrapped by this instance is completed. Dependent
        /// Async objects will be scheduled for execution after the
        /// completion callback has executed
        /// </param>
        public Async(AsyncDelegate ad, ThreadPool tp, AsyncCodeBlockExecutionCompleteCallback executionCompleteCallback)
        {
            Initialize(ad, tp, null, executionCompleteCallback);
        }

        /// <summary>
        /// Creates an instance of Async class, that executes
        /// the wrapped code block on the default AppDomain wide
        /// Managed IOCP based ThreadPool
        /// </summary>
        /// <param name="ad">
        /// Anonymous delegate wrapping the code block to execute
        /// </param>
        /// <param name="dependentOnAsync">
        /// Async object on which the current instance of Async 
        /// depends on. The code wrapped by the current instance 
        /// of Async object will be executed after the code wrapped 
        /// by dependentOnAsync object has completed execution
        /// </param>
        public Async(AsyncDelegate ad, Async dependentOnAsync)
        {
            Initialize(ad, null, new Async[] { dependentOnAsync }, null);
        }

        /// <summary>
        /// Creates an instance of Async class, that executes
        /// the wrapped code block on the default AppDomain wide
        /// Managed IOCP based ThreadPool
        /// </summary>
        /// <param name="ad">
        /// Anonymous delegate wrapping the code block to execute
        /// </param>
        /// <param name="dependentOnAsync">
        /// Async object on which the current instance of Async 
        /// depends on. The code wrapped by the current instance 
        /// of Async object will be executed after the code wrapped 
        /// by dependentOnAsync object has completed execution
        /// </param>
        /// <param name="executionCompleteCallback">
        /// Delegate handler that will be called when the execution of the
        /// code block wrapped by this instance is completed. Dependent
        /// Async objects will be scheduled for execution after the
        /// completion callback has executed
        /// </param>
        public Async(AsyncDelegate ad, Async dependentOnAsync, AsyncCodeBlockExecutionCompleteCallback executionCompleteCallback)
        {
            Initialize(ad, null, new Async[] { dependentOnAsync }, executionCompleteCallback);
        }

        /// <summary>
        /// Creates an instance of Async class, that executes
        /// the wrapped code block on the developer supplied
        /// Managed IOCP based ThreadPool
        /// </summary>
        /// <param name="ad">
        /// Anonymous delegate wrapping the code block to execute
        /// </param>
        /// <param name="tp">Managed IOCP based ThreadPool object</param>
        /// <param name="dependentOnAsync">
        /// Async object on which the current instance of Async 
        /// depends on. The code wrapped by the current instance 
        /// of Async object will be executed after the code wrapped 
        /// by dependentOnAsync object has completed execution
        /// </param>
        public Async(AsyncDelegate ad, ThreadPool tp, Async dependentOnAsync)
        {
            Initialize(ad, tp, new Async[] { dependentOnAsync }, null);
        }

        /// <summary>
        /// Creates an instance of Async class, that executes
        /// the wrapped code block on the developer supplied
        /// Managed IOCP based ThreadPool
        /// </summary>
        /// <param name="ad">
        /// Anonymous delegate wrapping the code block to execute
        /// </param>
        /// <param name="tp">Managed IOCP based ThreadPool object</param>
        /// <param name="dependentOnAsync">
        /// Async object on which the current instance of Async 
        /// depends on. The code wrapped by the current instance 
        /// of Async object will be executed after the code wrapped 
        /// by dependentOnAsync object has completed execution
        /// </param>
        /// <param name="executionCompleteCallback">
        /// Delegate handler that will be called when the execution of the
        /// code block wrapped by this instance is completed. Dependent
        /// Async objects will be scheduled for execution after the
        /// completion callback has executed
        /// </param>
        public Async(AsyncDelegate ad, ThreadPool tp, Async dependentOnAsync, AsyncCodeBlockExecutionCompleteCallback executionCompleteCallback)
        {
            Initialize(ad, tp, new Async[] { dependentOnAsync }, executionCompleteCallback);
        }

        /// <summary>
        /// Creates an instance of Async class, that executes
        /// the wrapped code block on the developer supplied
        /// Managed IOCP based ThreadPool
        /// </summary>
        /// <param name="ad">
        /// Anonymous delegate wrapping the code block to execute
        /// </param>
        /// <param name="tp">Managed IOCP based ThreadPool object</param>
        /// Async object array on which the current instance of Async 
        /// depends on. The code wrapped by the current instance 
        /// of Async object will be executed after the code wrapped 
        /// by dependentOnAsync object array has completed execution
        /// </param>
        public Async(AsyncDelegate ad, ThreadPool tp, Async[] arrDependentOnAsync)
        {
            Initialize(ad, tp, arrDependentOnAsync, null);
        }

        /// <summary>
        /// Creates an instance of Async class, that executes
        /// the wrapped code block on the developer supplied
        /// Managed IOCP based ThreadPool
        /// </summary>
        /// <param name="ad">
        /// Anonymous delegate wrapping the code block to execute
        /// </param>
        /// <param name="tp">Managed IOCP based ThreadPool object</param>
        /// Async object array on which the current instance of Async 
        /// depends on. The code wrapped by the current instance 
        /// of Async object will be executed after the code wrapped 
        /// by dependentOnAsync object array has completed execution
        /// </param>
        /// <param name="executionCompleteCallback">
        /// Delegate handler that will be called when the execution of the
        /// code block wrapped by this instance is completed. Dependent
        /// Async objects will be scheduled for execution after the
        /// completion callback has executed
        /// </param>
        public Async(AsyncDelegate ad, ThreadPool tp, Async[] arrDependentOnAsync, AsyncCodeBlockExecutionCompleteCallback executionCompleteCallback)
        {
            Initialize(ad, tp, arrDependentOnAsync, executionCompleteCallback);
        }

        #endregion
        #region Public Methods

        /// <summary>
        /// Returns the value of any local variable used within
        /// the code wrapped by this Async object
        /// </summary>
        /// <param name="name">Name of the local variable</param>
        /// <returns>Value of the local variable</returns>
        public object GetObject(string name)
        {
            object obj = _targetType.InvokeMember(name, BindingFlags.GetField, null, _targetObject, null);
            return obj;
        }

        /// <summary>
        /// Executes the given AsyncDelegate in the SynchronizationContext associated with this
        /// instance of Async class
        /// </summary>
        /// <param name="ad">AsyncDelegate object</param>
        public void ExecuteInSychronizationContext(AsyncDelegate ad)
        {
            if (_synchronizationContext != null)
            {
                _synchronizationContext.Send(
                    Delegate.CreateDelegate(typeof(SendOrPostCallback), ad.Method) as SendOrPostCallback, null);
            }
            else
            {
                throw new InvalidOperationException("SynchronizationContext object is not available to execute the supplied code");
            }
        }

        #endregion
        #region Public Virtual Methods

        /// <summary>
        /// Calling code cannot wait for code execution completion
        /// that is wrapped by Async objects
        /// </summary>
        /// <param name="msWaitTime"></param>
        /// <returns></returns>
        public virtual bool Wait(int msWaitTime)
        {
            return false;
        }

        /// <summary>
        /// Returns the Exception object if the execution of the code 
        /// wrapped by this Async object threw any exception
        /// </summary>
        public virtual Exception CodeException
        {
            get
            {
                Exception ex = null;
                AsyncDelegateTask adt = _task as AsyncDelegateTask;
                if (adt != null)
                {
                    if (adt.CodeException != null)
                        ex = adt.CodeException;
                }
                return ex;
            }
        }

        #endregion
        #region Internal Methods

        internal bool AddToDependencyCodeBlockList(Async asyncObj)
        {
            bool added = false;
            lock (_dependentCodeBlockList)
            {
                if (_executionCompleted == false)
                {
                    _dependentCodeBlockList.Add(asyncObj);
                    added = true;
                }
            }
            return added;
        }

        internal void MarkCompleted()
        {
            // Execute the execution completion callback
            //
            if (_executionCompleteCallback != null) _executionCompleteCallback(this);

            // Schedule execution of dependent Async objects
            //
            lock (_dependentCodeBlockList)
            {
                _executionCompleted = true;
                // Dispatch all dependent Async code blocks for
                // execution
                //
                if (_dependentCodeBlockList.Count > 0)
                {
                    foreach (Async asyncObj in _dependentCodeBlockList)
                    {
                        asyncObj.ExecuteSelf();
                    }
                    // Release our references to the dependent Async objects
                    //
                    _dependentCodeBlockList.Clear();
                }
            }
        }

        #endregion
        #region Protected Virtual Methods

        protected virtual void Initialize(AsyncDelegate ad, ThreadPool tp, Async[] arrDependentOnAsync,
            AsyncCodeBlockExecutionCompleteCallback executionCompleteCallback)
        {
            Initialize(ad, tp, arrDependentOnAsync, executionCompleteCallback, false);
        }

        #endregion
        #region Protected Methods

        protected void Initialize(AsyncDelegate ad, ThreadPool tp, Async[] arrDependentOnAsync, AsyncCodeBlockExecutionCompleteCallback executionCompleteCallback, bool waitable)
        {
            if (ad != null)
            {
                _waitable = waitable;
                _ad = ad;
                _tp = tp;
                _targetObject = ad.Target;
                _targetType = ad.Method.DeclaringType;
                if (_waitable == false)
                {
                    AsyncDelegateTask adt = new AsyncDelegateTask(ad);
                    adt.TaskCompleted = this.MarkCompleted;
                    _task = adt;
                }
                else
                {
                    WaitableAsyncDelegateTask wadt = new WaitableAsyncDelegateTask(ad);
                    wadt.TaskCompleted = this.MarkCompleted;
                    _task = wadt;
                }
                _executionCompleteCallback = executionCompleteCallback;
                bool dispatchForExecution = true;
                if (arrDependentOnAsync != null)
                {
                    lock (_syncObject)
                    {
                        foreach (Async asyncObj in arrDependentOnAsync)
                        {
                            if (asyncObj.AddToDependencyCodeBlockList(this) == true) _dependentCount++;
                        }
                        if (_dependentCount == 0)
                            dispatchForExecution = true;
                        else
                            dispatchForExecution = false;
                    }
                }
                // Store the current SynchronizationContext
                //
                _synchronizationContext = SynchronizationContext.Current;
                if (dispatchForExecution == true)
                {
                    Execute(ad, _task, tp);
                }
            }
        }

        #endregion
        #region Private Methods

        private void ExecuteSelf()
        {
            lock (_syncObject)
            {
                if ((_dependentCount > 0) && (--_dependentCount == 0))
                {
                    Execute(_ad, _task, _tp);
                }
            }
        }

        #endregion
        #region Public Properties

        /// <summary>
        /// Identifies whether the calling code wait on this
        /// Async object until the code wrapped by it executed.
        /// true: Can wait, false: Cannot wait.
        /// For Async objects this property always returns 'false'
        /// </summary>
        public bool Waitable
        {
            get { return _waitable; }
        }

        public bool Completed
        {
            get { return _executionCompleted; }
        }

        /// <summary>
        /// Gets the SynchronizationContext associated with this instance of the
        /// Async class
        /// </summary>
        public SynchronizationContext SynchronizationContext
        {
            get { return _synchronizationContext; }
        }

        #endregion
        #region Public Static Constructor

        static Async()
        {
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);
        }

        #endregion
        #region Private Static Methods

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
        #region Public Static Methods

        /// <summary>
        /// Initializes the AppDomain wide ManagedIOCP ThreadPool
        /// used by Async object execution
        /// </summary>
        public static void Open()
        {
            lock (typeof(Async))
            {
                if (s_AsyncCodeDelegateTP == null)
                {
                    s_AsyncCodeDelegateTP = new ThreadPool(1, 1);
                }
            }
        }

        /// <summary>
        /// Closes the AppDomain wide ManagedIOCP ThreadPool
        /// used by Async object execution
        /// </summary>
        public static void Close()
        {
            lock (typeof(Async))
            {
                if (s_AsyncCodeDelegateTP != null)
                {
                    s_AsyncCodeDelegateTP.Close();
                    s_AsyncCodeDelegateTP = null;
                }
            }
        }

        public static ThreadPool DefaultThreadPool
        {
            get
            {
                return s_AsyncCodeDelegateTP;
            }
        }
        #endregion
        #region Private Static Methods

        private static void Execute(AsyncDelegate ad, ITask task, ThreadPool tp)
        {
            if (s_AsyncCodeDelegateTP != null)
            {
                if (tp == null)
                {
                    s_AsyncCodeDelegateTP.Dispatch(task);
                }
                else
                {
                    tp.Dispatch(task);
                }
            }
            else
            {
                throw new ApplicationException("Thread Pool used by AsynchronousCodeBlock class is closed. " +
                    "Cannot execute any more asynchronous code blocks on default Thread pool. Please open the " +
                    "Thread Pool used by AsynchronousCodeBlock class or supply your own Thread Pool object for " +
                    "asynchronous code block");
            }
        }

        #endregion
        #region Protected Data Members

        protected ITask _task = null;
        protected bool _waitable = false;
        protected AsyncDelegate _ad = null;
        protected ThreadPool _tp = null;
        protected Type _targetType = null;
        protected object _targetObject = null;
        protected volatile bool _executionCompleted = false;
        protected List<Async> _dependentCodeBlockList = new List<Async>();
        protected object _syncObject = new object();
        protected volatile int _dependentCount = 0;
        protected AsyncCodeBlockExecutionCompleteCallback _executionCompleteCallback = null;
        protected SynchronizationContext _synchronizationContext = null;

        #endregion
        #region Private Static Data Members

        private static ThreadPool s_AsyncCodeDelegateTP = new ThreadPool(1, 1);

        #endregion
    }
}
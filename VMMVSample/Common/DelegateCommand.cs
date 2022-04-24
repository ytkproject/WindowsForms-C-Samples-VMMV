using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VMMVSample.Common
{
    /// <summary>
    /// DelegateCommand
    /// </summary>
    /// <typeparam name="T">Execute デリゲートと CanExecute デリゲートのパラメーターの型</typeparam>
    internal sealed class DelegateCommand<T> : ICommand
    {
        /// <summary>Execute デリゲート</summary>
        private Action<T> _execute;
        /// <summary>CanExecute デリゲート</summary>
        private Func<T, bool> _canExecute;
        
        /// <summary>
        /// [T]が値型か
        /// </summary>
        private bool IsValueType { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="execute">Execute デリゲート</param>
        internal DelegateCommand(Action<T> execute) : this(execute, x => true) { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="execute">Execute デリゲート</param>
        /// <param name="canExecute">CanExecute デリゲート</param>
        internal DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            this.IsValueType = typeof(T).IsValueType;

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// [CanExecute] 変更通知
        /// </summary>
        public event EventHandler CanExecuteChanged;
        /// <summary>
        /// [CanExecute] 変更通知
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// [Execute] が実行可能か
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(Cast(parameter)) ?? false;
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            _execute.Invoke(Cast(parameter));
        }

        /// <summary>
        /// convert parameter value
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private T Cast(object parameter)
        {
            var result = default(T);
            if (parameter == null && this.IsValueType)
            {
                // parameterがNullかつ値型の場合
                result = default(T);
            } 
            else
            {
                // 型変換
                result = (T)parameter;
            }
            return result;
        }
    }
}

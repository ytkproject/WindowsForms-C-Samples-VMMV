using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VMMVSample.Common
{
    /// <summary>
    /// BindableBase
    /// </summary>
    internal abstract class BindableBase : INotifyPropertyChanged, IDataErrorInfo, IDisposable
    {
        #region デストラクタ
        /// <summary>
        /// デストラクタ
        /// </summary>
        ~BindableBase()
        {
            this.OnDispose(false);
        }
        #endregion
        #region IDisposableメンバ
        /// <summary>
        /// 解放済みかどうか
        /// </summary>
        public bool IsDisposed
        {
            get; protected set;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.OnDispose(true);
            }
            catch
            {
                // Dispose時の例外は無視
            }
            return;
        }

        /// <summary>
        /// Dispose処理
        /// </summary>
        /// <param name="disposing">false:アンマネージドリソースのみ解放</param>
        protected virtual void OnDispose(bool disposing)
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;
            if (disposing)
            {
                if (this.PropertyChanged != null)
                {
                    foreach (PropertyChangedEventHandler h in this.PropertyChanged.GetInvocationList())
                    {
                        this.PropertyChanged -= h;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// プロパティの変更を通知するためのマルチキャスト イベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ値が変更された場合のみ、プロパティを設定し、リスナーに通知します
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="storage">get アクセス操作子と set アクセス操作子両方を使用したプロパティへの参照</param>
        /// <param name="value">プロパティに必要な値</param>
        /// <param name="propertyName">
        /// リスナーに通知するために使用するプロパティの名前
        /// この値は省略可能で、CallerMemberName をサポートするコンパイラから呼び出す場合に自動的に指定できます
        /// </param>
        /// <returns>値が変更された場合は true、既存の値が目的の値に一致した場合は false です</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) { 
                return false; 
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// プロパティ値が変更されたことをリスナーに通知します。
        /// </summary>
        /// <param name="propertyName">リスナーに通知するために使用するプロパティの名前。
        /// この値は省略可能で、
        /// <see cref="CallerMemberNameAttribute"/> をサポートするコンパイラから呼び出す場合に自動的に指定できます。</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// プロパティの変更を通知するためのマルチキャスト イベント。
        /// </summary>
        public event CancelEventHandler Validating;
        /// <summary>
        /// プロパティ値が変更されたことをリスナーに通知します。
        /// </summary>
        /// <param name="propertyName">リスナーに通知するために使用するプロパティの名前。
        /// この値は省略可能で、
        /// <see cref="CallerMemberNameAttribute"/> をサポートするコンパイラから呼び出す場合に自動的に指定できます。</param>
        protected bool OnValidating(string value, [CallerMemberName] string propertyName = null)
        {
            var result = new CancelEventArgs(false);
            
            this.Validating?.Invoke(this, result);
            return !result.Cancel;
        }



        /// <summary>
        /// エラー内容
        /// </summary>
        public string Error { get; }
        protected readonly Dictionary<string, string> _errors = new Dictionary<string, string>();
        public string this[string propertyName]
        {
            get
            {
                return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
            }
        }
        protected void UpdateErrors(string name, object value)
        {
            try
            {
                var v = new ValidationContext(this, null, null);
                v.MemberName = name;
                Validator.ValidateProperty(value, v);
                _errors.Remove(name);
            }
            catch (ValidationException ex)
            {
                _errors[name] = ex.Message;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Error"));
            }
        }
    }
}

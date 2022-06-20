using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace VMMVSample
{
    /// <summary> 
    /// $tmpversion$ 
    /// 汎用Conditionクラス 
    /// </summary> 
    /// <typeparam name="TSource">型</typeparam> 
    /// <remarks> 
    /// ※シリアライズ不可能な参照型の場合は「Value==SavedValue」になる(独自クラスのインスタンスによるプロパティ等) 
    /// ※検索時の状態で等、特定の状態での値を使用する場合は、Saveで値を保存しSavedValueで値を取得する 
    /// </remarks> 
    internal class ConditionBase<TSource> 
    { 
        #region Sub Class 
        #endregion 
        /// <summary> 
        /// 値格納クラス 
        /// </summary> 
        internal class Item : VMMVSample.Common.BindableBase, INotifyPropertyChanged
        {
            #region  
            #endregion
            /// <summary> 
            /// コンストラクタ 
            /// </summary> 
            /// <param name="name">値名(Property変更イベントで通知される名前)</param> 
            /// <param name="ev">Property変更イベント</param> 
            internal Item(in string name, in PropertyChangedEventHandler ev = null)
            {
                this.Name = name;
                if (ev != null)
                {
                    this.PropertyChanged += ev;
                }
            }

            #region Local variable 
            #endregion
            private bool _isInitializeValue = false;

            #region event 
            #endregion

            #region property 
            #endregion
            /// <summary> 
            /// 名称 
            /// </summary> 
            internal string Name { get; }

            private object _value;

            /// <summary> 
            /// 現在値 
            /// </summary> 
            public object Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    if (!_isInitializeValue)
                    {
                        // 初期値の為イベントはなし 
                        _isInitializeValue = true;
                        _value = value;
                    }
                    else if ((_value == null ^ value == null) || (_value != null && !_value.Equals(value)))
                    {
                        this.PreviousValue = GetValue(_value);
                        _value = GetValue(value);
                    }
                }
            }

            /// <summary> 
            /// 直前値 
            /// </summary> 
            internal object PreviousValue { get; private set; }

            /// <summary> 
            /// 保存値 
            /// </summary> 
            internal object SavedValue { get; private set; }

            #region internal method 
            #endregion
            /// <summary> 
            /// 現在の値保存 
            /// </summary> 
            internal void Save()
            {
                this.SavedValue = GetValue(this.Value);
            }

            #region private method 
            #endregion
            private object GetValue(in object value)
            {
                if (value != null && !value.GetType().IsValueType && value.GetType().IsSerializable)
                {
                    //*** 参照型かつシリアライズ可能な場合、シリアライズ後デシリアライズでインスタンスをコピーして返す 
                    using (var ms = new MemoryStream())
                    {
                        var bf = new BinaryFormatter();

                        bf.Serialize(ms, value);
                        ms.Position = 0;
                        return bf.Deserialize(ms);
                    }
                }
                else
                {
                    //*** 値型またはシリアライズ不可能な参照型の場合そのまま返す 
                    return value;
                }
            }
        }

        #region property 
        #endregion
        /// <summary> 
        /// バインド対象項目 
        /// </summary> 
        internal ConcurrentDictionary<string, Item> Items { get; } = new ConcurrentDictionary<string, Item>();

        #region internal method 
        #endregion
        /// <summary> 
        /// 全項目現在の値保存 
        /// </summary> 
        internal void Save()
        {
            this.Items.AsParallel().ForAll(x => x.Value.Save());
        }

        /// <summary> 
        /// バインド対象項目に対象が存在しない場合は追加し、値格納情報を返す 
        /// </summary> 
        /// <param name="expression">フルネームを取得する内容(ラムダ式：() =&gt; this.textBox1.Text ⇒ &quot;textBox1.Text&quot;)</param> 
        /// <param name="includeOwner">オーナー部を含めるかどうか</param> 
        /// <param name="value">値</param> 
        /// <param name="ev">イベント</param> 
        /// <returns>値格納情報</returns> 
        internal Item GetOrAdd(in string key, object value, PropertyChangedEventHandler ev = null)
        {
            return this.Items.GetOrAdd(key, x => new Item(x, ev) { Value = value });
        }

        /// <summary> 
        /// バインド対象項目に対象が存在しない場合は追加し、値格納情報を返す 
        /// </summary> 
        /// <param name="expression">フルネームを取得する内容(ラムダ式：() =&gt; this.textBox1.Text ⇒ &quot;textBox1.Text&quot;)</param> 
        /// <param name="includeOwner">オーナー部を含めるかどうか</param> 
        /// <param name="value">値</param> 
        /// <param name="ev">イベント</param> 
        /// <returns>値格納情報</returns> 
        internal Item GetOrAdd(in Expression<Func<object>> expression, in object value, in PropertyChangedEventHandler ev = null, in bool includeOwner = false)
        {
            var name = FullNameOf(expression, includeOwner);

            return GetOrAdd(name, value, ev);
        }

        /// <summary> 
        /// バインド対象項目に対象が存在しない場合は追加し、値格納情報を返す 
        /// </summary> 
        /// <param name="expression">紐付け情報(ラムダ式：x => x.textBox1.Text)</param> 
        /// <param name="value">値</param> 
        /// <param name="ev">イベント</param> 
        /// <returns>値格納情報</returns> 
        internal Item GetOrAdd(in Expression<Func<TSource, object>> expression, object value, PropertyChangedEventHandler ev = null)
        {
            var name = FullNameOf(expression);

            return this.Items.GetOrAdd(name, x => new Item(x, ev) { Value = value });
        }

        /// <summary> 
        /// 値格納情報取得 
        /// </summary> 
        /// <param name="key">取得項目</param> 
        /// <returns>値格納情報</returns> 
        internal Item GetItem(in string key)
        {
            this.Items.TryGetValue(key, out var result);

            return result;
        }

        /// <summary> 
        /// 値格納情報取得 
        /// </summary> 
        /// <param name="expression">フルネームを取得する内容(ラムダ式：() =&gt; this.textBox1.Text ⇒ &quot;textBox1.Text&quot;)</param> 
        /// <param name="includeOwner">オーナー部を含めるかどうか</param> 
        /// <returns>値格納情報</returns> 
        internal Item GetItem(in Expression<Func<object>> expression, in bool includeOwner = false)
        {
            var key = FullNameOf(expression, includeOwner);

            return GetItem(key);
        }

        /// <summary> 
        /// 値格納情報取得 
        /// </summary> 
        /// <param name="expression">紐付け情報(ラムダ式：x => x.textBox1.Text)</param> 
        /// <returns>値格納情報</returns> 
        internal Item GetItem(in Expression<Func<TSource, object>> expression)
        {
            var key = FullNameOf(expression);

            return GetItem(key);
        }

        /// <summary> 
        /// 現在値取得 
        /// </summary> 
        /// <param name="key">取得項目</param> 
        /// <returns>現在値</returns> 
        internal object GetValue(in string key)
        {
            var result = GetItem(key);

            return result?.Value;
        }

        /// <summary> 
        /// 現在値取得 
        /// </summary> 
        /// <param name="expression">フルネームを取得する内容(ラムダ式：() =&gt; this.textBox1.Text ⇒ &quot;textBox1.Text&quot;)</param> 
        /// <param name="includeOwner">オーナー部を含めるかどうか</param> 
        /// <returns>現在値</returns> 
        internal object GetValue(in Expression<Func<object>> expression, in bool includeOwner = false)
        {
            var result = GetItem(expression, includeOwner);

            return result?.Value;
        }

        /// <summary> 
        /// 現在値取得 
        /// </summary> 
        /// <param name="expression">紐付け情報(ラムダ式：x => x.textBox1.Text)</param> 
        /// <returns>現在値</returns> 
        internal object GetValue(in Expression<Func<TSource, object>> expression)
        {
            var result = GetItem(expression);

            return result?.Value;
        }

        /// <summary> 
        /// フルネームを取得します 
        /// </summary> 
        /// <param name="expression">フルネームを取得する内容(ラムダ式：x =&gt; x.textBox1.Text ⇒ & quot;textBox1.Text& quot;)</param> 
        /// <param name="includeOwner">オーナー部を含めるかどうか</param> 
        /// <returns>フルネーム</returns> 
        /// <remarks> 
        /// ex)Form1.textBox.Textを取得する場合 ⇒ FullNameOf(() =&gt; this.textBox1.Text)       ⇒ textBox1.Text 
        ///                                     ⇒ FullNameOf(() =&gt; this.textBox1.Text, true) ⇒ Form1.textBox1.Text 
        /// </remarks> 
        internal string FullNameOf(in Expression<Func<object>> expression, in bool includeOwner = false)
        {
            var memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
            {
                if (expression.Body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
                {
                    memberExpression = unaryExpression.Operand as MemberExpression;
                }
                else
                {
                    return null;
                }
            }

            var result = memberExpression.ToString();
            var idx = result.LastIndexOf(' ') + 1;
            if (includeOwner)
            {
                return result.Substring(idx);
            }
            else
            {
                return result.Substring(result.IndexOf('.', idx) + 1);
            }
        }

        /// <summary> 
        /// フルネームを取得します 
        /// </summary> 
        /// <param name="expression">フルネームを取得する内容(ラムダ式：x =&gt; x.textBox1.Text ⇒ & quot;textBox1.Text& quot;)</param> 
        /// <returns>フルネーム</returns> 
        /// <remarks> 
        /// ex)Form1.textBox.Textを取得する場合 ⇒ FullNameOf&lt;Form1&gt;(x =&gt; x.textBox1.Text) ⇒ textBox1.Text 
        /// </remarks> 
        internal string FullNameOf(in Expression<Func<TSource, object>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
            {
                if (expression.Body is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
                {
                    memberExpression = unaryExpression.Operand as MemberExpression;
                }
                else
                {
                    return null;
                }
            }

            var result = memberExpression.ToString();
            if (result.Contains(' '))
            {
                result = result.Substring(result.IndexOf(' ') + 1);
            }

            return result.Substring(result.IndexOf('.') + 1);
        }

        #region public method 
        #endregion
        /// <summary> 
        /// インスタンスの文字列 
        /// </summary> 
        public override string ToString()
        {
            var result = new System.Text.StringBuilder();
            var line = new string('-', 50);

            result.AppendLine(line);

            this.Items.ToList().ForEach(x => result.AppendLine($"{x.Key}:現在値={x.Value.Value} / 保存値={x.Value.SavedValue}"));

            result.AppendLine(line);

            return result.ToString();
        }
    } 
} 

 
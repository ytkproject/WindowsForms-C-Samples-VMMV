using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VMMVSample.Common
{
    internal abstract class ModelBase<T>
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected ModelBase()
        {
            // 各フィールド・プロパティをNonPublic含めて初期化
            this.Clear(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
                     , BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// 初期化各フィールド,プロパティをdefaultで初期化する
        /// </summary>
        /// <param name="fieldBindFlg">フィールド初期化対象：規定値Public|Instance</param>
        /// <param name="propertyBindFlg">プロパティ初期化対象：規定値Public|Instance</param>
        internal void Clear(BindingFlags fieldBindFlg = BindingFlags.Public | BindingFlags.Instance
                          , BindingFlags propertyBindFlg = BindingFlags.Public | BindingFlags.Instance)
        {
            U getValue<U>(U type) => default(U);

            foreach (var fld in typeof(T).GetFields(fieldBindFlg))
            {
                fld.SetValue(this, getValue(fld.FieldType));
            }
            foreach (var property in typeof(T).GetProperties(propertyBindFlg))
            {
                property.SetValue(this, getValue(property.PropertyType));
            }
        }

        /// <summary>
        /// フィールド値設定
        /// </summary>
        /// <param name="key">フィールド名</param>
        /// <param name="data">値</param>
        internal void SetField<U>(string key, U data)
        {
            // 変数名をキーにprivateのメンバ変数を取得
            var fieldInfo = this.GetType().GetField(key, BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(this, data);
        }

        /// <summary>
        /// プロパティ値設定
        /// </summary>
        /// <param name="key">プロパティ名</param>
        /// <param name="data">値</param>
        internal void SetProperty<U>(string key, U data)
        {
            // プロパティ名をキーにプロパティを取得
            var propertyInfo = this.GetType().GetProperty(key, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            propertyInfo.SetValue(this, data, null);
        }
    }
}

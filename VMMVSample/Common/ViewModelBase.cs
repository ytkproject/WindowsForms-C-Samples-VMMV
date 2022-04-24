using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VMMVSample.Common
{
    internal abstract class ViewModelBase<MODEL> : BindableBase where MODEL : class, new()
    {
        /// <summary>
        /// Modelのインスタンス
        /// </summary>
        protected MODEL Model { get; private set; }

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ViewModelBase()
        {
            this.Model = new MODEL();
        }
        #endregion
    }
}

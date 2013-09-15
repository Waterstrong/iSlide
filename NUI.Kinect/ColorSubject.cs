using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUI.Data;

namespace NUI.Kinect
{
    /// <summary>
    /// 图像数据处理通知者，观察者模式
    /// </summary>
    public class ColorSubject
    {
        public delegate void DataBindingEventHandler(ColorData data);
        public event DataBindingEventHandler DataBinding;

        ColorData _data = new ColorData();
        public void Notify(byte[] pixels, int width, int height)
        {
            if (DataBinding != null)
            {
                _data.SetImage(pixels, width, height);
                DataBinding(_data);
            }
        }
    }
}

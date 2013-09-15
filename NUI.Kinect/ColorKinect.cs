using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;
using NUI.Data;

namespace NUI.Kinect
{
    /// <summary>
    /// 图像处理类
    /// </summary>
    public class ColorKinect : KinectSuper
    {
        protected ColorSubject _subject = new ColorSubject();
        public NUI.Kinect.ColorSubject Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        /// <summary>
        /// 开始读取图像信息
        /// </summary>
        protected override void StartKinectFrame()
        {
            if (_nui != null)
            {
                _nui.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                _nui.ColorFrameReady += new EventHandler<Microsoft.Kinect.ColorImageFrameReadyEventArgs>(_nui_ColorFrameReady);
            }
        }
        /// <summary>
        /// 处理每一帧
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _nui_ColorFrameReady(object sender, Microsoft.Kinect.ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame == null)
                {
                    return;
                }
                byte[] pixels = new byte[frame.PixelDataLength];
                frame.CopyPixelDataTo(pixels);
                // 通知对象进行处理
                _subject.Notify(pixels, frame.Width, frame.Height);
            }
        }
        /// <summary>
        /// 停止读取
        /// </summary>
        protected override void StopKinectFrame()
        {
            if (_nui != null)
            {
                _nui.ColorFrameReady -= new EventHandler<Microsoft.Kinect.ColorImageFrameReadyEventArgs>(_nui_ColorFrameReady);
                _nui.ColorStream.Disable();
            }
        }
    }
}

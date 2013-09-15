using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace NUI.Kinect
{
    /// <summary>
    /// 深度数据处理
    /// </summary>
    public class DepthKinect : KinectSuper
    {
        const float MaxDepthDistance = 4095; // max value returned
        const float MinDepthDistance = 850; // min value returned
        const float MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;
        ColorSubject _subject = new ColorSubject();
        public NUI.Kinect.ColorSubject Subject
        {
            get { return _subject; }
        }
        /// <summary>
        /// 开始读取深度数据
        /// </summary>
        protected override void StartKinectFrame()
        {
            if (_nui != null)
            {
                _nui.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                _nui.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(_nui_DepthFrameReady);
            }
        }
        /// <summary>
        /// 停止读取深度数据
        /// </summary>
        protected override void StopKinectFrame()
        {
            if (_nui != null)
            {
                _nui.DepthFrameReady -= new EventHandler<DepthImageFrameReadyEventArgs>(_nui_DepthFrameReady);
                _nui.DepthStream.Disable();
            }
        }

        /// <summary>
        /// 处理每一帧
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _nui_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                {
                    return;
                }
                byte[] pixels = GenerateColoredBytes(frame);
                // 通知对象开始处理
                _subject.Notify(pixels, frame.Width, frame.Height);
            }
        }
        /// <summary>
        /// 生成深度信息图像
        /// </summary>
        /// <param name="depthFrame">传入深入帧</param>
        /// <returns>返回深度图像像素集</returns>
        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);
            byte[] pixels = new byte[depthFrame.Width * depthFrame.Height * 4];
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;
            for (int depthIndex = 0, colorIndex = 0;
                depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                ++depthIndex, colorIndex += 4)
            {
                int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                if (depth <= 900)
                {
                    pixels[colorIndex + BlueIndex] = 255;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 0;
                }
                else if (depth > 900 && depth < 2000)
                {
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 255;
                    pixels[colorIndex + RedIndex] = 0;
                }
                else if (depth > 2000)
                {
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 255;
                }
                byte intensity = CalculateIntensityFromeDepth(depth);
                pixels[colorIndex + BlueIndex] = intensity;
                pixels[colorIndex + GreenIndex] = intensity;
                pixels[colorIndex + RedIndex] = intensity;

                //if (player > 0)
                //{
                //    pixels[colorIndex + BlueIndex] = Colors.Gold.B;
                //    pixels[colorIndex + GreenIndex] = Colors.Gold.G;
                //    pixels[colorIndex + RedIndex] = Colors.Gold.R;
                //}
            }
            return pixels;
        }
        
        public static byte CalculateIntensityFromeDepth(int distance)
        {
            return (byte)(255 - (255 * Math.Max(distance - MinDepthDistance, 0) / (MaxDepthDistanceOffset)));
        }

    }
}

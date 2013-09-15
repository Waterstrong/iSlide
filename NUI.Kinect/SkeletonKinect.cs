using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;
using NUI.Net;

using System.Windows.Threading;
using System.IO;
using System.Windows.Forms;

namespace NUI.Kinect
{
    /// <summary>
    /// 骨架跟踪处理
    /// </summary>
    public class SkeletonKinect : KinectSuper
    {
        SkeletonSubject _subject = new SkeletonSubject();
        public NUI.Kinect.SkeletonSubject Subject
        {
            get { return _subject; }
        }
        protected bool _isSeated = false; // 是否坐姿模式，默认为站立模式
        public bool IsSeated
        {
            //get { return _isSeated; }
            set { _isSeated = value; }
        }

        //private int _trackingId = -1; // 识别ID号

        //骨骼平滑参数
        private TransformSmoothParameters parameters = new TransformSmoothParameters
        {
            Smoothing = 0.7f,
            Correction = 0.0f,
            Prediction = 0.0f,
            JitterRadius = 0.05f,
            MaxDeviationRadius = 0.04f
         };
//         private TransformSmoothParameters parameters = new TransformSmoothParameters
//         {
//             Prediction = 0.0f,
//             JitterRadius = 1.0f,
//             Smoothing = 0.3f,
//             Correction = 0.0f,
//             MaxDeviationRadius = 0.5f
//         };
                     
        private int GetTimeWaitCount(string fileName)
        {
            try
            {
                string strRead = File.ReadAllText(fileName); // 读取配置文件
                return int.Parse(strRead);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return 3;
        }
        /// <summary>
        /// 开始执行骨骼跟踪
        /// </summary>
        protected override void StartKinectFrame()
        {
            if (_nui != null)
            {
                _nui.SkeletonStream.TrackingMode = _isSeated ? SkeletonTrackingMode.Seated : SkeletonTrackingMode.Default;
                _nui.SkeletonStream.Enable(/*parameters*/);
                _nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(_nui_SkeletonFrameReady);
            }
        }
        /// <summary>
        /// 停止骨骼跟踪
        /// </summary>
        protected override void StopKinectFrame()
        {
            if (_nui != null)
            {
                _nui.SkeletonFrameReady -= new EventHandler<SkeletonFrameReadyEventArgs>(_nui_SkeletonFrameReady);
                _nui.SkeletonStream.Disable();
            }
        }

        /// <summary>
        /// 事件与委托，处理跟踪骨骼信息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">骨骼参数</param>
        private void _nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                {
                    return;
                }
                // 储存骨骼信息的数组
                Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);

                _subject.Notify(_nui, skeletons); // 通知进行处理
               
            }
        }

        #region 设置检测的阈值范围
        const float DetectionMinThreshold_X = -0.9f;
        const float DetectionMaxThreshold_X = 0.9f;

        const float DetectionMinThreshold_Y = -0.33f;
        const float DetectionMaxThreshold_Y = 0.53f;

        const float DetectionMinThreshold_Z = 1.5f;
        const float DetectionMaxThreshold_Z = 3.9f;
        #endregion
        
        /// <summary>
        /// 判断骨架是否在指定的范围中
        /// </summary>
        /// <param name="skeleton">要判断的骨架</param>
        /// <returns>在范围中返回true,否则false</returns>
        private bool IsSkeletonInRange(Skeleton skeleton)
        {
            float skeX = skeleton.Position.X;
            float skeY = skeleton.Position.Y;
            float skeZ = skeleton.Position.Z;
            // 确保在指定的范围内
            if (skeZ >= DetectionMinThreshold_Z &&
                skeZ <= DetectionMaxThreshold_Z &&
                skeX >= DetectionMinThreshold_X &&
                skeX <= DetectionMaxThreshold_X &&
                skeY >= DetectionMinThreshold_Y &&
                skeY <= DetectionMaxThreshold_Y)
            {
                return true;
            }
            return false;
        }

    }
}

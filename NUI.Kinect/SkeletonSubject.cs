using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;
using NUI.Data;

namespace NUI.Kinect
{
    /// <summary>
    /// 骨架处理后的通知者，观察者模式
    /// </summary>
    public class SkeletonSubject 
    {
        public delegate void MapPositionEventHandler(List<SkeletonData> datum);
        public event MapPositionEventHandler MapPosition;

        public delegate void TargetCountEventHandler(int tarcnt);
        public event TargetCountEventHandler TargetCount;

        List<SkeletonData> _skeletonDatum = new List<SkeletonData>();

        private int _maxTarcnt = 0;
        /// <summary>
        /// 通知观察者的目标
        /// </summary>
        /// <param name="skeletons">传入骨架数组</param>
        public void Notify(KinectSensor sensor, Skeleton[] skeletons)
        {
            if (TargetCount != null)
            {
                _skeletonDatum.Clear();
                foreach(Skeleton skeleton in skeletons)
                {
                    if (skeleton.TrackingState != SkeletonTrackingState.NotTracked)
                    {
                        // 设置骨架信息，用于映射到图像                        
                        _skeletonDatum.Add(new SkeletonData(sensor, skeleton));
                    }
                }
                if (MapPosition != null)
                {
                    MapPosition(_skeletonDatum);
                }

                // 计算当前设备获取到此批总人数
                int cnt = _skeletonDatum.Count();
                if (cnt == 0)
                {
                    if (_maxTarcnt > 0)
                    {
                        TargetCount(_maxTarcnt);// 传入骨架信息,返回追踪人数
                        _maxTarcnt = 0;
                    }
                }
                else
                {
                    if (_maxTarcnt < cnt)
                    {
                        _maxTarcnt = cnt;
                    }
                }
            }
            
        }
    }
}

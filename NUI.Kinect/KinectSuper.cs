using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Kinect;


namespace NUI.Kinect
{
    /// <summary>
    /// 与Kinect有关的处理超类
    /// </summary>
    public abstract class KinectSuper : INotifyPropertyChanged // INotifyPropertyChanged用于属性更改后通知（读取连接失败的原因）
    {
        protected KinectSensor _nui = null; // KinectSensor对象

        public event PropertyChangedEventHandler PropertyChanged; // 通知事件处理程序

        protected string _disconnectedReason = null; // 断开连接的原因
        public string DisconnectedReason
        {
            get
            { // 对外部可读取
                return _disconnectedReason;
            }
            private set 
            { // 对外部不可写
                if (_disconnectedReason != value)
                {
                    _disconnectedReason = value;
                    if (this.PropertyChanged != null && _disconnectedReason != null)
                    { // 通知外部断开的原因
                        this.PropertyChanged(this, new PropertyChangedEventArgs("DisconnectedReason"));
                    }
                }
            }
        }

        /// <summary>
        /// 启动Kinect
        /// </summary>
        public bool StartKinect()
        {
            this.StopKinect(); // 停止之前的，相当于重启
            KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged); // 委托给StatusChanged
            return this.Initialize(); // 初始化Kinect
        }

        /// <summary>
        /// 停止Kinect
        /// </summary>
        public void StopKinect()
        {
            KinectSensor.KinectSensors.StatusChanged -= new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
            this.StopKinectFrame(); // 停止读取帧

            this.Uninitialize(); // 恢复为最初状态
        }
        /// <summary>
        /// 不停地检测Kinect状态
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">发送者的参数，这里代表KinectSensor</param>
        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch(e.Status)
            {
                case KinectStatus.Connected: // 设备重新连接
                    Initialize(); // 初始化NUI
                    break;
                case KinectStatus.Disconnected: // 设备断开连接
                    DisconnectedReason = "设备已断开连接";
                    break;
                case KinectStatus.Initializing: // 初始化设备
                    break;
                case KinectStatus.NotPowered: // 设备未供电                    
                    DisconnectedReason = "设备未正常连接电源";
                    break;
                case KinectStatus.NotReady: // 设备未准备就绪
                    DisconnectedReason = "设备未准备就绪";
                    break;
                case KinectStatus.DeviceNotGenuine: // 设备更改后
                    //_nui = e.Sensor;
                    //_nui.Start();
                    Initialize(); // 初始化NUI
                    break;
                default: // 其它错误状态
                    DisconnectedReason = "其它错误状态";
                    // DebugLogFactory.GetLog().WriteLog("Other error status！", "void KinectSensors_StatusChanged(...)");
                    break;
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns>初始化成功返回true</returns>
        protected bool Initialize()
        {
            this.Uninitialize(); // 恢复为NULL
            if (KinectSensor.KinectSensors.Count > 0) // 保证至少有一台Kinect
            {
                try
                {
                    _nui = KinectSensor.KinectSensors[0];
                    StartKinectFrame();
                    _nui.Start();
                    return true;
                }
                catch (System.Exception ex)
                {
                    _nui = null;
                    DisconnectedReason = ex.Message + "\n已检测到设备，但初始化失败，请检查电源是否连接。";
                }
            }
            else
            {
                DisconnectedReason = "设备未能正常连接！";
            }
            return false;
        }
        /// <summary>
        /// 恢复初始化前的状态
        /// </summary>
        protected void Uninitialize()
        {
            if (_nui != null)
            {
                _nui.Stop();
                _nui = null;
            }
            DisconnectedReason = null;
        }

        /// <summary>
        /// 开始Kinect的帧跟踪
        /// </summary>
        protected abstract void StartKinectFrame();

        /// <summary>
        /// 停止Kinect的帧跟踪
        /// </summary>
        protected abstract void StopKinectFrame();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUI.Net
{
    /// <summary>
    /// 命令枚举类型
    /// </summary>
    enum Command 
    { 
        FootLeftUp = 1, // 左抬脚
        FootRightUp = 2, // 右抬脚
        FootTwoUp = 3, // 双脚起跳
        LegLeftUp = 10, // 左抬腿
        LegRightUp = 11, // 右抬腿
        HandsUp = 20, // 双手向上推
        HandsMiddle = 21, // 双手平推
        HandsDown = 22, // 双手向下推
        HandsUnfold = 23, // 双手水平展开
        HandsFold = 24, // 双手水平收缩
        HandLeftCircle = 30, // 手左划圆
        HandRightCircle = 31, // 手右划圆
        HandLeftSlide = 32, // 手左滑动
        HandRightSlide = 33, // 手右滑动 
        HandLeftUp = 34, // 手左向上
        HandRightUp = 35, // 手右手向上
        HandLeftDown = 36, // 手左向下
        HandRightDown = 37 // 手右向下
    }
    /// <summary>
    /// 识别动作后的命令包
    /// </summary>
    class CmdPacket
    {
        private Command _cmd; // 生成的命令
        private float _value; // 协同系数，如手移动的相对距离或强度
        private string _extra; // 其他附加信息
        public Command Cmd
        {
            get { return _cmd; }
            set { _cmd = value; }
        }        
        public float Value
        {
            get { return _value; }
            set { _value = value; }
        }        
        public string Extra
        {
            get { return _extra; }
            set { _extra = value; }
        }
    }
}

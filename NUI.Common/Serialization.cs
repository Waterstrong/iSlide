using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace NUI.Common
{
    public class Serialization
    {
        /// <summary>
        /// 将对像序列化为Byte数组
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>字节数组</returns>
        public static byte[] Serialize(Object obj)
        {
            byte[] bt;
            MemoryStream ms = new MemoryStream(); // 创建一个内存流，序列化后保存在其中
            BinaryFormatter bf = new BinaryFormatter(); //序列化对象
            bf.Serialize(ms, obj); // 将自定义类序列化为内存流
            bt = ms.GetBuffer(); // 读取到byte
            return bt;
        }

        /// <summary>
        /// 解序列化，将字节转化为自定义类型
        /// </summary>
        /// <param name="bt">字节数组</param>
        /// <returns>对象</returns>
        public static object Deserialize(byte[] bt)
        {
            object obj;
            BinaryFormatter bf = new BinaryFormatter();
            obj = bf.Deserialize(new MemoryStream(bt));
            return obj;
        }
    }
}

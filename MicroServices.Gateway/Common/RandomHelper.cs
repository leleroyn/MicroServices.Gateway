using MicroServices.Gateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MicroServices.Gateway.Common
{
    public class RandomHelper
    {
        /// <summary>
        /// 带权重的随机
        /// </summary>
        /// <param name="list">原始列表</param>
        /// <param name="count">随机抽取条数</param>
        /// <returns></returns>
        public static List<T> GetRandomList<T>(List<T> list, int count) where T : RandomObject
        {
            if (list == null || list.Count <= count || count <= 0)
            {
                return list;
            }

            //计算权重总和
            int totalWeights = 0;
            for (int i = 0; i < list.Count; i++)
            {
                totalWeights += list[i].Weight + 1;  //权重+1，防止为0情况。
            }

            //随机赋值权重
            System.Random ran = new System.Random(GetRandomSeed());  //GetRandomSeed()随机种子，防止快速频繁调用导致随机一样的问题 
            List<KeyValuePair<int, int>> wlist = new List<KeyValuePair<int, int>>();    //第一个int为list下标索引、第一个int为权重排序值
            for (int i = 0; i < list.Count; i++)
            {
                int w = (list[i].Weight + 1) + ran.Next(0, totalWeights);   // （权重+1） + 从0到（总权重-1）的随机数
                wlist.Add(new KeyValuePair<int, int>(i, w));
            }

            //排序
            wlist.Sort(
              delegate (KeyValuePair<int, int> kvp1, KeyValuePair<int, int> kvp2)
              {
                  return kvp2.Value - kvp1.Value;
              });

            //根据实际情况取排在最前面的几个
            List<T> newList = new List<T>();
            for (int i = 0; i < count; i++)
            {
                T entiy = list[wlist[i].Key];
                newList.Add(entiy);
            }

            //随机法则
            return newList;
        }
        /// <summary>
        /// 随机种子值
        /// </summary>
        /// <returns></returns>
        private static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
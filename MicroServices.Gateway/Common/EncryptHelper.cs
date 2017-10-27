using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace MicroServices.Gateway.Common
{
    public static class EncryptHelper
    {
        #region DES加密/解密

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="srcData">待加密的字符串</param>
        /// <param name="encryPwd">加密密钥</param>
        /// <returns>加密成功返回加密后的字符串,失败返回源串</returns>
        public static string DESEncrypt(string srcData, string encryPwd)
        {
            if (string.IsNullOrEmpty(srcData) || string.IsNullOrEmpty(encryPwd))
            {
                return string.Empty;
            }
            encryPwd =  GetMd5Hash(encryPwd);
            string key = encryPwd.Substring(0, 8);
            var byteKey = Encoding.UTF8.GetBytes(key);
            var byteIV = Encoding.UTF8.GetBytes(key);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(srcData);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byteKey, byteIV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();

            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptData">待解密的字符串</param>
        /// <param name="encryPwd">解密密钥</param>
        /// <returns>解密成功返回解密后的字符串,失败返源串</returns>
        public static string DESDecrypt(string encryptData, string encryPwd)
        {
            if (string.IsNullOrEmpty(encryptData) || string.IsNullOrEmpty(encryPwd))
            {
                return string.Empty;
            }
            encryPwd = GetMd5Hash(encryPwd);
            string key = encryPwd.Substring(0, 8);
            var byteKey = Encoding.UTF8.GetBytes(key);
            var byteIV = Encoding.UTF8.GetBytes(key);
            byte[] inputByteArray = new byte[encryptData.Length / 2];
            for (int x = 0; x < encryptData.Length / 2; x++)
            {
                int i = (Convert.ToInt32(encryptData.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            try
            {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byteKey, byteIV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                Encoding encoding = new UTF8Encoding();
                return encoding.GetString(ms.ToArray());
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region MD5

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">需加密字符串</param>
        /// <returns></returns>
        public static string GetMd5Hash(string input)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        #endregion
    }
}
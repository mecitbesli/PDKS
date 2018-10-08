using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace PDKS.Pages
{
    public class Crypt : PageModel
    {
        private static string strKeyDef = "avABa39Q";

        ///<summary>
        ///Encrypts given string with 256 bit AES and returns encrypted
        ///</summary>
        public static string Encrypt(string strData)
        {
            return Encrypt(strData, strKeyDef);
        }
        ///<summary>
        ///Decrypts given string with 256 bit AES and returns decrypted
        ///</summary>
        public static string Decrypt(string strData)
        {
            return Decrypt(strData, strKeyDef);
        }
        ///<summary>
        ///Encrypts given string with given key with 256 bit AES and returns encrypted
        ///</summary>
        public static string Encrypt(string strData, string strKey)
        {
            byte[] key = { };
            byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };
            byte[] inputByteArray;
            try
            {
                key = Encoding.UTF8.GetBytes(strKey);
                DESCryptoServiceProvider ObjDES = new DESCryptoServiceProvider();
                inputByteArray = Encoding.UTF8.GetBytes(strData);
                MemoryStream Objmst = new MemoryStream();
                CryptoStream Objcs = new CryptoStream(Objmst, ObjDES.CreateEncryptor(key, IV), CryptoStreamMode.Write);
                Objcs.Write(inputByteArray, 0, inputByteArray.Length);
                Objcs.FlushFinalBlock();
                return Convert.ToBase64String(Objmst.ToArray());
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        ///<summary>
        ///Decrypts given string with given key with 256 bit AES and returns decrypted
        ///</summary>
        public static string Decrypt(string strData, string strKey)
        {
            byte[] key = { };
            byte[] IV = { 10, 20, 30, 40, 50, 60, 70, 80 };
            byte[] inputByteArray = new byte[strData.Length];
            try
            {
                key = Encoding.UTF8.GetBytes(strKey);
                DESCryptoServiceProvider ObjDES = new DESCryptoServiceProvider();
                inputByteArray = Convert.FromBase64String(strData);
                MemoryStream Objmst = new MemoryStream();
                CryptoStream Objcs = new CryptoStream(Objmst, ObjDES.CreateDecryptor(key, IV), CryptoStreamMode.Write);
                Objcs.Write(inputByteArray, 0, inputByteArray.Length);
                Objcs.FlushFinalBlock();
                Encoding encoding = Encoding.UTF8;
                return encoding.GetString(Objmst.ToArray());
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
    }
}
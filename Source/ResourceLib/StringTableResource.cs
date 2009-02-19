using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib
{
    public class StringTableResource : Resource
    {
        Kernel32.STRING_OR_VAR_INFO_HEADER _blockInfo;
        string _blockKey;
        Dictionary<string, string> _strings;

        public string BlockKey
        {
            get
            {
                return _blockKey;
            }
        }

        public Dictionary<string, string> Strings
        {
            get
            {
                return _strings;
            }
        }

        public Kernel32.STRING_OR_VAR_INFO_HEADER BlockInfo
        {
            get
            {
                return _blockInfo;
            }
        }

        public StringTableResource(IntPtr hResource, ushort type, string name, ushort wIDLanguage, int size)
            : base(hResource, new IntPtr(type), Marshal.StringToHGlobalAuto(name), wIDLanguage, size)
        {
            Load(hResource);
        }

        public void Load(IntPtr lpRes)
        {
            _strings = new Dictionary<string, string>();

            _blockInfo = (Kernel32.STRING_OR_VAR_INFO_HEADER) Marshal.PtrToStructure(
                lpRes, typeof(Kernel32.STRING_OR_VAR_INFO_HEADER));

            IntPtr pBlockKey = ResourceUtil.Align(lpRes.ToInt32() + Marshal.SizeOf(_blockInfo));
            _blockKey = Marshal.PtrToStringUni(pBlockKey);

            IntPtr pChild = ResourceUtil.Align(pBlockKey.ToInt32() + (_blockKey.Length + 1) * 2);
            Kernel32.STRING_OR_VAR_INFO_HEADER pChildInfo = (Kernel32.STRING_OR_VAR_INFO_HEADER) Marshal.PtrToStructure(
                pChild, typeof(Kernel32.STRING_OR_VAR_INFO_HEADER));

            // read strings, each string is in a structure described in http://msdn.microsoft.com/en-us/library/aa909025.aspx
            while (pChild.ToInt32() < (lpRes.ToInt32() + _blockInfo.wLength))
            {
                IntPtr pChildKey = new IntPtr(pChild.ToInt32() + Marshal.SizeOf(pChildInfo));
                string key = Marshal.PtrToStringUni(pChildKey);

                IntPtr pValue = ResourceUtil.Align(pChildKey.ToInt32() + (key.Length + 1) * 2);
                string value = pChildInfo.wValueLength > 0 ? Marshal.PtrToStringUni(pValue, pChildInfo.wValueLength - 1) : null;

                _strings.Add(key, value);

                pChild = ResourceUtil.Align(pChild.ToInt32() + pChildInfo.wLength);

                pChildInfo = (Kernel32.STRING_OR_VAR_INFO_HEADER)Marshal.PtrToStructure(
                    pChild, typeof(Kernel32.STRING_OR_VAR_INFO_HEADER));
            }
            
        }
    }
}

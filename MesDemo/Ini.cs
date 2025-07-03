using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace MesDemo
{
    public class Ini
    {
        ArrayList m_vPairs = new ArrayList();

        public class CEntry
        {
            public string m_strKey, m_strValue;

            public CEntry(string key, string val)
            {
                m_strKey = key;
                m_strValue = val;
            }

            public bool isKeyEquals(string lpszKey)
            {
                return m_strKey.Equals(lpszKey);
            }
        }


        public bool load(string lpszPath, Encoding enc)
        {
            StreamReader fileStream = new StreamReader(lpszPath, enc);

            string line;
            // Read and display lines from the file until the end of 
            // the file is reached.
            while ((line = fileStream.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("#"))
                { // start with '#'
                    continue;
                }
                int i = line.IndexOf("=");
                if (i != -1)
                {
                    string key = line.Substring(0, i);
                    key = key.Trim();
                    string val = line.Substring(i + 1);
                    val = val.Trim();
                    m_vPairs.Add(new CEntry(key, val));
                }
                else
                {
                    // skip...
                }
            }

            fileStream.Close();

            return true;
        }


        public bool load(string lpszPath)
        {
            return load(lpszPath, Encoding.UTF8);
        }

        public string getValue(string lpszKey)
        {
            return getValue(lpszKey, "");
        }

        public int getValueInt(string lpszKey)
        {
            return getValueInt(lpszKey, 0);
        }

        public string getValue(string lpszKey, string strDefaultValue)
        {
            foreach (CEntry entry in m_vPairs)
            {
                if (entry.isKeyEquals(lpszKey))
                {
                    return entry.m_strValue;
                }
            }
            return strDefaultValue;
        }


        public int getValueInt(string lpszKey, int defaultVal)
        {
            foreach (CEntry entry in m_vPairs)
            {
                if (entry.isKeyEquals(lpszKey))
                {
                    return int.Parse(entry.m_strValue);
                }
            }
            return defaultVal;
        }
    }
}

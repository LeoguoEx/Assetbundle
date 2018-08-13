using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asgard
{
    public interface IAttributes
    {
        void SetAttributeString(string attributeName, string value);

        string GetAttributeString(string attributeName, string defaultValue);

        void SetAttributeFloat(string attributeName, float value);

        float GetAttributeFloat(string attributeName, float defaultValue);

        void SetAttributeInt(string attributeName, int value);

        int GetAttributeInt(string attributeName, int defaultValue);

        void SetAttributeLong(string attributeName, long value);

        long GetAttributeLong(string attributeName, long defaultValue);

        void SetAttributeBool(string attributeName, bool value);

        bool GetAttributeBool(string attributeName, bool defaultValue);

    }
}
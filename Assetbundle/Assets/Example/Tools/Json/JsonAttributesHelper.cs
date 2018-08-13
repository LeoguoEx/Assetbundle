using Asgard.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asgard.Json
{
    public class JsonAttributesHelper : IAttributes
    {
        private JsonNode _refNode;
        public void BindJsonNode(JsonNode jsonNode)
        {
            _refNode = jsonNode;
        }

        public bool ContainsAttribute(string attributeName)
        {
            return (_refNode != null)?_refNode.ContainsAttribute(attributeName):false;
        }

        public void SetAttributeString(string attributeName, string value)
        {
            if (_refNode != null)
                _refNode.SetAttributeString(attributeName, value);
        }

        public string GetAttributeString(string attributeName, string defaultValue)
        {
            return (_refNode != null) ? _refNode.GetAttributeString(attributeName, defaultValue) : defaultValue;
        }

        public void SetAttributeFloat(string attributeName, float value)
        {
            if (_refNode != null)
                _refNode.SetAttributeNumber(attributeName, value);
        }

        public float GetAttributeFloat(string attributeName, float defaultValue)
        {
            return (_refNode != null) ? (float)_refNode.GetAttributeNumber(attributeName, defaultValue) : defaultValue;
        }
        public void SetAttributeInt(string attributeName, int value)
        {
            if (_refNode != null)
                _refNode.SetAttributeInt(attributeName, value);
        }
        public int GetAttributeInt(string attributeName, int defaultValue)
        {
            return (_refNode != null) ? _refNode.GetAttributeInt(attributeName, defaultValue) : defaultValue;
        }
        public void SetAttributeLong(string attributeName, long value)
        {
            if (_refNode != null)
                _refNode.SetAttributeLong(attributeName, value);
        }
        public long GetAttributeLong(string attributeName, long defaultValue)
        {
            return (_refNode != null) ? _refNode.GetAttributeLong(attributeName, defaultValue) : defaultValue;
        }
        public void SetAttributeBool(string attributeName, bool value)
        {
            if (_refNode != null)
                _refNode.SetAttributeBool(attributeName, value);
        }
        public bool GetAttributeBool(string attributeName, bool defaultValue)
        {
            return (_refNode != null) ? _refNode.GetAttributeBool(attributeName, defaultValue) : defaultValue;
        }
    }
}

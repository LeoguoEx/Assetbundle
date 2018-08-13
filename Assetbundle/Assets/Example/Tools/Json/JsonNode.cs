using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asgard.Json
{
    public enum JsonNodeType
    {
        None = -1,           // Used to denote no Lookahead available
        StringValue,
        IntegerValue,
        NumberValue,
        BooleanValue,
        ArrayValue,
        ObjectValue,
        NullValue,
        Object
    }

    public class JsonNode
    {
        private string _name;
        private JsonNodeType _nodeType;

        private string _stringValue;
        private double _doubleValue;
        private long _longValue;
        private JsonNode _objectValue;
        private List<JsonNode> _childs;

        public JsonNode()
        {
			_name = "";
			_nodeType = JsonNodeType.None;
			_childs = null;
			_objectValue = null;
			_objectValue = null;
			_stringValue = null;
			_doubleValue = 0.0;
			_longValue = 0;
        }

        public void Release()
        {
            _name = "";
            ReleaseData();
        }

        private void ReleaseData()
        {
			if (_nodeType == JsonNodeType.None)
				return;

            if (_childs != null)
            {
                for (int i = 0; i < _childs.Count; i++)
                {
                    _childs[i].ReleaseData();
                }
                _childs.Clear();
            }
            _childs = null;
            if (_objectValue != null)
            {
                _objectValue.ReleaseData();
            }
            _objectValue = null;
            _stringValue = null;
            _doubleValue = 0.0;
            _longValue = 0;
        }

        public string Name
        {
            get { return _name; }
            set { _name = (value != null) ? value : "Unknown"; }
        }

        public JsonNodeType NodeType
        {
            get { return _nodeType; }
        }
        public void SetNullValue()
        {
            if (_nodeType != JsonNodeType.NullValue)
            {
                ReleaseData();
                _nodeType = JsonNodeType.NullValue;
            }
        }

        public void SetStringValue(string value)
        {
            if (_nodeType != JsonNodeType.StringValue)
            {
                ReleaseData();
                _nodeType = JsonNodeType.StringValue;
            }
            _stringValue = value;
        }

        public string GetStringValue(string defaultValue)
        {
            if (_nodeType == JsonNodeType.StringValue)
                return _stringValue;
            else
                return defaultValue;
        }

        public void SetNumberValue(double value)
        {
            if (_nodeType != JsonNodeType.NumberValue)
            {
                ReleaseData();
                _nodeType = JsonNodeType.NumberValue;
            }
            _doubleValue = value;
        }

        public double GetNumberValue(double defaultValue)
        {
            if (_nodeType == JsonNodeType.NumberValue || _nodeType == JsonNodeType.IntegerValue)
                return _doubleValue;
            else
                return defaultValue;
        }

        public void SetIntValue(int value)
        {
            if (_nodeType != JsonNodeType.IntegerValue)
            {
                ReleaseData();
                _nodeType = JsonNodeType.IntegerValue;
            }
            _longValue = value;
            _doubleValue = value;
        }
        public int GetIntValue(int defaultValue)
        {
            if (_nodeType == JsonNodeType.IntegerValue || _nodeType == JsonNodeType.NumberValue)
                return (int)_longValue;
            else
                return defaultValue;
        }

        public void SetLongValue(long value)
        {
            if (_nodeType != JsonNodeType.IntegerValue)
            {
                ReleaseData();
                _nodeType = JsonNodeType.IntegerValue;
            }
            _longValue = value;
            _doubleValue = value;
        }

        public long GetLongValue(long defaultValue)
        {
            if (_nodeType == JsonNodeType.IntegerValue || _nodeType == JsonNodeType.NumberValue)
                return _longValue;
            else
                return defaultValue;
        }

        public void SetBoolValue(bool value)
        {
            if (_nodeType != JsonNodeType.BooleanValue)
            {
                ReleaseData();
                _nodeType = JsonNodeType.BooleanValue;
            }
            _longValue = (value) ? 1 : 0;
        }

        public bool GetBoolValue(bool defaultValue)
        {
            if (_nodeType == JsonNodeType.BooleanValue)
                return (_longValue == 1);
            else
                return defaultValue;
        }

        public int ArraySize
        {
            get { return (_nodeType == JsonNodeType.ArrayValue && _childs != null) ? _childs.Count : 0; }
        }

        public JsonNode GetArrayItem(int idx)
        {
            if (idx >= 0 && _childs != null && idx < _childs.Count)
                return _childs[idx];
            else
                return null;
        }

        public void AddArrayItem(JsonNode node)
        {
            if (node == null)
                return;

            if (_nodeType != JsonNodeType.ArrayValue)
            {
                ReleaseData();
                _nodeType = JsonNodeType.ArrayValue;
            }

            if (_childs == null)
            {
                _childs = new List<JsonNode>();
            }
            _childs.Add(node);
        }

        public JsonNode NewArrayItem()
        {
            JsonNode node = new JsonNode();
            AddArrayItem(node);
            return node;
        }

        public void SetObjectValue(JsonNode objNode)
        {
            if (_nodeType != JsonNodeType.ObjectValue)
            {
                ReleaseData();
                _nodeType = JsonNodeType.ObjectValue;
            }
            _objectValue = objNode;
        }

        public JsonNode GetObjectValue(bool forceType)
        {
            if (_nodeType == JsonNodeType.ObjectValue)
                return _objectValue;
            else
            {
                if (forceType)
                {
                    ReleaseData();
                    _nodeType = JsonNodeType.ObjectValue;
                    _objectValue = new JsonNode();
                    return _objectValue;
                }
                else
                    return null;
            }
        }

        private JsonNode FindChildNode(string childNodeName)
        {
            if (_childs == null && childNodeName == null || childNodeName.Length <= 0)
                return null;

            JsonNode result = null;
            int nodeCount = _childs.Count;
            for (int i = 0; result == null && i < nodeCount; i++)
            {
                if (childNodeName == _childs[i].Name)
                {
                    result = _childs[i];
                }
            }
            return result;
        }

        public bool ContainsAttribute(string attributeName)
        {
            return (FindChildNode(attributeName)!=null);
        }

        public JsonNode this[string attributeName]
        {
            get
            {
                if (_nodeType == JsonNodeType.Object)
                    return FindChildNode(attributeName);
                else
                    return null;
            }
        }

        public JsonNode GetAttribute(string attributeName, bool forceCreate=false)
        {
            if (_childs == null && attributeName == null || attributeName.Length <= 0)
                return null;

            JsonNode result = null;
            if (_nodeType == JsonNodeType.Object)
            {
                result = FindChildNode(attributeName);
            }

            if (forceCreate && result == null)
            {
                if (_nodeType != JsonNodeType.Object)
                {
                    ReleaseData();
                    _nodeType = JsonNodeType.Object;
                }

                result = new JsonNode();
                result.Name = attributeName;
                if (_childs == null)
                {
                    _childs = new List<JsonNode>();
                }
                _childs.Add(result);
            }
            return result;
        }

        public int AttributeCount
        {
            get
            {
                if (_nodeType == JsonNodeType.Object)
                    return (_childs != null) ? _childs.Count : 0;
                else
                    return 0;
            }
        }

        public JsonNode GetAttributeAt(int idx)
        {
            if (_nodeType == JsonNodeType.Object && idx >= 0 && _childs != null && idx < _childs.Count)
                return _childs[idx];
            else
                return null;
        }

        //************************************
        public void SetAttributeString(string attributeName,string value)
        {
            if (_nodeType == JsonNodeType.Object)
            {
                JsonNode attNode = GetAttribute(attributeName,true);
                if (attNode != null)
                {
                    attNode.SetStringValue(value);
                }
                
            }
        }

        public string GetAttributeString(string attributeName,string defaultValue)
        {
            if (_nodeType == JsonNodeType.Object)
            {
                JsonNode attNode = FindChildNode(attributeName);
                if (attNode != null)
                {
                    return attNode.GetStringValue(defaultValue);
                }
            }
            return defaultValue;
        }

        public void SetAttributeNumber(string attributeName, double value)
        {
            if (_nodeType == JsonNodeType.Object)
            {
                JsonNode attNode = GetAttribute(attributeName, true);
                if(attNode!=null)
                {
                    attNode.SetNumberValue(value); ;
                }                
            }
        }

        public double GetAttributeNumber(string attributeName,double defaultValue)
        {
            if (_nodeType == JsonNodeType.Object)
            {
                JsonNode attNode = FindChildNode(attributeName);
                if (attNode != null)
                {
                    return attNode.GetNumberValue(defaultValue);
                }
            }
            return defaultValue;
        }

        public void SetAttributeInt(string attributeName,int value)
        {
            if (_nodeType == JsonNodeType.Object)
            {
                JsonNode attNode = GetAttribute(attributeName, true);
                if (attNode != null)
                {
                    attNode.SetIntValue(value); ;
                }
            }
        }
        public int GetAttributeInt(string attributeName,int defaultValue)
        {
            if (_nodeType == JsonNodeType.Object)
            {
                JsonNode attNode = FindChildNode(attributeName);
                if (attNode != null)
                {
                    return attNode.GetIntValue(defaultValue);
                }
            }
            return defaultValue;
        }

        public void SetAttributeLong(string attributeName,long value)
        {
            if (_nodeType == JsonNodeType.Object)
            {
                JsonNode attNode = GetAttribute(attributeName, true);
                if (attNode != null)
                {
                    attNode.SetLongValue(value); ;
                }
            }
        }

        public long GetAttributeLong(string attributeName,long defaultValue)
        {
            if (_nodeType == JsonNodeType.Object)
            {
                JsonNode attNode = FindChildNode(attributeName);
                if (attNode != null)
                {
                    return attNode.GetLongValue(defaultValue);
                }
            }
            return defaultValue;
        }

        public void SetAttributeBool(string attributeName,bool value)
        {
            if (_nodeType == JsonNodeType.Object)
            {
                JsonNode attNode = GetAttribute(attributeName, true);
                if (attNode != null)
                {
                    attNode.SetBoolValue(value); ;
                }
            }
        }

        public bool GetAttributeBool(string attributeName,bool defaultValue)
        {
            if (_nodeType == JsonNodeType.Object)
            {
                JsonNode attNode = FindChildNode(attributeName);
                if (attNode != null)
                {
                    return attNode.GetBoolValue(defaultValue);
                }
            }
            return defaultValue;
        }

    }
}

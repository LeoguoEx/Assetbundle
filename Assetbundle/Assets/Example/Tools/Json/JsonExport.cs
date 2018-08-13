using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asgard.Json
{
    public class JsonExport
    {
        public static string ExportJson(JsonNode node)
        {
            StringBuilder exportBuffer = new StringBuilder();
            ExportJsonNode(node, exportBuffer);
            return exportBuffer.ToString();
        }

        private static void ExportJsonNode(JsonNode node, StringBuilder buffer)
        {
            if (node == null)
                return;

            switch (node.NodeType)
            {
                case JsonNodeType.StringValue:
                    buffer.Append("\"").Append(node.Name).Append("\":\"").Append(node.GetStringValue("")).Append("\"");
                    break;
                case JsonNodeType.NumberValue:
                    buffer.Append("\"").Append(node.Name).Append("\":").Append(node.GetNumberValue(0.0));
                    break;
                case JsonNodeType.IntegerValue:
                    buffer.Append("\"").Append(node.Name).Append("\":").Append(node.GetLongValue(0));
                    break;
                case JsonNodeType.BooleanValue:
                    buffer.Append("\"").Append(node.Name).Append("\":").Append(node.GetBoolValue(false));
                    break;
                case JsonNodeType.ArrayValue:
                    buffer.Append("\"").Append(node.Name).Append("\":\r\n[\r\n");
                    int childCount = node.ArraySize;
                    for (int i = 0; i < childCount; i++)
                    {
                        if (i > 0)
                        {
                            buffer.Append(",\r\n");
                        }
                        ExportJsonNode(node.GetArrayItem(i), buffer);

                    }
                    buffer.Append("]\r\n");
                    break;
                case JsonNodeType.Object:
                    buffer.Append("{");
                    int attCount = node.AttributeCount;
                    for (int i = 0; i < attCount; i++)
                    {
                        if (i > 0)
                        {
                            buffer.Append(",");
                        }
                        ExportJsonNode(node.GetAttributeAt(i), buffer);
                    }
                    buffer.Append("}\r\n");
                    break;
                case JsonNodeType.NullValue:
                    buffer.Append("\"").Append(node.Name).Append("\":").Append(node.GetStringValue(""));
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Asgard.Json
{

    public enum JsonToken
    {
        None = -1,           // Used to denote no Lookahead available
        Curly_Open,  //{
        Curly_Close,  //}
        Squared_Open,  //[
        Squared_Close, //]
        Colon,  //:
        Comma,  //,
        String,
        Number,
        True,
        False,
        Null
    }
    public class JsonParser
    {
        private JsonNode _rootNode;
        private readonly string _json;
        private readonly StringBuilder _textBuffer = new StringBuilder();
        private JsonToken _lookAheadToken = JsonToken.None;
        private int _index;
        private bool _ignorecase = false;

        public JsonParser(string json, bool ignorecase)
        {
            this._json = json;
            this._ignorecase = ignorecase;
            this._rootNode = new JsonNode();
        }

        public JsonNode Decode()
        {
            if(this._json!=null && this._json.Length>0)
            {
                ParseValue(_rootNode);
            }
            return _rootNode;
        }

        private void ParseObject(JsonNode refNode)
        {
            ConsumeToken(); // {

            while (true)
            {
                switch (LookAhead())
                {

                    case JsonToken.Comma:
                        ConsumeToken();
                        break;

                    case JsonToken.Curly_Close:
                        ConsumeToken();
                        return;
                    default:
                        {
                            // name
                            string name = ParseString();
                            if (_ignorecase)
                                name = name.ToLower();

                            // :
                            if (NextToken() != JsonToken.Colon)
                            {
                                throw new Exception("Expected colon at index " + _index);
                            }

                            // value
					        JsonNode attributeNode = refNode.GetAttribute(name, true);
                            ParseValue(attributeNode);
                        }
                        break;
                }
            }
        }

        private void ParseArray(JsonNode refNode)
        {
            ConsumeToken(); // [

            while (true)
            {
                switch (LookAhead())
                {
                    case JsonToken.Comma:
                        ConsumeToken();
                        break;

                    case JsonToken.Squared_Close:
                        ConsumeToken();
                        return;

                    default:
                        JsonNode childNode = refNode.NewArrayItem();
                        ParseValue(childNode);
                        break;
                }
            }
        }

        private void ParseValue(JsonNode refNode)
        {
            switch (LookAhead())
            {
                case JsonToken.Number:
                    ParseNumber(refNode);
                    return;
                case JsonToken.String:
                    refNode.SetStringValue(ParseString());
                    return;
                case JsonToken.Curly_Open:
                    ParseObject(refNode);
                    return;
                case JsonToken.Squared_Open:
                    ParseArray(refNode);
                    return;
                case JsonToken.True:
                    refNode.SetBoolValue(true);
                    ConsumeToken();
                    return;
                case JsonToken.False:
                    refNode.SetBoolValue(false);
                    ConsumeToken();
                    return;
                case JsonToken.Null:
                    refNode.SetNullValue();
                    ConsumeToken();
                    return;
            }

            throw new Exception("Unrecognized token at index" + _index);
        }

        private string ParseString()
        {
            ConsumeToken(); // "

            _textBuffer.Length = 0;

            int runIndex = -1;
            char c,v;
            while (_index < _json.Length)
            {
                c = _json[_index++];

                if (c == '"')
                {
                    if (runIndex != -1)
                    {
                        if (_textBuffer.Length == 0)
                            return _json.Substring(runIndex, _index - runIndex - 1);

                        _textBuffer.Append(_json, runIndex, _index - runIndex - 1);
                    }
                    return _textBuffer.ToString();
                }

                if (c != '\\')
                {
                    if (runIndex == -1)
                        runIndex = _index - 1;

                    continue;
                }

                if (_index == _json.Length) break;

                if (runIndex != -1)
                {
                    _textBuffer.Append(_json, runIndex, _index - runIndex - 1);
                    runIndex = -1;
                }

                v=_json[_index++];
                switch (v)
                {
                    case '"':
                        _textBuffer.Append('"');
                        break;

                    case '\\':
                        _textBuffer.Append('\\');
                        break;

                    case '/':
                        _textBuffer.Append('/');
                        break;

                    case 'b':
                        _textBuffer.Append('\b');
                        break;

                    case 'f':
                        _textBuffer.Append('\f');
                        break;

                    case 'n':
                        _textBuffer.Append('\n');
                        break;

                    case 'r':
                        _textBuffer.Append('\r');
                        break;

                    case 't':
                        _textBuffer.Append('\t');
                        break;
                    case 'u':
                        {
                            int remainingLength = _json.Length - _index;
                            if (remainingLength < 4) break;

                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint = ParseUnicode(_json[_index], _json[_index + 1], _json[_index + 2], _json[_index + 3]);
                            _textBuffer.Append((char)codePoint);

                            // skip 4 chars
                            _index += 4;
                        }
                        break;
                    default:
                        //不是转义符
                        _textBuffer.Append("\\").Append(v);
                        break;
                }
            }

            throw new Exception("Unexpectedly reached end of string");
        }

        private uint ParseSingleChar(char c1, uint multipliyer)
        {
            uint p1 = 0;
            if (c1 >= '0' && c1 <= '9')
                p1 = (uint)(c1 - '0') * multipliyer;
            else if (c1 >= 'A' && c1 <= 'F')
                p1 = (uint)((c1 - 'A') + 10) * multipliyer;
            else if (c1 >= 'a' && c1 <= 'f')
                p1 = (uint)((c1 - 'a') + 10) * multipliyer;
            return p1;
        }

        private uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            uint p1 = ParseSingleChar(c1, 0x1000);
            uint p2 = ParseSingleChar(c2, 0x100);
            uint p3 = ParseSingleChar(c3, 0x10);
            uint p4 = ParseSingleChar(c4, 1);

            return p1 + p2 + p3 + p4;
        }

        private long CreateLong(string s)
        {
            long num = 0;
            bool neg = false;
            foreach (char cc in s)
            {
                if (cc == '-')
                    neg = true;
                else if (cc == '+')
                    neg = false;
                else
                {
                    num *= 10;
                    num += (int)(cc - '0');
                }
            }

            return neg ? -num : num;
        }

        private void ParseNumber(JsonNode refNode)
        {
            ConsumeToken();

            // Need to start back one place because the first digit is also a token and would have been consumed
            var startIndex = _index - 1;
            bool dec = false;
            do
            {
                if (_index == _json.Length)
                    break;
                var c = _json[_index];

                if ((c >= '0' && c <= '9') || c == '.' || c == '-' || c == '+' || c == 'e' || c == 'E')
                {
                    if (c == '.' || c == 'e' || c == 'E')
                        dec = true;
                    if (++_index == _json.Length)
                        break;//throw new Exception("Unexpected end of string whilst parsing number");
                    continue;
                }
                break;
            } while (true);

            if (dec)
            {
                string s = _json.Substring(startIndex, _index - startIndex);
                refNode.SetNumberValue(double.Parse(s, NumberFormatInfo.InvariantInfo));
                return;
            }
            long num;
            CreateLong(out num, _json, startIndex, _index - startIndex);
            refNode.SetLongValue(num);
            return;
        }

        private JsonToken LookAhead()
        {
            if (_lookAheadToken != JsonToken.None) return _lookAheadToken;

            return _lookAheadToken = NextTokenCore();
        }

        private void ConsumeToken()
        {
            _lookAheadToken = JsonToken.None;
        }

        private JsonToken NextToken()
        {
            var result = _lookAheadToken != JsonToken.None ? _lookAheadToken : NextTokenCore();

            _lookAheadToken = JsonToken.None;

            return result;
        }

        private JsonToken NextTokenCore()
        {
            char c;

            // Skip past whitespace
            do
            {
                c = _json[_index];

                if (c > ' ') break;
                if (c != ' ' && c != '\t' && c != '\n' && c != '\r') break;

            } while (++_index < _json.Length);

            if (_index == _json.Length)
            {
                throw new Exception("Reached end of string unexpectedly");
            }

            c = _json[_index];

            _index++;

            switch (c)
            {
                case '{':
                    return JsonToken.Curly_Open;

                case '}':
                    return JsonToken.Curly_Close;

                case '[':
                    return JsonToken.Squared_Open;

                case ']':
                    return JsonToken.Squared_Close;

                case ',':
                    return JsonToken.Comma;

                case '"':
                    return JsonToken.String;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                case '+':
                case '.':
                    return JsonToken.Number;

                case ':':
                    return JsonToken.Colon;

                case 'f':
                    if (_json.Length - _index >= 4 &&
                        _json[_index + 0] == 'a' &&
                        _json[_index + 1] == 'l' &&
                        _json[_index + 2] == 's' &&
                        _json[_index + 3] == 'e')
                    {
                        _index += 4;
                        return JsonToken.False;
                    }
                    break;

                case 't':
                    if (_json.Length - _index >= 3 &&
                        _json[_index + 0] == 'r' &&
                        _json[_index + 1] == 'u' &&
                        _json[_index + 2] == 'e')
                    {
                        _index += 3;
                        return JsonToken.True;
                    }
                    break;

                case 'n':
                    if (_json.Length - _index >= 3 &&
                        _json[_index + 0] == 'u' &&
                        _json[_index + 1] == 'l' &&
                        _json[_index + 2] == 'l')
                    {
                        _index += 3;
                        return JsonToken.Null;
                    }
                    break;
            }
            throw new Exception("Could not find token at index " + --_index);
        }
        private static long CreateLong(out long num, string s, int index, int count)
        {
            num = 0;
            bool neg = false;
            for (int x = 0; x < count; x++, index++)
            {
                char cc = s[index];

                if (cc == '-')
                    neg = true;
                else if (cc == '+')
                    neg = false;
                else
                {
                    num *= 10;
                    num += (int)(cc - '0');
                }
            }
            if (neg) num = -num;

            return num;
        }

    }
}

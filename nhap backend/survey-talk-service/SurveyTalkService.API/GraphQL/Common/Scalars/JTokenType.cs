// using HotChocolate.Language;
// using HotChocolate.Types;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
// using System.Collections.Generic;

// namespace SurveyTalkService.GraphQL.Types
// {
//     public class JTokenScalarType : ScalarType<JToken, StringValueNode>
//     {
//         public JTokenType() : base("JSON_JTOKEN") { }

//         // Override ParseResult để xử lý JToken trả về dạng StringValueNode
//         public override IValueNode ParseResult(object? resultValue)
//         {
//             if (resultValue == null)
//                 return NullValueNode.Default;

//             if (resultValue is JToken jToken)
//             {
//                 return new StringValueNode(jToken.ToString());
//             }

//             throw new ArgumentException("Invalid result value type", nameof(resultValue));
//         }

//         // Serialize đối tượng JToken thành chuỗi
//         public override object? Serialize(object? runtimeValue)
//         {
//             return runtimeValue?.ToString();
//         }

//         // Deserialize đối tượng
//         public override object? Deserialize(object? resultValue)
//         {
//             return resultValue;
//         }

//         // Override ParseValue trả về StringValueNode
//         protected override StringValueNode ParseValue(JToken runtimeValue)
//         {
//             if (runtimeValue == null)
//                 return null;

//             return new StringValueNode(runtimeValue.ToString());
//         }

//         // ParseLiteral để chuyển StringValueNode thành JToken
//         protected override JToken ParseLiteral(StringValueNode valueSyntax)
//         {
//             return JToken.Parse(valueSyntax.Value);
//         }

//         // Nếu bạn đang làm việc với một danh sách JToken, chuyển chúng thành List<String>
//         // Nếu kiểu trả về là JEnumerable<JToken> thì chuyển thành List<JToken> hoặc List<String>
//         public static List<JToken> ParseJEnumerableToList(JEnumerable<JToken> jEnumerable)
//         {
//             var list = new List<JToken>();
//             foreach (var token in jEnumerable)
//             {
//                 list.Add(token);
//             }
//             return list;
//         }
//     }
// }




using System;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using Newtonsoft.Json.Linq;

namespace SurveyTalkService.GraphQL.Common.Scalars
{
    public class JTokenScalarType : ScalarType
    {
        public JTokenScalarType() : base("JSON_JTOKEN", BindingBehavior.Implicit)
        {
        }

        public override Type RuntimeType => typeof(JToken);

        public override bool IsInstanceOfType(IValueNode valueNode)
        {
            return valueNode is StringValueNode;
        }

        public override bool TrySerialize(object? runtimeValue, out object? resultValue)
        {
            if (runtimeValue is JToken jToken)
            {
                resultValue = jToken.ToString();
                // resultValue = jToken;
                return true;
            }

            resultValue = null;
            return false;
        }

        public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
        {
            if (resultValue is string jsonString)
            {
                try
                {
                    runtimeValue = JToken.Parse(jsonString);
                    return true;
                }
                catch
                {
                    runtimeValue = null;
                    return false;
                }
            }

            runtimeValue = null;
            return false;
        }

        public override IValueNode ParseValue(object? runtimeValue)
        {
            if (runtimeValue is JToken jToken)
            {
                return new StringValueNode(jToken.ToString());
            }

            throw new GraphQLException("Expected a JToken instance.");
        }

        public override IValueNode ParseResult(object? resultValue)
        {
            if (resultValue is string jsonString)
            {
                return new StringValueNode(jsonString);
            }

            throw new GraphQLException("Expected a JSON string.");
        }

        public override object ParseLiteral(IValueNode valueNode)
        {
            if (valueNode is StringValueNode stringValue)
            {
                return JToken.Parse(stringValue.Value);
            }

            throw new GraphQLException("Expected a JSON string.");
        }
    }

    // public class JTokenScalarType : ScalarType<JToken, StringValueNode>
    // {
    //     public JTokenType() : base("JSON_JTOKEN") { }

    //     public override IValueNode ParseResult(object resultValue)
    //     {
    //         if (resultValue is string jsonString)
    //         {
    //             return new StringValueNode(jsonString);
    //         }

    //         throw new GraphQLException("Expected a JSON string.");
    //     }

    //     protected override JToken ParseLiteral(StringValueNode valueSyntax)
    //     {
    //         return JToken.Parse(valueSyntax.Value);
    //     }

    //     protected override StringValueNode ParseValue(JToken runtimeValue)
    //     {
    //         return new StringValueNode(runtimeValue.ToString(Newtonsoft.Json.Formatting.None));
    //     }

    //     public override bool TrySerialize(object runtimeValue, out object resultValue)
    //     {
    //         if (runtimeValue is JToken jToken)
    //         {
    //             resultValue = jToken.ToString(Newtonsoft.Json.Formatting.None);
    //             return true;
    //         }

    //         resultValue = null;
    //         return false;
    //     }

    //     public override bool TryDeserialize(object resultValue, out object runtimeValue)
    //     {
    //         if (resultValue is string json)
    //         {
    //             runtimeValue = JToken.Parse(json);
    //             return true;
    //         }

    //         runtimeValue = null;
    //         return false;
    //     }

    // }

}
using Bladecoder.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blade_json
{
    class Program
    {
        static void Main(string[] args)
        {
            string json = @"
                {
                    var1: ""stringvar"",
                    var2: 123,
                    var3: 1.2,
                    var4: [9,8,7],
                    var5: { a: ""v5a""}
                }
            ";

            JsonReader jreader = new JsonReader();
            JsonValue jvalue = jreader.parse(json);
            string var1 = jvalue.get("var1").asString();
            int var2 = jvalue.get("var2").asInt();
            float var3 = jvalue.get("var3").asFloat();
            var var4 = jvalue.get("var4").asIntArray();
            string var5 = jvalue.get("var5").get("a").asString();
            Console.WriteLine($">>>>> var1: {var1}");
            Console.WriteLine($">>>>> var2: {var2}");
            Console.WriteLine($">>>>> var3: {var3}");
            Console.WriteLine($">>>>> var4: {var4[1]}");
            Console.WriteLine($">>>>> var5: {var5}");
        }
    }
}

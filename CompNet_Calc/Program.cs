using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace CompNet_Calc
{
    class Program
    {
        /// <summary>
        /// Encodes <paramref name="D"/> to transmit usnig CDMA
        /// </summary>
        /// <param name="K">Device code</param>
        /// <param name="D">Data</param>
        /// <returns>Encoded sequence</returns>
        static string CDMAencode(string K, string D)
        {
            var code = K.Trim('(', ')');
            var notCode = string.Join(',', K.Trim('(', ')').Replace(" ", "").Split(',').Where(s => s != "").Select(val => val.StartsWith('-')? val.Substring(1) :"-" + val));
            var data = D.Trim('(', ')').Replace(" ", "").Split(',').Where(s => s != "").ToArray();
            return "(" + string.Join(',', data.Select(val => val == "1" ? code : notCode)) + ")";
        }

        /// <summary>
        /// Decodes <paramref name="S"/> recived as transmission usnig CDMA
        /// </summary>
        /// <param name="K">Deivce code</param>
        /// <param name="S">Encoded sequence</param>
        /// <returns>(raw data, interpreted data)</returns>
        static (string,string) CDMAdecode(string K, string S)
        {
            var code = K.Trim('(', ')').Replace(" ", "").Split(',').Where(s => s != "").Select(i => int.Parse(i)).ToArray();
            var input = S.Trim('(', ')').Replace(" ", "").Split(',').Where(s => s != "").Select(i => int.Parse(i)).ToArray();
            var res = new List<double>();
            for (int i = 0; i < input.Length / code.Length; i++)
            {
                int t = 0;
                for (int j = 0; j < code.Length; j++)
                    t += code[j] * input[i * code.Length + j];
                res.Add(t / (double)code.Length);
            }
            return (string.Join(", ", res.Select(r => r.ToString("f2"))), string.Join(", ", res.Select(r => Math.Sign(r).ToString())));
            
        }
        /// <summary>
        /// Generates CRC (and prints the process)
        /// </summary>
        /// <param name="D">Data</param>
        /// <param name="G">Generator (polynomial)</param>
        static void CRC(string D, string G)
        {
            D += string.Concat(Enumerable.Repeat("0", G.Length - 1));
            var gen = Convert.ToInt32(G,2);
            Console.WriteLine(D+"|"+G);
            int DstartLength = D.Length;
            var data = D.Select(i => int.Parse(i.ToString()));
            int cur = 0;
            while(data.Any())
            {
                int cnt = 0;
                if (cur == 0)
                    cnt = G.Length;
                else
                    cnt = G.Length - Convert.ToString(cur, 2).Length;
                cur = Convert.ToInt32(cnt == 0? "" : Convert.ToString(cur, 2) + string.Concat(data.Take(cnt)),2);
                if (D.Length != data.Count())
                    Console.WriteLine(string.Concat(Enumerable.Repeat(' ', DstartLength - data.Count() - (G.Length - cnt))) + Convert.ToString(cur, 2));
                Console.WriteLine(string.Concat(Enumerable.Repeat(' ', DstartLength - data.Count() - (G.Length - cnt))) + G);
                Console.WriteLine(string.Concat(Enumerable.Repeat(' ', DstartLength - data.Count() - (G.Length - cnt))) + string.Concat(Enumerable.Repeat('_', G.Length)));
                data = data.Skip(cnt);
                cur = cur ^ gen;
            }
            Console.WriteLine(string.Concat(Enumerable.Repeat(' ', D.Length - Convert.ToString(cur, 2).Length)) + Convert.ToString(cur, 2));
        }

        /// <summary>
        /// Generates list of positions to insert check bit for data with specified <paramref name="length"/>.
        /// </summary>
        /// <param name="length">Data length in bits</param>
        /// <returns>List of positions</returns>
        static int[] PosForHamming(int length)
        {
            var res = new List<int>();
            int cur = 1;
            while(cur<length)
            {
                res.Add(cur);
                length++;
                cur <<= 1;
            }
            return res.ToArray();
        }

        /// <summary>
        /// Generates Hamming code (and prints the process)
        /// </summary>
        /// <param name="D">Data</param>
        static void HammingCode(string D)
        {
            var pows = PosForHamming(D.Length);
            //var pows = Enumerable.Range(0, (int)Math.Sqrt(D.Length) + 1).Select(s => (int)Math.Pow(2, s)).ToArray().Where(s => s <= D.Length).ToArray();
            foreach (var p in pows)
                D = D.Insert(p - 1, "0");
            Console.WriteLine($"1) Вставим контрольные биты на позиции степеней двойки({string.Join(',', pows)}): {D}");
            Console.WriteLine("2) Рассчитаем значения контрольных бит по правилу “контрольный бит с номером N контролирует все последующие N бит через каждые N бит, начиная с позиции N”");
            for (int i = 0; i < pows.Length; i++)
            {
                int p = pows[i];
                string filteredD = "";
                for (int j = p - 1; j < D.Length; j += 2 * p)
                    filteredD += (j + p <= D.Length)? D.Substring(j, p) : D.Substring(j);
                var OneCnt = filteredD.Count(c => c == '1');
                Console.WriteLine($"{2 + 1 + i}) Для {p} бита смотрим позиции: {filteredD} => {OneCnt} единицы ({(OneCnt % 2 == 0? "четное" : "нечётное" )}) => устанавливаем {OneCnt % 2}");
                var t = D.ToCharArray();
                t[p - 1] = (OneCnt % 2).ToString()[0];
                D = new string(t);
            }
            Console.WriteLine($"Результат: {D}");
        }

        static void Main(string[] args)
        {
            Console.WriteLine(CDMAencode("(1,-1,1,1)", "(1, -1, -1)"));

            var ss = CDMAdecode("(1,1,-1,1)", "(1,1,-1,1,1,-1,1,-1,1,-1,-1,1)");
            Console.WriteLine($"Raw decode results: {ss.Item1}");
            Console.WriteLine($"Final decode results: {ss.Item2}");

            CRC("100110101111010010001", "1011");

            HammingCode("1011");
            HammingCode("1 0 0 1 0 0 1 0 1 1 1 0 0 0 1".Replace(" ",""));
            //Console.WriteLine("\u0331");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ExcelNew
{
    public class Reader
    {
        private TextReader stream;
        private char[] buff;
        private const int bufferLen = 16384;
        private long index;
        private long len;

        public Reader(TextReader stream)
        {
            this.stream = stream;
            buff = new char[bufferLen];
            index = 0;
            len = 0;
        }
        StringBuilder wr = new StringBuilder();
        private string FromBuffer(char[] buf, long start, long len)
        {
            wr.Length = 0;
            for (long i = start; i < start + len; i++)
                wr.Append(buf[i]);

            return wr.ToString();
        }

        public string Word()
        {
            if (!SkipSpaces())
                return null;

            var wasNewLine = false;
            if (!SkipLines(out wasNewLine))
                return null;
            else if (wasNewLine)
                return "";

            long start = index;

            while (index < len && !Char.IsWhiteSpace(buff[index]))
                index++;
            if (index < len)
                return FromBuffer(buff, start, index - start);

            StringBuilder sb = new StringBuilder();
            sb.Append(FromBuffer(buff, start, index - start));
            while (index == len && Update())
            {
                while (index < len && !Char.IsWhiteSpace(buff[index]))
                    index++;
                sb.Append(FromBuffer(buff, 0, index));
            }
            return sb.ToString();
        }
        private bool Update()
        {
            if (stream == null || len == -1)
                return false;
            if (index < len)
                return true;

            index = 0;
            if ((len = stream.Read(buff, 0, bufferLen)) == 0)
            {
                len = -1;
                buff = null;
                return false;
            }
            return true;
        }

        private bool isSpace(char c)
        {
            if (c == ' ' || c == '\t')
                return true;
            else
                return false;
        }

        private bool isNewline(char c)
        {
            if (c == '\n' || c == '\r')
                return true;
            else
                return false;
        }
        private bool SkipSpaces()
        {
            while (Update())
            {
                while (index < len && isSpace(buff[index]))
                    index++;
                if (index < len)
                    return true;
            }
            return false;
        }

        private bool SkipLines(out bool isNewLine)
        {
            int newlines = 0;

            while (Update())
            {
                while (index < len && isNewline(buff[index]))
                {
                    newlines++;
                    index++;
                    while (index < len && isSpace(buff[index]))
                        index++;
                }
                if (index < len)
                {
                    if (newlines >= 1) isNewLine = true;
                    else isNewLine = false;
                    return true;
                }
            }
            isNewLine = false;
            return false;
        }

    }
    class Tab
    {
        public string Value { get; set; }
        public char Sign { get; set; }
        public int ColLeft { get; set; }
        public int LineLeft { get; set; }
        public int ColRight { get; set; }
        public int LineRight { get; set; }
        public bool Processed { get; set; }
    }
    class Cycle
    {
        public int ColLeft { get; set; }
        public int LineLeft { get; set; }
        public int ColRight { get; set; }
        public int LineRight { get; set; }
    }
    class ReadTab
    {
        public List<List<Tab>> tab;
        private char[] numbers = "123456789".ToCharArray();
        private char[] sign = "+-/*".ToCharArray();
        private Tab bun;

        public ReadTab()
        {
            tab = new List<List<Tab>>();
        }
        
        public List<List<Tab>> Read(TextReader InFile)
        {
            Reader read = new Reader(InFile);
            string word = "";

            while (word != null)
            {
                word = read.Word();
                List<Tab> line = new List<Tab>();
                while (word != "" && word != null)
                {
                    bun = new Tab() { Value=word };
                    line.Add(Proces(bun));
                    word = read.Word();
                }
                if (line.Count != 0)
                {
                    tab.Add(line);
                }
            }
            return tab;
        }

        public Tab Proces(Tab bunka)
        {
            if (bunka.Value == "[]")
            {
                bunka.Processed = true;
                return bunka;
            }

            int Number;
            if (int.TryParse(bunka.Value, out Number))
            {
                bunka.Processed = true;
                return bunka;
            }

            if (bunka.Value[0] != '=' || bunka.Value.Length == 0)
            {
                bunka.Value = "#INVVAL";
                bunka.Processed = true;
                return bunka;
            }

            int IndexOfOperand = bunka.Value.IndexOfAny(sign);
            if (IndexOfOperand == -1)
            {
                bunka.Value = "#MISSOP";
                bunka.Processed = true;
                return bunka;
            }

            if (IndexOfOperand == 1 || IndexOfOperand + 1 >= bunka.Value.Length)
            {
                bunka.Value = "#FORMULA";
                bunka.Processed = true;
                return bunka;
            }
            char si = bunka.Value[IndexOfOperand];
            int ColLeft; 
            int LineLeft;
            string left = NumberOfTab(bunka.Value.Substring(1, IndexOfOperand - 1), out LineLeft, out ColLeft);
            int ColRight; 
            int LineRight;
            string right = NumberOfTab(bunka.Value.Substring(IndexOfOperand + 1), out LineRight, out ColRight);

            if(left ==null && right == null)
            {
                bunka.LineLeft = LineLeft;
                bunka.LineRight = LineRight;
                bunka.ColLeft = ColLeft;
                bunka.ColRight = ColRight;

                bunka.Sign = si;
                bunka.Value = "";

                bunka.Processed = false;
                return bunka;
            }
            else
            {
                bunka.Value = "#FORMULA";
                bunka.Processed = true;
                return bunka;
            }
            
        }

        public string NumberOfTab(string tabs, out int NumbLine, out int NumbColumn)
        {
            NumbLine = -1;
            NumbColumn = -1;
            int IndexOfOpernd = tabs.IndexOfAny(numbers);
            if (IndexOfOpernd == -1) return "#INVVAL";

            string col = tabs.Substring(0, IndexOfOpernd);
            string num = tabs.Substring(IndexOfOpernd);

            if (!int.TryParse(num, out NumbLine)) return "#INVVAL";
            if (!NumberOfColumn(col, out NumbColumn)) return "#INVVAL";
            NumbLine--;
            NumbColumn--;

            return null;
        }

        public bool NumberOfColumn(string column, out int numberColumn)
        {
            numberColumn = 0;
            int k = 0;
            for (int i = column.Length - 1; i >= 0; i--)
            {
                int value = (int)column[i] - 64;
                if (value > 26 || value < 1) return false;
                numberColumn += value * ((int)Math.Pow(26, k));
                k++;
            }
            return true;
        }
    }
    class Process
    {
        private List<List<Tab>> tab;
        private StringBuilder str;
        private Stack<Tab> stack;

        public Process(List<List<Tab>> tab)
        {
            this.tab = tab;
           
            str = new StringBuilder();
            stack = new Stack<Tab>();
        }

        public List<List<Tab>> Complete()
        {
            for (int i = 0; i < tab.Count; i++)
            {
                for (int l = 0; l < tab[i].Count; l++)
                {
                    if (tab[i][l].Processed) continue;
                    
                    MainProcess(tab[i][l], i, l);
                }
            }
            return tab;
        }

        public void MainProcess(Tab bunka, int i, int l)
        {
            string rightOp="";
            string leftOp="";
            stack.Push(bunka);
            while (stack.Count > 0)
            {
                Tab bun = stack.Pop();
               
                if(bun.LineLeft >=0 && bun.LineLeft<tab.Count && bun.ColLeft>=0 && bun.ColLeft< tab[bun.LineLeft].Count)
                {
                        if (tab[bun.LineLeft][bun.ColLeft].Processed)
                        {
                            leftOp = tab[bun.LineLeft][bun.ColLeft].Value;
                        }
                        else
                        {
                            if (!stack.Contains(bun))
                            {
                                stack.Push(bun);
                                stack.Push(tab[bun.LineLeft][bun.ColLeft]);
                                continue;
                            }
                        Tab bunk = stack.Pop();
                        while (bunk != bun)
                        {
                            bunk.Value = "#CYCLE";
                            bunk.Processed = true;
                            bunk = stack.Pop();

                        }
                        bunk.Value = "#CYCLE";
                        bunk.Processed = true;
                        continue;
                    }
                }
                else
                {
                    leftOp = "0";
                }

                if (bun.LineRight >= 0 && bun.LineRight < tab.Count && bun.ColRight >= 0 && bun.ColRight < tab[bun.LineRight].Count)
                {
                    if (tab[bun.LineRight][bun.ColRight].Processed)
                    {
                        rightOp = tab[bun.LineRight][bun.ColRight].Value;
                    }
                    else
                    {
                        if (!stack.Contains(bun))
                        {
                            stack.Push(bun);
                            stack.Push(tab[bun.LineRight][bun.ColRight]);
                            continue;
                        }
                        Tab bunk = stack.Pop();
                        while (bunk != bun)
                        {
                            bunk.Value = "#CYCLE";
                            bunk.Processed = true;
                            bunk = stack.Pop();

                        }
                        bunk.Value = "#CYCLE";
                        bunk.Processed = true;
                        continue;
                    }
                }
                else
                {
                    rightOp = "0";
                }

                //if(leftOp == "#CYCLE" || rightOp == "#CYCLE")
                //{
                //    bun.Value = "#CYCLE";
                //    bun.Processed = true;
                //    continue;
                //}
                char Operand = bun.Sign;
                if (leftOp == "[]") leftOp = "0";
                if (rightOp == "[]") rightOp = "0";
                if ((rightOp == "[]" || rightOp == "0") && Operand == '/' && leftOp[0] != '#')
                {
                    bun.Value = "#DIV0";
                    bun.Processed = true;
                    continue;
                }
                int LNumber=0; int RNumber=0;
                if (leftOp[0] == '#' || rightOp[0] == '#' || !int.TryParse(leftOp, out LNumber) || !int.TryParse(rightOp, out RNumber))
                {
                    bun.Value = "#ERROR";
                    bun.Processed = true;
                    continue;
                }

                switch (Operand)
                {
                    case '+':
                        bun.Value = (LNumber + RNumber).ToString();
                        break;
                    case '-':
                        bun.Value = (LNumber - RNumber).ToString();
                        break;
                    case '/':
                        bun.Value = (LNumber / RNumber).ToString();
                        break;
                    case '*':
                        bun.Value = (LNumber * RNumber).ToString();
                        break;
                }
                bun.Processed = true;
            }
        }
    }
        class WriteTab
    {
        private List<List<Tab>> tab;
        public WriteTab(List<List<Tab>> tab)
        {
            this.tab = tab;
        }

        public void Write(TextWriter OutFile)
        {
            for (int i = 0; i < tab.Count; i++)
            {
                for (int l = 0; l < tab[i].Count; l++)
                {
                    if (l == tab[i].Count - 1)
                    {
                        OutFile.Write(tab[i][l].Value);
                    }
                    else OutFile.Write(tab[i][l].Value + " ");
                }
                if (i < tab.Count - 1)
                    OutFile.WriteLine();
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Argument Error");
                return;
            }
            try
            {
                StreamReader reader = new StreamReader(args[0]);
                StreamWriter writer = new StreamWriter(args[1]);
                ReadTab readTab = new ReadTab();
                var tab = readTab.Read(reader);
                Process process = new Process(tab);
                WriteTab writeTab = new WriteTab(process.Complete());
                writeTab.Write(writer);
                reader.Close();
                writer.Close();
            }
            catch
            {
                Console.WriteLine("File Error");
            }
        }
    }
}

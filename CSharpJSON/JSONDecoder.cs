using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpJSON
{
    
    public class JSONElement
    {
        public JSONElement(BracketStyle S, int Pos = 0)
        {
            Style = S;
        }
        public enum BracketStyle { Array, Object, Quote, Property, Value, Constant };
        public BracketStyle Style;
        public List<JSONElement> Children = new List<JSONElement>();
        public string Text = "";

        public void RemoveChild(JSONElement B)
        {
            Children.Remove(B);
        }

        public JSONElement this[int i]
        {
            get
            {
                if (Style == BracketStyle.Constant)
                    return this;

                if (Style == BracketStyle.Array)
                {
                    return Children[i];
                }
                if (Style == BracketStyle.Object)
                {
                    string Srch = i.ToString();
                    foreach (JSONElement C in Children)
                    {
                        if (C.Style == BracketStyle.Property)
                        {
                            if (C.Text == Srch)
                            {
                                return C.Children[0];
                            }
                        }
                    }
                }
                return null;
            }
        }

        public JSONElement this[string s]
        {
            get
            {
                if (Style == BracketStyle.Constant)
                    return this;

                if (Style == BracketStyle.Object)
                {
                    foreach (JSONElement C in Children)
                    {
                        if (C.Style == BracketStyle.Property)
                        {
                            if (C.Text == s)
                            {
                                return C.Children[0];
                            }
                        }
                    }
                }
                return null;
            }
        }

        public IEnumerator<JSONElement> GetEnumerator()
        {           
            foreach (JSONElement E in Children)
            {
                yield return E;
            }
        }
    }

    public class JSONDecoder
    {
        public JSONElement this[string s]
        {
            get
            {
                return Results[0][s];
            }
        }

        public JSONElement this[int i]
        {
            get
            {
                return Results[0][i];
            }
        }

        class BracketManager : List<JSONElement>
        {
            JSONElement Current = null;

            public void AddBracket(JSONElement B, JSONElement Parent = null)
            {
                if (Parent != null)
                {
                    Parent.Children.Add(B);
                    return;
                }
                Add(B);
                if (B.Style != JSONElement.BracketStyle.Quote)
                {
                    Current = B;
                }
            }

            public void FlattenQuote(JSONElement B)
            {
                Current.Text = B.Text;
                Current.RemoveChild(B);
            }

        }

        BracketManager Results = new BracketManager();

        public JSONDecoder(string S)
        {
            //base(JSONElement.BracketStyle.Object);
            List<JSONElement> BracketStack = new List<JSONElement>();

            int ln = S.Length;
            bool Quoted = false;
            bool Escaped = false;

            for (int x = 0; x < ln; x++)
            {
                if (S[x] == '/')
                {
                    Escaped = true;
                    continue;
                }
                if (S[x] == '"')
                {
                    if (Escaped == true)
                        continue;

                    if (BracketStack.Count > 0)
                    {
                        if (BracketStack[BracketStack.Count - 1].Style == JSONElement.BracketStyle.Quote)
                        {
                            Quoted = false;
                            BracketStack[BracketStack.Count - 2].Text = BracketStack[BracketStack.Count - 1].Text;
                            BracketStack.RemoveAt(BracketStack.Count - 1);

                            //Quoted elements actually get removed from the final brackets...                            
                            continue;
                        }
                    }
                    JSONElement B = new JSONElement(JSONElement.BracketStyle.Quote);
                    BracketStack.Add(B);
                    /*try
                    {
                        Results.AddBracket(B, BracketStack[BracketStack.Count - 2]);
                    }
                    catch
                    {
                        Results.AddBracket(B);
                    }*/

                    continue;
                }
                if (S[x] == '[')
                {
                    if (Quoted != true)
                    {
                        JSONElement B = new JSONElement(JSONElement.BracketStyle.Array);
                        BracketStack.Add(B);
                        try
                        {
                            Results.AddBracket(B, BracketStack[BracketStack.Count - 2]);
                        }
                        catch
                        {
                            Results.AddBracket(B);
                        }

                        B = new JSONElement(JSONElement.BracketStyle.Value);
                        BracketStack.Add(B);
                        try
                        {
                            Results.AddBracket(B, BracketStack[BracketStack.Count - 2]);
                        }
                        catch
                        {
                            Results.AddBracket(B);
                        }
                        continue;
                    }
                }
                if (S[x] == ']')
                {
                    if (Quoted != true)
                    {
                        BracketStack.RemoveAt(BracketStack.Count - 1);
                        continue;
                    }
                }
                if (S[x] == '{')
                {
                    if (Quoted != true)
                    {

                        JSONElement B = new JSONElement(JSONElement.BracketStyle.Object);
                        BracketStack.Add(B);
                        try
                        {
                            Results.AddBracket(B, BracketStack[BracketStack.Count - 2]);
                        }
                        catch
                        {
                            Results.AddBracket(B);
                        }

                        B = new JSONElement(JSONElement.BracketStyle.Property);
                        BracketStack.Add(B);
                        try
                        {
                            Results.AddBracket(B, BracketStack[BracketStack.Count - 2]);
                        }
                        catch
                        {
                            Results.AddBracket(B);
                        }

                        continue;
                    }
                }
                if (S[x] == '}')
                {
                    if (Quoted != true)
                    {
                        BracketStack.RemoveAt(BracketStack.Count - 1);
                        BracketStack.RemoveAt(BracketStack.Count - 1);
                        //if (BracketStack[BracketStack.Count - 1].Style == JSONElement.BracketStyle.Property)
                        // {
                        //}

                        continue;
                    }
                }
                if (S[x] == ':')
                {
                    if (Quoted != true)
                    {
                        if (BracketStack[BracketStack.Count - 1].Style == JSONElement.BracketStyle.Property)
                        {

                            JSONElement B = new JSONElement(JSONElement.BracketStyle.Value);
                            BracketStack.Add(B);

                            try
                            {
                                Results.AddBracket(B, BracketStack[BracketStack.Count - 2]);
                            }
                            catch
                            {
                                Results.AddBracket(B);
                            }
                            BracketStack.RemoveAt(BracketStack.Count - 2);
                            continue;
                        }
                    }
                }
                if (S[x] == ',')
                {
                    if (Quoted != true)
                    {
                        if (BracketStack[BracketStack.Count - 1].Style == JSONElement.BracketStyle.Value)
                        {
                            BracketStack.RemoveAt(BracketStack.Count - 1);
                            if (BracketStack[BracketStack.Count - 1].Style == JSONElement.BracketStyle.Object)
                            {
                                JSONElement B = new JSONElement(JSONElement.BracketStyle.Property);
                                BracketStack.Add(B);
                                try
                                {
                                    Results.AddBracket(B, BracketStack[BracketStack.Count - 2]);
                                }
                                catch
                                {
                                    Results.AddBracket(B);
                                }
                            }
                            else
                            {
                                JSONElement B = new JSONElement(JSONElement.BracketStyle.Value);
                                BracketStack.Add(B);
                                try
                                {
                                    Results.AddBracket(B, BracketStack[BracketStack.Count - 2]);
                                }
                                catch
                                {
                                    Results.AddBracket(B);
                                }
                            }
                        }
                        else
                        {
                            if (BracketStack[BracketStack.Count - 1].Style == JSONElement.BracketStyle.Property)
                            {
                                Console.WriteLine("Property with No Value?");
                            }
                        }
                        continue;
                    }

                }

                BracketStack[BracketStack.Count - 1].Text += S[x];
                Escaped = false;
            }

            RemoveValueElements();
        }

        class BracketGuide
        {
            public JSONElement Bracket;
            public JSONElement Parent;
            public BracketGuide(JSONElement ob, JSONElement par)
            {
                Parent = par;
                Bracket = ob;
            }
        }
        void RemoveValueElements()
        {
            List<BracketGuide> ToProcess = new List<BracketGuide>();
            foreach (JSONElement B in Results)
            {
                ToProcess.Add(new BracketGuide(B, null));
            }

            int x = 0;
            while (x < ToProcess.Count)
            {
                JSONElement Bx = ToProcess[x].Bracket;
                if (ToProcess[x].Bracket.Style == JSONElement.BracketStyle.Value)
                {
                    if (ToProcess[x].Bracket.Children.Count > 0)
                    {
                        ToProcess[x].Parent.Children.Clear();
                        ToProcess[x].Parent.Children = Bx.Children;
                    }
                    else
                    {
                        ToProcess[x].Bracket.Style = JSONElement.BracketStyle.Constant;
                    }
                }

                foreach (JSONElement B in Bx.Children)
                {
                    bool Found = false;
                    foreach (BracketGuide BG in ToProcess)
                    {
                        if (BG.Bracket == B)
                        {
                            Found = true;
                            break;
                        }
                    }
                    if (Found == false)
                    {
                        ToProcess.Add(new BracketGuide(B, Bx));
                    }
                }
                x++;
            }
        }

        
    }
}

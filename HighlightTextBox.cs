using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SYD.Acccess.MultilineSql
{
    public partial class HighlightTextBox : RichTextBox
    {
        string[] greenColorStr = new string[] { "insert", "into", "select","as","values","dmax","dmin","max","min","count","sum","order","by","where","is","null","isnull","from" };
        string[] strings = { @"'((.|\n)*?)'" };
        string[] whiteSpace = { "\t", "\n", "	" };
        [DllImport("user32")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int Wparam, IntPtr IParam);
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            SendMessage(base.Handle, 0xB, 0, IntPtr.Zero);
            int sIndex = this.SelectionStart;
            this.SelectAll();
            this.SelectionColor = Color.Black;
            this.Select(sIndex, 0);
            HighLightText(greenColorStr,Color.Green);
            HighLightText(strings, Color.Blue);
            HighLightText(whiteSpace, Color.Black);
            this.Select(sIndex, 0);
            this.SelectionColor = Color.Black;
            SendMessage(base.Handle, 0xB, 1, IntPtr.Zero);
            this.Refresh();
            
        }
        private void ChangeColor(string text, Color color)
        {
            int s = 0;
            while ((-1 + text.Length - 1) != (s = text.Length - 1 + this.Find(text, s, -1,  RichTextBoxFinds.WholeWord)))
            {
                this.SelectionColor = color;
            }
        }
        private void HighLightText(string[] wordList, Color color)
        {
            foreach (string word in wordList)
            {
                Regex re = new Regex(word, RegexOptions.IgnoreCase);
                foreach (Match m in re.Matches(this.Text))
                {
                    this.Select(m.Index, m.Length);
                    this.SelectionColor = color;
                }
            }
        }
    }
}

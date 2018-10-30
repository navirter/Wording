using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using My;

namespace Wording
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Word[] words = null;
        private void button1_Click(object sender, EventArgs e)
        {
            string source = textBox3.Text;
            var separators = textBox2.Lines.ToList();
            separators.ForEach(s => s = s.Trim());
            separators = separators.Where(s => s.Length > 0).ToList();
            //if (separators.Count == 0)
            //    separators.Add(" ");
            if (checkBox6.Checked)
                separators.AddRange(new[] { "\n", "\r" });
            if (checkBox7.Checked)
                separators.Add(" ");
            if (separators.Count == 0)
            {
                var holder = new List<string>(new[] { "Нет", "Разделителей" });
                showWords(holder);
                finalize(holder, 2, 2);
                return;
            }
            Settings settings = new Settings(separators.ToArray(), checkBox1.Checked, checkBox2.Checked,
                checkBox3.Checked, checkBox4.Checked);
            words = settings.filterAndSort(source);
            int amount = 0; int totalAmount = 0;
            var strings = Word.getStrings(true, radioButton1.Checked, words, out amount, out totalAmount);
            finalize(strings, amount, totalAmount);
        }
        void finalize(IEnumerable<string> words, int amount, int totalAmount)
        {
            listBox1.Items.Clear();
            label3.Text = totalAmount + "=>" + amount;
            showWords(words);
            get_string_to_copy();
        }
        void showWords(IEnumerable<Word> words)
        {
            int amount = 0; int totalAmount = 0;
            var news = Word.getStrings(true, radioButton1.Checked, words.ToArray(), out amount, out totalAmount);
            showWords(news);
        }
        void showWords(IEnumerable<string> words)
        {
            listBox1.Items.Clear();
            foreach (var v in words)
                listBox1.Items.Add(v);
        }
        class Word
        {
            public string word;
            public string initialWord;
            public int amount;
            public Word(string Word, string InitialWord, int Amount)
            {
                word = Word;
                initialWord = InitialWord;
                amount = Amount;
            }
            public Word(Word word)
            {
                this.word = word.word;
                this.initialWord = word.initialWord;
                this.amount = word.amount;
            }


            public static List<string> getStrings(bool withAmounts, bool alphabethic, Word[] words, out int amount, out int totalAmount)
            {
                amount = 0;
                totalAmount = 0;
                if (words == null || words.Length == 0)
                    return new List<string>();
                List<string> res = new List<string>();
                if (alphabethic)
                {
                    words = words.OrderBy(s => s.word).ToArray();
                    foreach (var v in words)
                    {
                        if (v.word.Trim() == "") continue;
                        string toadd = v.word;
                        if (withAmounts)
                            toadd += "(кол.=" + v.amount + ")";
                        res.Add(toadd);
                        amount++;
                        totalAmount += v.amount;
                    }
                }
                else
                {
                    words = words.OrderBy(s => s.amount).Reverse().ToArray();
                    foreach (var v in words)
                    {
                        string toadd = v.word;
                        if (withAmounts)
                            toadd += "(кол.=" + v.amount + ")";
                        res.Add(toadd);
                        amount++;
                        totalAmount += v.amount;
                    }
                }
                return res;
            }

            public override string ToString()
            {

                return base.ToString();
            }
        }
        class Settings
        {
            public Settings( string[] Separators, bool ConsiderCapital, bool ConsiderNumbers,
                bool ConsiderSigns, bool RemoveDublicates)
            {
                separators = Separators;
                considerCapital = ConsiderCapital;
                considerNumbers = ConsiderNumbers;
                considerSigns = ConsiderSigns;
                removeDublicates = RemoveDublicates;
            }
            public string[] separators;
            public bool considerCapital;
            public bool considerNumbers;
            public bool considerSigns;
            public bool removeDublicates;

            public Word[] filterAndSort(string source)
            {
                try
                {
                    string[] initialSplits = new string[0];
                    string[] sep = separators ;
                    initialSplits = source.Split(separators , StringSplitOptions.RemoveEmptyEntries);                    
                    for (int i = 0; i < initialSplits.Length; i++)
                        initialSplits[i] = initialSplits[i].Trim();
                    var processed = processSplits(initialSplits);
                    var noDublicates = undublicateAndSort(processed);
                    //var res = getInitialWords(noDublicates, initialSplits);
                    var res = sort(noDublicates);
                    return res;
                }
                catch (Exception e)
                {

                }
                return new Word[0];
            }
            Word[] processSplits(string[] initialSplits)
            {
                List<Word> res = new List<Word>();
                foreach (var split in initialSplits) res.Add(new Word("", split, 1));
                for (int i = 0; i < res.Count; i++)
                {
                    res[i].word = res[i].initialWord;
                    if (!considerCapital) res[i].word = res[i].word.ToLower();
                    if (!considerNumbers)
                        res[i].word = new string(res[i].word.ToCharArray().ToList()
                            .FindAll(s => !Char.IsDigit(s)).ToArray());
                    if (!considerSigns)
                        res[i].word = new string(res[i].word.ToCharArray().ToList()
                            .FindAll(s => Char.IsDigit(s) || Char.IsLetter(s) || s == ' ').ToArray());
                    res[i].initialWord = res[i].initialWord.Trim();
                }
                return res.ToArray();
            }
            Word[] undublicateAndSort(Word[] values)
            {
                List<Word> res = new List<Word>();
                if (removeDublicates && values.Length > 1)
                {
                    List<Word> noDublicates = new List<Word>();
                    for (int i = 0; i < values.Length; i++)
                    {
                        bool add = true;
                        if (noDublicates.Count > 0)
                            for (int j = 0; j < noDublicates.Count; j++)
                                if (noDublicates[j].word == values[i].word)
                                {
                                    add = false;
                                    noDublicates[j].amount++;
                                    break;
                                }
                        if (add)
                            noDublicates.Add(values[i]);
                    }
                    res = noDublicates;
                }
                else
                    res = new List<Word>(values);

                var resarr = sort(res.ToArray());
                return resarr;
            }
            Word[] sort(Word[] words)
            {
                List<Word> res = new List<Word>();
                List<string> wordo = new List<string>();
                foreach (var v in words) wordo.Add(v.word);
                wordo.Sort();
                foreach (var w in wordo)
                    foreach (var v in words)
                        if (w == v.word)
                        {
                            res.Add(v);
                            break;
                        }
                return res.ToArray();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string result = get_string_to_copy();
            Clipboard.SetText(result);
            //Clipboard.SetText(textBox4.Text);
        }
        string get_string_to_copy()
        {
            label6.Text = "0";
            List<string> strings = new List<string>();
            List<int> ints = new List<int>();
            int minimumAmount = Convert.ToInt32(numericUpDown1.Value);
            foreach (var v in listBox1.Items)
                try
                {
                    string s = v.ToString();
                    string[] si = s.Split(new[] { "(кол.=" }, StringSplitOptions.RemoveEmptyEntries);
                    string word = si[0];
                    int amount = int.Parse(si[1].Remove(si[1].Length - 1).Trim());
                    if (minimumAmount > 0 && amount < minimumAmount) continue;
                    strings.Add(word);
                    ints.Add(amount);
                }
                catch { }
            string result = "";
            if (checkBox5.Checked == false)
                foreach (var v in strings)
                    result += "\r\n" + v;
            else
                for (int i = 0; i < strings.Count; i++)
                    result += "\r\n" + strings[i] + "(" + ints[i] + ")";
            result = result.Remove(0, 2);
            label6.Text = strings.Count.ToString();
            return result;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormClosing += new FormClosingEventHandler(close);
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                textBox3.Text = File.ReadAllText(path + "\\last.txt", Encoding.GetEncoding(1251));
            }
            catch(Exception ee)
            { MessageBox.Show(ee.Message, "Текст скопирован"); Clipboard.SetText(ee.Message); }
        }

        void close(object sender, EventArgs e)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Directory.CreateDirectory(path);
                File.WriteAllText(path + "\\last.txt", textBox3.Text, Encoding.GetEncoding(1251));
            }
            catch { }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == false)
                return;
            showWords(words);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton2.Checked)
                return;
            showWords(words);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                get_string_to_copy();
            }
            catch { }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                Directory.CreateDirectory(path);
                File.WriteAllText(path + "\\last.txt", textBox3.Text, Encoding.GetEncoding(1251));
            }
            catch { }
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        void set_default_settings()
        {
            try
            {
                checkBox6.Checked = false;
                checkBox7.Checked = false;
                textBox2.Text = "";
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                checkBox4.Checked = true;
                radioButton1.Checked = true;
                radioButton2.Checked = false;
                checkBox5.Checked = false;
                numericUpDown1.Value = 0;
            }
            catch { }
        }
    }
}

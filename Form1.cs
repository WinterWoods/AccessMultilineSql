using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace SYD.Acccess.MultilineSql
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            isOpen = false;

        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            btnClose.Enabled = false;
            btnCommit.Enabled = false;
            btnRollback.Enabled = false;
            btnExec.Enabled = false;
            tbMsg.Text = "";
        }
        OleDbConnection conn;
        bool isOpen;
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbDBPath.Text)) { WritMsg("数据库路径不能为空"); return; }
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbDBPath.Text = openFileDialog1.FileName;
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!File.Exists(tbDBPath.Text)) { WritMsg("数据库不存在"); return; }
            if (tbDBPath.Text.Substring(tbDBPath.Text.Length - 3) != "mdb") { WritMsg("不是access数据库"); return; }
            if (string.IsNullOrEmpty(tbPassword.Text))
            {
                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + tbDBPath.Text + ";persist Security Info=true");
            }
            else
            {
                conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + tbDBPath.Text + ";persist Security Info=true;Password=" + tbPassword.Text);
            }
            try
            {
                conn.Open();
                isOpen = true;
                btnOpen.Enabled = false;
                btnClose.Enabled = true;
                btnExec.Enabled = true;
                tbDBPath.Enabled = false;
                btnSelectPath.Enabled = false;
                tbPassword.Enabled = false;
                WritMsg("数据库打开成功!");
            }
            catch (Exception ex)
            {
                WritMsg("数据库打开异常:" + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (isOpen)
            {
                try
                {
                    Rollback();
                    conn.Close();
                    isOpen = false;
                    btnOpen.Enabled = true;
                    btnClose.Enabled = false;
                    btnSelectPath.Enabled = true;
                    tbPassword.Enabled = true;
                    tbDBPath.Enabled = true;
                    WritMsg("数据库关闭成功!");
                }
                catch (Exception ex)
                {
                    WritMsg("数据库关闭异常:" + ex.Message);
                }
            }
        }
        OleDbTransaction tran;
        private void btnExec_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbSql.Text)) { WritMsg("要执行的sql为空"); return; }
            if (tran == null)
            {
                tran = conn.BeginTransaction();
                
            }
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;
            
            cmd.Transaction = tran;
            cmd.CommandType = CommandType.Text;
            try
            {
                //cmd.CommandText = tbSql.Text;
                //int cmdnum = cmd.ExecuteNonQuery();
                WritMsg(ExecuteMultilineSql(tbSql.Text, cmd));
                btnCommit.Enabled = true;
                btnRollback.Enabled = true;
            }
            catch (Exception ex)
            {
                WritMsg("执行sql异常:" + ex.Message);
                Rollback();
            }
        }
        private string ExecuteMultilineSql(string sql,OleDbCommand cmd)
        {
            string[] mutilineSql=sql.Split(new char[]{';'},StringSplitOptions.None);
            int execNum = 0;
            int iIndex=0;
            string befStr=string.Empty;
            ArrayList AllSql=new ArrayList();
            AllSql.Clear();
            foreach (string str in mutilineSql)
            {
                if (string.IsNullOrEmpty(str.Trim())) continue;
                Regex rx=new Regex("insert|select|update|delete*");
                Match m = rx.Match(str.Trim());
                if (!m.Success&&iIndex!=0)
                {
                    AllSql.Remove(befStr);
                    AllSql.Add(befStr +";"+ str);
                }
                else
                {
                    AllSql.Add(str);
                }
                iIndex++;
                befStr=str;
            }
            int exSqlNum = 0;
            foreach (string str in AllSql)
            {
                try
                {
                    cmd.CommandText = str;
                    execNum += cmd.ExecuteNonQuery();
                    exSqlNum++;
                }
                catch (Exception ex)
                {
                    WritMsg("执行SQL:\"" + str + "\"异常:" + ex.Message);
                    break;
                }
            }
            return "SQL语句共:" + iIndex + "条,执行语句共:" + exSqlNum + "条,共影响" + execNum + "条数据";
        }
        private void WritMsg(string msg)
        {
            MessageBox.Show(msg);
            tbMsg.Text += msg + Environment.NewLine;
            tbMsg.SelectionStart = tbMsg.Text.Length;
            tbMsg.ScrollToCaret();
        }

        private void btnSub_Click(object sender, EventArgs e)
        {
            try
            {
                Commit();
            }
            catch (Exception ex)
            {
                WritMsg("执行sql异常:" + ex.Message);
            }
        }

        private void btnRec_Click(object sender, EventArgs e)
        {
            try
            {
                Rollback();
            }
            catch (Exception ex)
            {
                WritMsg("执行sql异常:" + ex.Message);
            }
        }
        private void Commit()
        {
            if (tran != null)
            {
                tran.Commit();
                
            }
            tran = null;
            btnRollback.Enabled = false;
            btnCommit.Enabled = false;
        }
        private void Rollback()
        {
            if (tran != null)
            {
                tran.Rollback();
                
            }
            btnRollback.Enabled = false;
            btnCommit.Enabled = false;
            tran = null;
        }
        private bool CloseYZ()
        {
            if (tran != null)
            {
                if (MessageBox.Show("有sql没有提交,确定回滚?", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    Rollback();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(tbSql.Text))
            {
                if (MessageBox.Show("确定关闭", "提示", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    return false;
                }
            }
            tbMsg.Text = "";
            return true;
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseYZ();
        }
        
    }
    
}

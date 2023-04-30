using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using TShockAPI.DB;
using Terraria.DataStructures;
using System.Data;
using System.Timers;

namespace TestPlugin
{

    class LC//本地配置
    {
        public static ConfigFile LConfig { get; set; }//配置文件
        internal static string LConfigPath { get { return Path.Combine(TShock.SavePath, "饥荒RPG.json"); } }//配置文件路径
        public static LPlayer[] LPlayers { get; set; }//L表示local本地的意思，跨库需静态
        public static LNPC[] LNpcs { get; set; }//创建一个本地玩家的集合
        public static DateTime LastDataTiem { get; set; }
        public static int CheckTime { get; set; }//
        internal static string ServerExpPath { get { return Path.Combine(TShock.SavePath, "服务器经验.txt"); } }//服务器经验文件路径
        public static int ServeLeve { get; set; }//服务器等级
        public static long ServeExp { get; set; }//服务器经验
        public static long ServeDayExp { get; set; }//服务器日经验
        public static string ServeExpDayTimeText { get; set; }//服务器日经验文本
        public static  DateTime ServeExpLastDataTime { get; set; }//服务器经验末次同步时间

        public static void AddSE()//增加服务器经验
        {
            if(!LConfig.启动公共等级) return;
            if (ServeLeve > LConfig.公共最高等级) return;

            string timetext = DateTime.Now.ToString("yyyy-MM-dd");
            if (timetext != ServeExpDayTimeText)
            {
                ServeExpDayTimeText = timetext;
                ServeDayExp = 0;
            }


            if (LC.LConfig.公共日最高经验 != -1)//肉后
                if (LC.LConfig.公共日最高经验 <= ServeDayExp) return;
            if (LC.LConfig.公共最高等级 != -1)
                if (LC.LConfig.公共最高等级 <= ServeLeve) return;


            if (Main.hardMode)//等级经验限制
            {
                if (!NPC.downedGolemBoss)//石巨人没杀
                    if (LC.LConfig.公共巨人日最高经验 != -1)
                        if (LC.LConfig.公共巨人日最高经验 <= ServeDayExp) return;

                if (!NPC.downedGolemBoss)//石巨人没杀
                    if (LC.LConfig.公共巨人最高等级 != -1)
                        if (LC.LConfig.公共巨人最高等级 <= ServeLeve) return;
            }
            else 
            {
                if (LC.LConfig.公共肉前日最高经验 != -1)//肉前
                    if (LC.LConfig.公共肉前日最高经验 <= ServeDayExp) return;

                if (LC.LConfig.公共肉前最高等级 != -1)
                    if (LC.LConfig.公共肉前最高等级 <= ServeLeve) return;
            }

            ServeDayExp++;
            ServeExp++;

            long e = Sundry.ComputeExp(ServeLeve + 1);//下级的最低经验

            if (e <= ServeExp)
            {
                ServeLeve++;
                TShock.Utils.Broadcast( "服务器公共等级已升级到" + ServeLeve + "级！！！", 255, 215, 0);
            }

            if ((DateTime.UtcNow - ServeExpLastDataTime).TotalMilliseconds < 5000) return;//5秒只同步一次
            ServeExpLastDataTime = DateTime.UtcNow;
            SaveSE();

        }
        public static bool SEDM()//服务器日经验是否最大值
        {

            string timetext = DateTime.Now.ToString("yyyy-MM-dd");
            if (timetext != ServeExpDayTimeText)
            {
                ServeExpDayTimeText = timetext;
                ServeDayExp = 0;
            }

            if (LC.LConfig.公共日最高经验 != -1)//肉后
                if (LC.LConfig.公共日最高经验 <= ServeDayExp) return true;

            if (Main.hardMode)//等级经验限制
            {
                if (!NPC.downedGolemBoss)//石巨人没杀
                    if (LC.LConfig.公共巨人日最高经验 != -1)
                        if (LC.LConfig.公共巨人日最高经验 <= ServeDayExp) return true;
            }
            else if (LC.LConfig.公共肉前日最高经验 != -1)//肉前
            {
                if (LC.LConfig.公共肉前日最高经验 <= ServeDayExp) return true;
            }

            return false;
        }
        public static bool SELM()//服务器等级是否最大值
        {
            if (LC.LConfig.公共最高等级 != -1)
                if (LC.LConfig.公共最高等级 <= ServeLeve) return true;

            if (Main.hardMode)//等级经验限制
            { 
                if (!NPC.downedGolemBoss)//石巨人没杀
                    if (LC.LConfig.公共巨人最高等级 != -1)
                        if (LC.LConfig.公共巨人最高等级 <= ServeLeve) return true;

            }
            else if (LC.LConfig.公共肉前最高等级 != -1)//肉前
            {
                if (LC.LConfig.公共肉前最高等级 <= ServeLeve) return true;
            }

            return false;
        }
        public static string SEDMP()//当日经验百分比文本
        {
                if (Main.hardMode)//经验限制
                {
                    if (!NPC.downedGolemBoss)//石巨人没杀
                        if (LC.LConfig.公共巨人日最高经验 != -1)
                            if (LC.LConfig.公共巨人日最高经验 > ServeDayExp) return " 余:" + (100 - ServeDayExp * 100 / LC.LConfig.公共巨人日最高经验) + "%";

                    if (LC.LConfig.公共日最高经验 != -1)//肉后
                        if (LC.LConfig.公共日最高经验 > ServeDayExp) return " 余:" + (100 - ServeDayExp * 100 / LC.LConfig.公共日最高经验) + "%";

                }
                else if (LC.LConfig.公共肉前日最高经验 != -1)//肉前
                    if (LC.LConfig.公共肉前日最高经验 > ServeDayExp) return " 余:" + (100 - ServeDayExp * 100 / LC.LConfig.公共肉前日最高经验) + "%";


            return "";
        }


        public static void SaveSE()//保存服务器经验
        {
            using (var tw = new StreamWriter(ServerExpPath))
            {
                string text = "";
                text += ServeLeve + "|";
                text += ServeExp + "|";
                text += ServeDayExp + "|";
                text += ServeExpDayTimeText;
                tw.Write(text);
            }

        }
        public static void RSE()//重读服务器经验
        {
            if (!File.Exists(ServerExpPath)) File.Create(ServerExpPath).Close(); ;//如果没有这个文件就创建这个文件

            


            using (var tr = new StreamReader(ServerExpPath))
            {
                string text = tr.ReadToEnd();
                string[] line = text.Split('|');

                //Console.WriteLine(text);
                //Console.WriteLine("测试"+line.Length+"试试"+ line.Count());

                if (line.Length != 4)
                {
                    ServeLeve = 0;
                    ServeExp = 0;
                    ServeDayExp = 0;
                    ServeExpDayTimeText = "";
                }
                else
                {
                    ServeLeve = int.Parse(line[0]);
                    ServeExp=long.Parse(line[1]);
                    ServeDayExp= long.Parse(line[2]);
                    ServeExpDayTimeText= line[3];
                }

            }
        }

        public static void RI()//初始化
        {
            LNpcs = new LNPC[201];
            LPlayers = new LPlayer[256]; 
            LConfig = new ConfigFile();
            LastDataTiem = DateTime.UtcNow;
            ServeExpLastDataTime = DateTime.UtcNow;
        }

        public static void RC()//读取/重读配置文件
        {
            try
            {
                if (!File.Exists(LConfigPath)) TShock.Log.ConsoleError("未找到饥荒RPG配置文件，已为您创建！");//检测提示
                LConfig = ConfigFile.Read(LConfigPath);// 读取配置并且自动补全配置
                //此处可以定义最小值


                LConfig.Write(LConfigPath);//无论如何读完后都会写一遍读到的内容
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError("饥荒RPG插件错误配置读取错误:" + ex.ToString());
            }
        }
        public static void RD()//读取
        {
            var table = new SqlTable("饥荒RPG",
                new SqlColumn("ID", MySqlDbType.Int32) { Primary = true },//唯一的主键玩家id
                new SqlColumn("经验", MySqlDbType.Int64),//长整数
                new SqlColumn("等级", MySqlDbType.Int32),
                new SqlColumn("状态", MySqlDbType.Text),//常驻状态，逗号分隔
                new SqlColumn("停状态", MySqlDbType.Int32),
                new SqlColumn("隐藏信息", MySqlDbType.Int32),
                new SqlColumn("任务", MySqlDbType.Int32),
                new SqlColumn("完成量", MySqlDbType.Int32),
                new SqlColumn("日经验", MySqlDbType.Int64),
                new SqlColumn("日文本", MySqlDbType.VarChar, 32),
                new SqlColumn("颜色", MySqlDbType.Text),
                new SqlColumn("前缀", MySqlDbType.Text),
                new SqlColumn("后缀", MySqlDbType.Text),
                new SqlColumn("召唤", MySqlDbType.Int32),
                new SqlColumn("入侵", MySqlDbType.Int32),
                new SqlColumn("日召唤", MySqlDbType.Int32),
                new SqlColumn("日入侵", MySqlDbType.Int32),
                new SqlColumn("超召唤", MySqlDbType.Int32),

                new SqlColumn("小队", MySqlDbType.Int32),//为id//为名字不是更好吗?答不好,因为小队会改名

                new SqlColumn("贡献经验", MySqlDbType.Int64),//长整数
                new SqlColumn("日贡经验", MySqlDbType.Int64)//长整
                );
            var SQLWriter = new SqlTableCreator(TShock.DB,
                TShock.DB.GetSqlType() == SqlType.Sqlite ?
                (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter.EnsureTableStructure(table);


            if (LConfig.启动小队部分)
            {
                var table2 = new SqlTable("小队部分",
                new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, AutoIncrement = true },//自增唯一的主键小队id
                new SqlColumn("Name", MySqlDbType.VarChar, 255) { Unique = true },//唯一的小队名称

                new SqlColumn("队长", MySqlDbType.Int32),//队长ID,意义不是很大

                new SqlColumn("经验", MySqlDbType.Int64),//长整数(预留经验)
                new SqlColumn("等级", MySqlDbType.Int32),
                new SqlColumn("状态", MySqlDbType.Text),//常驻状态，逗号分隔
                new SqlColumn("前缀", MySqlDbType.Text)//,

                //new SqlColumn("后缀", MySqlDbType.Text),//究竟有必要?
                //new SqlColumn("颜色", MySqlDbType.Text)//究竟有必要?
                );
                var SQLWriter2 = new SqlTableCreator(TShock.DB,
                    TShock.DB.GetSqlType() == SqlType.Sqlite ?
                    (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
                SQLWriter2.EnsureTableStructure(table2);
            }
        }

        


    }
}

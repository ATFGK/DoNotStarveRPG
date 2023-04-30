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
using Terraria.GameContent;

namespace TestPlugin
{
    public class LNPC//本地NPC类
    {
        public int Index { get; set; }
        public bool Dead { get; set; }
        public DateTime StartTiem { get; set; }//开始时间
        public Dictionary<string, int> Damage { get; set; }
        public LNPC(int index)
        {
            Index = index;
            Dead = false;
            StartTiem = DateTime.UtcNow;
            Damage = new Dictionary<string, int>();
        }
    }
    public class LDM//本地伤害类
    {
        public string Name { get; set; }
        public int Damage { get; set; }
        public int Proportion { get; set; }

        public LDM(string name, int damage)
        {
            Name = name;
            Damage = damage;
            Proportion = 0;
        }
    }

    public class LDL//LeveDamageLimit//等级伤害限制
    { 
        public int 起始等级 { get; set; }//包含//0为不限
        public int 结束等级 { get; set; }//包含//0为不限
        public int 伤害限制 { get; set; }//0为无限
        public int DPS限制 { get; set; }///0为无限
        public LDL(int fromL, int toL, int DL, int DPSL)
        {
            起始等级 = fromL;
            结束等级 = toL;
            伤害限制 = DL;
            DPS限制 = DPSL;
        }
    }
    public class LTask//本地任务类
    {
        public string  标题 { get; set; }
        public int 类型 { get; set; }//类型，0拥有，1击杀，2使用，3佩戴，4搜集，5对话，6死亡
        public int 目标ID { get; set; }//需要达成的id
        public int 目标数量 { get; set; }
        public int 经验 { get; set; }//奖励经验
        public string 剧情 { get; set; }

        /// <summary>
        /// 添加新任务:类型，0拥有，1击杀，2使用，3佩戴，4搜集，5对话，6死亡
        /// </summary>
        /// <param name="name">任务的标题</param>
        /// <param name="type">类型，0拥有，1击杀，2使用，3佩戴</param>
        /// <param name="objid">目标ID</param>
        /// <param name="exp">所奖励的经验</param>
        public LTask(string name,int type,int objid, int number ,int exp, string text ="")
        {
            标题 = name;
            类型 = type;
            目标ID = objid;
            目标数量 = number;
            经验 = exp;
            剧情 = text;
        }
    }


    public class LPlayer//本地玩家类
    {
        public int Who { get; set; }
        public int Leve { get; set; }//-1代表没读出来，
        public long Exp { get; set; }
        public bool StopBuff { get; set; }
        public bool HidingInfo { get; set; }
        public List<int> Buff { get; set; }

        public string Prefix { get; set; }//聊天前缀
        public string Suffix { get; set; }//聊天后缀
        public string ChatColor { get; set; }//聊天颜色

        public string CPrefix { get; set; }//自定义前缀
        public string CSuffix { get; set; }//自定义后缀
        public string CChatColor { get; set; }//自定义颜色


        public int TeamID { get; set; }//小队ID
        public int TeamLeve { get; set; }//小队等级
        public string TeamName { get; set; }//小队名字
        public long TeamExp { get; set; }//小队经验
        public long TeamDayExp { get; set; }//小队日经验
        //public string TeamDayTimeText { get; set; }//小队日时间文本,没必要,和普通经验一致即可
        public List<int> TeamBuff { get; set; }//小队状态
        public string TPrefix { get; set; }//小队前缀//位于自定义前缀优先级之后
        public int AutTeamExpTime { get; set; }//自动奖励时间

        public DateTime LastDataTime { get; set; }//末次同步时间
        public DateTime LastStrikeTime { get; set; }//末次击打时间
        public DateTime LastExpTime { get; set; }//末次提示经验时间
        public byte SelectedItem { get; set; }
        public int DisableItem { get; set; }//冻结时间
        public int TaskID { get; set; }//任务序号
        public int TaskNumber { get; set; }//任务数字累计
        public long DayExp { get; set; }//日经验
        public string DayTimeText { get; set; }//日时间文本
        public string GroupName { get; set; }
        public int AutExpTime { get; set; }//自动奖励时间
        public int HungerTime { get; set; }//进服不饿时间

        public int SummonBN { get; set; }//能够召唤boss数量
        public int InvadeN { get; set; }//能够引发入侵数量

        public int DaySummonBN { get; set; }//今日召唤boss数量
        public int DayInvadeN { get; set; }//今日引发入侵数量

        public int OverSummon { get; set; }//能够超越时间召唤boss的数量

        public DateTime DTiem { get; set; }//伤害踢误判时间
        public int DKick { get; set; }//伤害踢误判?
        public long DPS { get; set; }//秒伤
        public DateTime DPSLastTiem { get; set; }//秒伤时间

        //public int TempExp { get; set; }//临时经验

        //public bool InitSpawn { get; set; }
        //此处可以增加变量
        //要增加一个延时，防止经验写入数据库过快，在退出时写，和每隔1分钟写？

        public LPlayer(int who, int leve, long exp, List<int> buff,bool stopbuff,bool hidinginfo,int taskid,int tasknumber,long dayexp,string daytimetext,string prefix,string chatcolor,string suffix,int summonBN, int invadeN,int ds,int di,int os, int team, long texp, long tdayexp)//类初始化时
        {
            Who = who;
            Leve = leve;
            Exp = exp;
            Buff = buff;

            DKick = 0;
            DPS = 0;
            DTiem = DateTime.UtcNow;
            DPSLastTiem = DateTime.UtcNow;

            LastDataTime = DateTime.UtcNow;
            LastStrikeTime = DateTime.UtcNow;
            LastExpTime = DateTime.UtcNow;
            StopBuff = stopbuff;
            HidingInfo= hidinginfo;
            SelectedItem = 0;
            DisableItem = 5;//冻结时变为0
            TaskID= taskid;
            TaskNumber = tasknumber;
            DayExp = dayexp;
            DayTimeText=daytimetext;

            GroupName = "";


            CPrefix = prefix;
            CSuffix = suffix;
            CChatColor = chatcolor;

            SummonBN = summonBN;
            InvadeN = invadeN;

            DaySummonBN = ds;
            DayInvadeN = di;

            OverSummon = os;

            HungerTime =LC.LConfig .饥饿延迟;
            if (HungerTime < 3) HungerTime = 3;//最小3

            TeamID = team;
            TeamExp =texp;
            TeamDayExp = tdayexp;
            TeamName = "";
            TeamLeve = 0;
            TeamBuff = new List<int>();
            TPrefix = "";

            if (Leve != -1)
            {
                string timetext = DateTime.Now.ToString("yyyy-MM-dd");
                if (timetext != DayTimeText)
                {
                    DayTimeText = timetext;
                    DayExp = 0;TeamDayExp = 0;DaySS();

                    if (DayTimeText!="")
                        TShock.Players[Who].SendSuccessMessage("欢迎您回来,您的每日限制已更新!");
                }

                if (LC.LConfig.启动小队部分 && TeamID > 0)//此时需要读小队状态
                {
                    try
                    {
                        using (var SQLWriter = TShock.DB.QueryReader("SELECT * FROM 小队部分 WHERE ID=@0", TeamID))
                        {
                            if (SQLWriter.Read())//如果读出来了，
                            {
                                TeamName = SQLWriter.Get<string>("Name");
                                TeamLeve = SQLWriter.Get<int>("等级");
                                string text = SQLWriter.Get<string>("状态");
                                if (text != "" && text != null)
                                    TeamBuff = text.Split(',').ToList().Select(int.Parse).ToList();
                                TPrefix= SQLWriter.Get<string>("前缀");

                                //chatcolor = SQLWriter.Get<string>("颜色");
                                //suffix = SQLWriter.Get<string>("后缀");
                            }
                            else
                            {
                                TeamID = -1;//无此小队,使其变成-1
                            }
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.ConsoleError(TeamID + "的小队数据库读取错误:" + ex.ToString());
                    }
                }




            }
           
        }

        public void DaySS()//每日召唤加入
        {
            if (TShock.Players[Who].HasPermission(Permissions.summonboss) && !TShock.Players[Who].HasPermission("忽略召唤限制"))
            {
                int a = Sundry.MaxDaySummon(TShock.Players[Who]) - DaySummonBN;//昨天剩余的
                if (a < 0)
                    a = 0;
                SummonBN += Sundry.MaxDaySummon(TShock.Players[Who]);//加上今天的总额
                SummonBN += a;//加上昨天剩余的

                a = Sundry.DaySummonT(TShock.Players[Who]);//读最大值
                if (SummonBN > a)
                    SummonBN = a;

                SummonBN -= Sundry.MaxDaySummon(TShock.Players[Who]);//再减去今天的,因为今天的按最新权限
                if (SummonBN < 0)
                    SummonBN = 0;
            }

            if (TShock.Players[Who].HasPermission(Permissions.startinvasion) && !TShock.Players[Who].HasPermission("忽略入侵限制"))
            {
                //if (InvadeN < 0)
                //    InvadeN = 0;
                //InvadeN += Sundry.MaxDayInvade(TShock.Players[Who]);
                //int a = Sundry.DayInvadeT(TShock.Players[Who]);
                //if (InvadeN > a)
                //    InvadeN = a;


                int a = Sundry.MaxDayInvade(TShock.Players[Who]) - DayInvadeN;//昨天剩余的
                if (a < 0)
                    a = 0;
                InvadeN += Sundry.MaxDayInvade(TShock.Players[Who]);//加上今天的总额
                InvadeN += a;//加上昨天剩余的

                a = Sundry.DayInvadeT(TShock.Players[Who]);//读最大值
                if (InvadeN > a)
                    InvadeN = a;

                InvadeN -= Sundry.MaxDayInvade(TShock.Players[Who]);//再减去今天的,因为今天的按最新权限
                if (InvadeN < 0)
                    InvadeN = 0;
            }

            DaySummonBN = 0;
            DayInvadeN = 0;
            OverSummon = 0;//每天重置
        }

        public void AddExp(NPC npc,int proportion)//第二个参数是比例
        {
            if (proportion < LC.LConfig.老怪经验最低比例) return;//小于比例没有经验


            int exp = 0;

            if (!LC.LConfig.替换经验表.TryGetValue(npc.netID, out exp))
                exp = Sundry.ComputeExpByKillLife(npc.lifeMax);

            if (exp == 0) return;

            if (npc.lifeMax < Sundry.ComputeNoExpByLeve(Leve)) exp = 1;//如达不到则最小经验1

            AddExp(exp * proportion / 100,false);//计入日限制


            
        }
        

        public void AddExp(int exp,bool nodayexp =true)//第二个参数是无视日限制
        {
            if (TShock.Players[Who] == null) return;
            
            if (Leve == -1)
            {
                TShock.Players[Who].SendErrorMessage("经验读取错误，可尝试退出重进服务器！");
                return;
            }

            if (exp == 0) return;


            if (exp > 0)
            {
                if (Main.hardMode)//等级限制
                {

                if (!NPC.downedGolemBoss)//石巨人没杀
                    if (LC.LConfig.巨人最高等级 != -1)
                        if (LC.LConfig.巨人最高等级 <= Leve) return;

                if (LC.LConfig.最高等级 != -1)
                    if (LC.LConfig.最高等级 <= Leve) return;

                }
                else if (LC.LConfig.肉前最高等级 != -1)
                if (LC.LConfig.肉前最高等级 <= Leve) return;
            }

            if (!nodayexp)//如果不无视
            {
                if (Leve < LC.LConfig.不限制经验级别)
                {
                    string timetext = DateTime.Now.ToString("yyyy-MM-dd");
                    if (timetext != DayTimeText)
                    {
                        DayTimeText = timetext;
                        DayExp = 0; TeamDayExp = 0; DaySS();
                    }
                    if (exp > 0)
                    {
                        if (Main.hardMode)//经验限制
                        {

                            if (!NPC.downedGolemBoss)//石巨人没杀
                                if (LC.LConfig.巨人日最高经验 != -1)
                                    if (LC.LConfig.巨人日最高经验 <= DayExp + exp)
                                    {
                                        exp = (int)(LC.LConfig.巨人日最高经验 - DayExp);
                                    }


                            if (LC.LConfig.日最高经验 != -1)//肉后
                                if (LC.LConfig.日最高经验 <= DayExp + exp)
                                {
                                    exp = (int)(LC.LConfig.日最高经验 - DayExp);
                                }


                        }
                        else if (LC.LConfig.肉前日最高经验 != -1)//肉前
                            if (LC.LConfig.肉前日最高经验 <= DayExp + exp)
                            {
                                exp = (int)(LC.LConfig.肉前日最高经验 - DayExp);
                            }


                        if (exp <= 0) return;
                        DayExp += exp;//差不多能补正到0，同时更换（不能补正，一旦补正的话多服务器可以利用进度差距刷经验）
                    }
                    else
                    {
                        //负经验好像无需理会，因为只限制得，扣经验不扣得经验
                    }

                }

            }


            if (exp > 0)
            {
                long e = Sundry.ComputeExp(Leve + 1);//下级的最低经验
                if (e <= Exp + exp)
                {
                    List<LTask> ltask;
                    if (LC.LConfig.等级任务.TryGetValue(Leve, out ltask))//是否包含,并赋值到ltask
                    {
                        if (ltask.Count - 1 >= TaskID)
                        {
                            TShock.Players[Who].SendErrorMessage("您当前的任务因升级而被跳过!");
                        }
                    }


                    Leve++;

                    TaskID = 0;
                    TaskNumber = 0;//用于初始化等级任务

                    TShock.Utils.Broadcast( TShock.Players[Who].Name + "已升级到" + Leve + "级！！！", 255, 215, 0);
                    


                    Color c = new Color(255, 215, 0);

                    TShock.Players[Who].SendData(PacketTypes.CreateCombatTextExtended, "↑↑↑UpLeve" + Leve + "↑↑↑", (int)c.PackedValue, TShock.Players[Who].X, TShock.Players[Who].Y);


                }    

            }
            else
            { 
                long e = Sundry.ComputeExp(Leve);//当前等级的最低经验
                if (e > Exp + exp)//证明要掉级了
                {
                    exp = (int)(e - Exp);
                }
                if (exp == 0) return;
            }
            Exp = Exp + exp;
            if (TShock.Players[Who] != null)
                if ((DateTime.UtcNow - LastExpTime).TotalMilliseconds < 50) return;
                else
                {
                    LastExpTime = DateTime.UtcNow;
                    if (exp > 0)
                    {
                        Color c = new Color(255, 230, 130);
                        TShock.Players[Who].SendData(PacketTypes.CreateCombatTextExtended, "↑EXP" + "↑" + exp+ "↑", (int)c.PackedValue, TShock.Players[Who].X, TShock.Players[Who].Y);
                    }
                    else
                    {
                        Color c = new Color(150, 130, 130);
                        TShock.Players[Who].SendData(PacketTypes.CreateCombatTextExtended, "↓EXP" + "↓" + Math.Abs(exp) + "↓", (int)c.PackedValue, TShock.Players[Who].X, TShock.Players[Who].Y);
                    }
                }
            UpDataDB(false);
        }
        public void AddTeamExp(int exp, bool nodayexp = false)//第二个参数是无视日限制
        {
            if (TShock.Players[Who] == null) return;

            if (Leve == -1 || !LC.LConfig.启动小队部分 || TeamID <= 0)
            {
                return;
            }


            if (exp <= 0) return;//此值不能为负数

            if (!nodayexp)//如果不无视
            {
                string timetext = DateTime.Now.ToString("yyyy-MM-dd");
                if (timetext != DayTimeText)
                {
                    DayTimeText = timetext;
                    DayExp = 0; TeamDayExp = 0; DaySS();
                }

                if (LC.LConfig.小队日最高经验 <= TeamDayExp + exp)
                {
                    exp = (int)(LC.LConfig.小队日最高经验 - TeamDayExp);
                }

                if (exp <= 0) return;
                TeamDayExp += exp;//差不多能补正到0，同时更换（不能补正，一旦补正的话多服务器可以利用进度差距刷经验）
            }

            TeamExp += exp;

            UpDataDB(false);

        }
        public void UpDataDB(bool notime)//参数忽略时间限制
        {
            if (TShock.Players[Who] == null) return;
            if (Leve == -1)
            {
                TShock.Log.ConsoleError("饥荒RPG插件数据库取消保存"+ TShock.Players[Who].Name);
                return;
            }
            if (!notime)
            {
                if ((DateTime.UtcNow - LastDataTime).TotalMilliseconds < 5000) return;//5秒只同步一次
                if ((DateTime.UtcNow - LC.LastDataTiem).TotalMilliseconds < 3000) return;//3秒只同步一人
                LastDataTime = DateTime.UtcNow;
                LC.LastDataTiem = DateTime.UtcNow;
            }
            try
            {
                string query = "UPDATE 饥荒RPG SET 经验=@0, 等级=@1, 停状态=@2, 隐藏信息=@3, 任务=@4, 完成量=@5, 日经验=@6, 日文本=@7, 前缀=@8, 颜色=@9, 后缀=@10, 召唤=@11, 入侵=@12, 日召唤=@13, 日入侵=@14, 超召唤=@15, 小队=@16, 贡献经验=@17, 日贡经验=@18 WHERE ID=@19";
                TShock.DB.Query(query, Exp, Leve, StopBuff ? 1 : 0, HidingInfo ? 1 : 0,TaskID,TaskNumber,DayExp,DayTimeText
                    , CPrefix, CChatColor, CSuffix, SummonBN, InvadeN, DaySummonBN, DayInvadeN, OverSummon
                    ,TeamID,TeamExp ,TeamDayExp 
                    , TShock.Players[Who].Account.ID);
            }
            catch (Exception ex)
            { TShock.Log.Error("饥荒RPG插件数据库保存出错:" + ex.ToString()); }




        }
        public bool IsMaxLeve()//检查是否达到最大级别
        {
            if (Main.hardMode)//等级限制
            {

                if (!NPC.downedGolemBoss)//石巨人没杀
                    if (LC.LConfig.巨人最高等级 != -1)
                        if (LC.LConfig.巨人最高等级 <= Leve) return true;


                if (LC.LConfig.最高等级 != -1)
                    if (LC.LConfig.最高等级 <= Leve) return true;

            }
            else if (LC.LConfig.肉前最高等级 != -1)
                if (LC.LConfig.肉前最高等级 <= Leve) return true;


            return false;
        }
        public void GetChatCustom()//获取自定义聊天缀
        {
            var user =TShock.Players[Who];
            if (user == null) return ;
            if (user.Group.Name==GroupName) return;

            Prefix = "";
            ChatColor = "";
            Suffix = "";


            string prefix = "";
            string chatcolor = "";
            string suffix = "";

            try
            {
                using (var SQLWriter = TShock.DB.QueryReader("SELECT * FROM GroupList WHERE GroupName=@0", user.Group.Name))
                {
                    if (SQLWriter.Read())//如果读出来了
                    {
                        prefix = SQLWriter.Get<string>("Prefix");
                        chatcolor = SQLWriter.Get<string>("ChatColor");
                        suffix = SQLWriter.Get<string>("Suffix");
                    }
                    else//不然判断是否是超管
                    {
                        if (user.Group.Name == "superadmin")
                        {
                            prefix = TShock.Config.Settings.SuperAdminChatPrefix;
                            chatcolor = TShock.Config.Settings.SuperAdminChatRGB[0] +","+ TShock.Config.Settings.SuperAdminChatRGB[1] + "," + TShock.Config.Settings.SuperAdminChatRGB[2];
                            suffix = TShock.Config.Settings.SuperAdminChatSuffix;
                        }
                        else
                            TShock.Log.ConsoleError(user.Group.Name + "饥荒RPG前缀未知！");
                    }

                }

            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError("饥荒RPG聊天风格读取错误:" + ex.ToString());
            }

            GroupName = user.Group.Name;



            if (user.tempGroup != null || !user.HasPermission("显示自定义缀"))
            {
                Prefix = prefix;
                ChatColor = chatcolor;
                Suffix = suffix;
                return;
            }



            if (CPrefix != null && CPrefix != "")
                Prefix = CPrefix;
            else if (TPrefix != null && TPrefix != "")
                Prefix = TPrefix;
            else Prefix = prefix;



            if (CChatColor ==null || CChatColor=="")
                ChatColor = chatcolor;
            else ChatColor = CChatColor;

            if (CSuffix== null || CSuffix=="")
                Suffix = suffix;
            else Suffix = CSuffix;



            if (ChatColor  == null || ChatColor=="")
                ChatColor = "255,255,255";

            return;
        }
        public bool IsMaxExp()//检查是否达到最大经验
        {
            if (Leve < LC.LConfig.不限制经验级别)
            {

                string timetext = DateTime.Now.ToString("yyyy-MM-dd");
                if (timetext != DayTimeText)
                {
                    DayTimeText = timetext;
                    DayExp = 0; TeamDayExp = 0; DaySS();
                }

                if (Main.hardMode)//经验限制
                {

                    if (!NPC.downedGolemBoss)//石巨人没杀
                        if (LC.LConfig.巨人日最高经验 != -1)
                            if (LC.LConfig.巨人日最高经验 <= DayExp) return true;

                    if (LC.LConfig.日最高经验 != -1)//肉后
                        if (LC.LConfig.日最高经验 <= DayExp) return true;



                }
                else if (LC.LConfig.肉前日最高经验 != -1)//肉前
                    if (LC.LConfig.肉前日最高经验 <= DayExp) return true;

            }

            return false;
        }

        public string MaxExpP()//当日经验百分比文本
        {
            if (Leve < LC.LConfig.不限制经验级别)
            {

                if (Main.hardMode)//经验限制
                {

                    if (!NPC.downedGolemBoss)//石巨人没杀
                        if (LC.LConfig.巨人日最高经验 != -1)
                            if (LC.LConfig.巨人日最高经验 > DayExp) return " 余:" + (100 - DayExp * 100 / LC.LConfig.巨人日最高经验) + "%";

                    if (LC.LConfig.日最高经验 != -1)//肉后
                        if (LC.LConfig.日最高经验 > DayExp) return " 余:" + (100 - DayExp * 100 / LC.LConfig.日最高经验) + "%";

                }
                else if (LC.LConfig.肉前日最高经验 != -1)//肉前
                    if (LC.LConfig.肉前日最高经验 > DayExp) return " 余:" + (100 - DayExp*100 / LC.LConfig.肉前日最高经验) + "%";

            }

            return "";
        }


        public void ClearProj()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i] == null) continue;
                //Console.WriteLine("弹丸属于{0}", Main.projectile[i].owner);
                if (Main.projectile[i].owner == Who)
                {
                    Main.projectile[i].active = false;
                    Main.projectile[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", i);
                }

            }
        }

    }
    




    //可以更多
}

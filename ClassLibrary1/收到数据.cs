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
using System.Text.RegularExpressions;
using System.IO.Streams;
using Terraria.Localization;
using Terraria.GameContent.Tile_Entities;
using TShockAPI.Net;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ObjectData;
using TShockAPI.Localization;
using TShockAPI.Models;
using TShockAPI.Models.PlayerUpdate;
using TShockAPI.Models.Projectiles;
using Terraria.Net;
using Terraria.GameContent.NetModules;


namespace TestPlugin
{
	public delegate bool GetDataHandlerDelegate(GetDataHandlerArgs args);//设置代理
	public class GetDataHandlerArgs : EventArgs//要让这个数据在数据的基础上再多两个信息，用户和解析后的数据
	{
		public TSPlayer Player { get; private set; }
		public MemoryStream Data { get; private set; }
		public Player TPlayer { get { return Player.TPlayer; } }
		public GetDataHandlerArgs(TSPlayer player, MemoryStream data) { Player = player; Data = data; }
	}
    public static class GetDataHandlers
    {
        //static string EditHouse = "house.edit";//一些变量
        private static Dictionary<PacketTypes, GetDataHandlerDelegate> GetDataHandlerDelegates;//创建词典
        public static void InitGetDataHandler()
        {
            GetDataHandlerDelegates = new Dictionary<PacketTypes, GetDataHandlerDelegate>
            {   { PacketTypes.PlayerSpawn, OnPlayerSpawn},//玩家生成
			    {PacketTypes.PlayerDeathV2,OnPlayerDeath},//玩家死亡
                {PacketTypes.PlayerUpdate,OnPlayerUpdate},//玩家更新
                {PacketTypes.PlayerSlot,OnPlayerSlot},//玩家更新槽
                {PacketTypes.NpcTalk,OnNpcTalk},//与npc对话
                { PacketTypes.CompleteAnglerQuest, HandleCompleteAnglerQuest },//渔夫任务完成
                { PacketTypes.SpawnBossorInvasion, HandleSpawnBoss },//用物品召唤boss时
                { PacketTypes.FishOutNPC, HandleFishOutNPC },//钓鱼出NPC
                { PacketTypes.CrystalInvasionStart, HandleOldOnesArmy },//开启老将军
            };
        }


        public static bool HandlerGetData(PacketTypes type, TSPlayer player, MemoryStream data)
        {
            GetDataHandlerDelegate handler;//代理变量
            if (GetDataHandlerDelegates.TryGetValue(type, out handler))//是否包含,并赋值到handler
            {
                try { return handler(new GetDataHandlerArgs(player, data)); }
                catch (Exception ex) { TShock.Log.Error("饥荒RPG插件错误调用事件时出错:" + ex.ToString()); }
            }
            return false;
        }
        private static bool HandleOldOnesArmy(GetDataHandlerArgs args)//113启动老将军
        {
            if (args.Player.IsBouncerThrottled()) return true;

            if (LC.LConfig.启动入侵限制 && args.Player.HasPermission(Permissions.startdd2))
            {
                if (!args.Player.HasPermission("忽略入侵时间限制") && !args.Player.HasPermission("忽略入侵限制"))//若改新版,后面那个没用
                {
                    int w = (int)DateTime.Now.DayOfWeek;//获取当前周几
                    int b, e;
                    if (w == 0 || w == 6)
                    {
                        b = LC.LConfig.周末额外入侵起时间;
                        e = LC.LConfig.周末额外入侵终时间;
                    }
                    else
                    {
                        b = LC.LConfig.日入侵起时间;
                        e = LC.LConfig.日入侵终时间;
                    }

                    if (b > 0 || e > 0)
                    {
                        int h = DateTime.Now.Hour;      //获取当前时间的小时部分
                        int m = DateTime.Now.Minute;
                        int t = h * 60 + m;

                        if (t < b || t > e)//两头都包
                        {
                            if (w == 0 || w == 6)
                            {
                                int ab = LC.LConfig.日召唤起时间;//全部时间
                                int ae = LC.LConfig.日召唤终时间;//全部时间
                                if (ab > 0 || ae > 0)
                                {
                                    if (t < ab || t > ae)//两头都包
                                    {
                                        args.Player.SendErrorMessage("当前 周末" + h + ":" + m + " 无法入侵,服务器设定周末 " + (b / 60) + ":" + (b % 60) + "至" + (e / 60) + ":" + (e % 60) + "或" + (ab / 60) + ":" + (ab % 60) + "至" + (ae / 60) + ":" + (ae % 60) + " 才能入侵.");
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                args.Player.SendErrorMessage("当前 " + h + ":" + m + " 无法入侵,服务器设定每天 " + (b / 60) + ":" + (b % 60) + "至" + (e / 60) + ":" + (e % 60) + " 才能入侵.");
                                return true;
                            }
                        }
                    }
                }

                if (args.Player.HasPermission("忽略入侵限制")) return false;


                lock (LC.LPlayers)
                {
                    var lpy = LC.LPlayers[args.Player.Index];
                    if (lpy == null) return false;
                    if (lpy.DayInvadeN >= Sundry.MaxDayInvade(args.Player))
                        if (lpy.InvadeN <= 0)
                        {
                            args.Player.SendErrorMessage("你没有足够的入侵次数.");
                            return true;
                        }
                        else lpy.InvadeN--;
                    else
                        lpy.DayInvadeN++;
                    lpy.UpDataDB(false);
                    if (lpy.DayInvadeN > Sundry.MaxDayInvade(args.Player) && lpy.InvadeN <= 0) args.Player.SendInfoMessage("你的入侵次数已耗尽.");
                }
            }

            return false;
        }
        private static bool HandleFishOutNPC(GetDataHandlerArgs args)//130钓鱼出NPC
        {
            ushort tileX = args.Data.ReadUInt16();
            ushort tileY = args.Data.ReadUInt16();
            short npcType = args.Data.ReadInt16();
            
            if (LC.LConfig .启动召唤限制  && npcType == NPCID.DukeFishron && args.Player.HasPermission(Permissions.summonboss))//掉出猪鲨
            {
                int timeType = 0;//时间类型
                string timeText = "";//时间文本

                if (!args.Player.HasPermission("忽略召唤时间限制"))
                {
                    int w = (int)DateTime.Now.DayOfWeek;//获取当前周几
                    int b, e;
                    if (w == 0 || w == 6)
                    {
                        b = LC.LConfig.周末额外召唤起时间;
                        e = LC.LConfig.周末额外召唤终时间;
                    }
                    else
                    {
                        b = LC.LConfig.日召唤起时间;
                        e = LC.LConfig.日召唤终时间;
                    }

                    if (b > 0 || e > 0)
                    {
                        int h = DateTime.Now.Hour;      //获取当前时间的小时部分
                        int m = DateTime.Now.Minute;
                        int t = h * 60 + m;

                        if (t < b || t > e)//两头都包
                        {
                            if (w == 0 || w == 6)
                            {
                                int ab = LC.LConfig.日召唤起时间;//全部时间
                                int ae = LC.LConfig.日召唤终时间;//全部时间
                                if (ab > 0 || ae > 0)
                                {
                                    if (t < ab || t > ae)//两头都包
                                    {
                                        timeType = 1;
                                        timeText = "当前 周末" + h + ":" + m + " 无法召唤,服务器设定周末 " + (b / 60) + ":" + (b % 60) + "至" + (e / 60) + ":" + (e % 60) + "或" + (ab / 60) + ":" + (ab % 60) + "至" + (ae / 60) + ":" + (ae % 60) + " 才能召唤.";
                                        //移到了下面
                                    }
                                }
                            }
                            else
                            {
                                timeType = 2;
                                timeText = "当前 " + h + ":" + m + " 无法召唤,服务器设定每天 " + (b / 60) + ":" + (b % 60) + "至" + (e / 60) + ":" + (e % 60) + " 才能召唤.";
                            }
                        }
                    }
                }

                if (args.Player.HasPermission("忽略召唤限制")) return false;

                lock (LC.LPlayers)
                {
                    var lpy = LC.LPlayers[args.Player.Index];
                    if (lpy == null) return false;

                    if (timeType != 0)
                        if (lpy.OverSummon <= 0)
                            if (timeType == 1)
                            {
                                args.Player.SendErrorMessage(timeText);
                                return true;
                            }
                            else if (timeType == 2)
                            {
                                args.Player.SendErrorMessage(timeText);
                                return true;
                            }


                    if (lpy.DaySummonBN >= Sundry.MaxDaySummon(args.Player))
                        if (lpy.SummonBN <= 0)
                        {
                            args.Player.SendErrorMessage("你没有足够的召唤次数.");
                            return true;
                        }
                        else lpy.SummonBN--;
                    else
                        lpy.DaySummonBN++;

                    if (timeType != 0) lpy.OverSummon--;

                    lpy.UpDataDB(false);
                    if (lpy.DaySummonBN > Sundry.MaxDaySummon(args.Player) && lpy.SummonBN <= 0) args.Player.SendInfoMessage("你的召唤次数已耗尽.");
                }
            }

            return false;
        }
        private static readonly int[] invasions = { -1, -2, -3, -4, -5, -6, -7, -8, -10, -11 };
        private static readonly int[] bosses = { 4, 13, 50, 125, 126, 134, 127, 128, 131, 129, 130, 222, 245, 266, 370, 657 };
        private static bool HandleSpawnBoss(GetDataHandlerArgs args)//61用物品召唤boss时
        {
            if (args.Player.IsBouncerThrottled()) return true;

            var plr = args.Data.ReadInt16();
            var thingType = args.Data.ReadInt16();
            NPC npc = new NPC();
            npc.SetDefaults(thingType);

            if (plr != args.Player.Index)
                return true;


            //多体节怪特例
            if (thingType == 125 || thingType == 126)//魔眼
                if (Main.npc.Count(p => p != null && p.active && (p.netID == 125 || p.active && p.netID == 126)) > 0) return false;
            if (thingType == 128 || thingType == 131 || thingType == 129 || thingType == 130)//四肢
                if (Main.npc.Count(p => p != null && p.active && p.netID == 127) > 0) return false;


            if (LC.LConfig.启动召唤限制 && bosses.Contains(thingType) && args.Player.HasPermission(Permissions.summonboss))//需要有权限
            {
                int timeType = 0;//时间类型
                string timeText = "";//时间文本

                if (!args.Player.HasPermission("忽略召唤时间限制"))
                {
                    int w = (int)DateTime.Now.DayOfWeek;//获取当前周几
                    int b, e;
                    if (w == 0 || w == 6)
                    {
                        b = LC.LConfig.周末额外召唤起时间;
                        e = LC.LConfig.周末额外召唤终时间;
                    }
                    else
                    {
                        b = LC.LConfig.日召唤起时间;
                        e = LC.LConfig.日召唤终时间;
                    }

                    if (b > 0 || e > 0)
                    {
                        int h = DateTime.Now.Hour;      //获取当前时间的小时部分
                        int m = DateTime.Now.Minute;
                        int t = h * 60 + m;

                        if (t < b || t > e)//两头都包
                        {
                            if (w == 0 || w == 6)
                            {
                                int ab = LC.LConfig.日召唤起时间;//全部时间
                                int ae = LC.LConfig.日召唤终时间;//全部时间
                                if (ab > 0 || ae > 0)
                                {
                                    if (t < ab || t > ae)//两头都包
                                    {
                                        timeType = 1;
                                        timeText = "当前 周末" + h + ":" + m + " 无法召唤,服务器设定周末 " + (b / 60) + ":" + (b % 60) + "至" + (e / 60) + ":" + (e % 60) + "或" + (ab / 60) + ":" + (ab % 60) + "至" + (ae / 60) + ":" + (ae % 60) + " 才能召唤.";
                                        //移到了下面

                                        //args.Player.SendErrorMessage("当前 周末" + h + ":" + m + " 无法召唤,服务器设定周末 " + (b / 60) + ":" + (b % 60) + "至" + (e / 60) + ":" + (e % 60) + "或" + (ab / 60) + ":" + (ab % 60) + "至" + (ae / 60) + ":" + (ae % 60) + " 才能召唤.");
                                        //return true;
                                    }
                                }
                            }
                            else
                            {
                                timeType = 2;
                                timeText = "当前 " + h + ":" + m + " 无法召唤,服务器设定每天 " + (b / 60) + ":" + (b % 60) + "至" + (e / 60) + ":" + (e % 60) + " 才能召唤.";

                                //args.Player.SendErrorMessage("当前 " + h + ":" + m + " 无法召唤,服务器设定每天 " + (b / 60) + ":" + (b % 60) + "至" + (e / 60) + ":" + (e % 60) + " 才能召唤.");
                                //return true;
                            }
                        }
                    }
                }


                if (args.Player.HasPermission("忽略召唤限制")) return false;

                lock (LC.LPlayers)
                {
                    var lpy = LC.LPlayers[args.Player.Index];
                    if (lpy == null) return false;//究竟?

                    if (timeType != 0)
                        if (lpy.OverSummon <= 0)
                            if (timeType == 1)
                            {
                                args.Player.SendErrorMessage(timeText);
                                return true;
                            }
                            else if (timeType == 2)
                            {
                                args.Player.SendErrorMessage(timeText);
                                return true;
                            }


                    if (lpy.DaySummonBN >= Sundry.MaxDaySummon(args.Player))
                        if (lpy.SummonBN <= 0)
                        {
                            args.Player.SendErrorMessage("你没有足够的召唤次数.");
                            return true;
                        }
                        else lpy.SummonBN--;
                    else//只有当日的才加,上日的减少上日的
                        lpy.DaySummonBN++;

                    if (timeType != 0) lpy.OverSummon--;

                    lpy.UpDataDB(false);
                    if (lpy.DaySummonBN >= Sundry.MaxDaySummon(args.Player) && lpy.SummonBN <= 0) args.Player.SendInfoMessage("你的召唤次数已耗尽.");
                }
            }


            if (LC.LConfig.启动入侵限制 && invasions.Contains(thingType) && args.Player.HasPermission(Permissions.startinvasion))
            {
                if (!args.Player.HasPermission("忽略入侵时间限制") && !args.Player.HasPermission("忽略入侵限制"))//若改新版,后面那个没用
                {
                    int w = (int)DateTime.Now.DayOfWeek;//获取当前周几
                    int b, e;
                    if (w == 0 || w == 6)
                    {
                        b = LC.LConfig.周末额外入侵起时间;
                        e = LC.LConfig.周末额外入侵终时间;
                    }
                    else
                    {
                        b = LC.LConfig.日入侵起时间;
                        e = LC.LConfig.日入侵终时间;
                    }

                    if (b > 0 || e > 0)
                    {
                        int h = DateTime.Now.Hour;      //获取当前时间的小时部分
                        int m = DateTime.Now.Minute;
                        int t = h * 60 + m;

                        if (t < b || t > e)//两头都包
                        {
                            if (w == 0 || w == 6)
                            {
                                int ab = LC.LConfig.日召唤起时间;//全部时间
                                int ae = LC.LConfig.日召唤终时间;//全部时间
                                if (ab > 0 || ae > 0)
                                {
                                    if (t < ab || t > ae)//两头都包
                                    {
                                        args.Player.SendErrorMessage("当前 周末" + h + ":" + m + " 无法入侵,服务器设定周末 " + (b / 60) + ":" + (b % 60) + "至" + (e / 60) + ":" + (e % 60) + "或" + (ab / 60) + ":" + (ab % 60) + "至" + (ae / 60) + ":" + (ae % 60) + " 才能入侵.");
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                args.Player.SendErrorMessage("当前 " + h + ":" + m + " 无法入侵,服务器设定每天 " + (b / 60) + ":" + (b % 60) + "至" + (e / 60) + ":" + (e % 60) + " 才能入侵.");
                                return true;
                            }

                        }
                    }
                }


                if (args.Player.HasPermission("忽略入侵限制")) return false;

                lock (LC.LPlayers)
                {
                    var lpy = LC.LPlayers[args.Player.Index];
                    if (lpy == null) return false;

                    if (lpy.DayInvadeN >= Sundry.MaxDayInvade(args.Player))
                        if (lpy.InvadeN <= 0)
                        {
                            args.Player.SendErrorMessage("你没有足够的入侵次数.");
                            return true;
                        }
                        else lpy.InvadeN--;
                    else
                        lpy.DayInvadeN++;
                    lpy.UpDataDB(false);
                    if (lpy.DayInvadeN > Sundry.MaxDayInvade(args.Player) && lpy.InvadeN <= 0) args.Player.SendInfoMessage("你的入侵次数已耗尽.");
                }
            }

            return false;
        }
        private static bool HandleCompleteAnglerQuest(GetDataHandlerArgs args)//75渔夫任务完成
        {
            if(!LC.LConfig.启动渔夫任务经验) return false;

            lock (LC.LPlayers)
            {
                var lpy = LC.LPlayers[args.Player.Index];
                if (lpy == null) return false;

                lpy.AddExp(lpy.Leve);

            }

            return false;
        }

        private static bool OnNpcTalk(GetDataHandlerArgs args)//40与npc交谈
        {
            args.Data.ReadByte();
            var npcid = args.Data.ReadInt16();
            if (npcid<0) return false;
            NPC npc =Main.npc[npcid];
            if (npc == null) return false;
            //Console.WriteLine(args.Player.Name+"与"+ npc .netID+ "交谈");


            lock (LC.LPlayers)
            {
                var lpy = LC.LPlayers[args.Player.Index];
                if (lpy == null) return false;

                if (!lpy.IsMaxLeve())
                {
                    List<LTask> ltask;
                    if (LC.LConfig.等级任务.TryGetValue(lpy.Leve, out ltask))//是否包含,并赋值到ltask
                    {
                        if (ltask.Count - 1 >= lpy.TaskID)
                        {
                            if (ltask[lpy.TaskID].类型 == 5 && npc.netID == ltask[lpy.TaskID].目标ID)
                            {

                                if (lpy.TaskNumber < ltask[lpy.TaskID].目标数量)
                                    lpy.TaskNumber++;

                                //此处可以判定自动完成,但没必要
                            }

                        }

                    }

                }
                    
            }

            return false;
        }
        private static bool OnPlayerSpawn(GetDataHandlerArgs args)//12玩家生成
		{
            args.Data.ReadByte();//跳过玩家号
            short spawnx = args.Data.ReadInt16();
            short spawny = args.Data.ReadInt16();
            int respawnTimer = args.Data.ReadInt32();
            args.Data.ReadInt16(); //numberOfDeathsPVE 
            args.Data.ReadInt16(); //numberOfDeathsPVP
            PlayerSpawnContext context = (PlayerSpawnContext)args.Data.ReadByte();
            //Console.WriteLine("苏生位置X{0}Y{1}类型{2}时间{3} ", spawnx, spawny, context, respawnTimer);
            if (context == PlayerSpawnContext.ReviveFromDeath && respawnTimer <= 0)//若玩家复活
            {
                if (LC.LConfig.启动饥荒 && LC.LConfig.复活饱食>0)
                    args.Player.SetBuff(26, 3600 * LC.LConfig.复活饱食);
            }
            else if (context == PlayerSpawnContext.SpawningIntoWorld)
            {
                //if (LC.LConfig.启动饥荒)
                //    user.SetBuff(26, 3600 * LC.LConfig.进入饱食);//暂时无法记录玩家的buff，因此只能为上线时获得5分钟buff


                long exp = 0;
                int leve = -1;
                List<int> buff = new List<int>();
                bool stopbuff = false;
                bool hidinginfo = false;
                int taskid = 0;
                int tasknumber = 0;
                long dayexp = 0;
                string daytimetext = "";

                string prefix = "";
                string chatcolor = "";
                string suffix = "";

                int summonBN = 0;
                int invadeN = 0;

                int Ileve = 0;//初始等级
                long Iexp = 0;//初始经验

                int ds = 0;
                int di = 0;
                int os = 0;



                int team = -1;
                long texp = 0;//初始经验
                long tdayexp = 0;


                if (Main.hardMode)
                {
                    Iexp=LC.LConfig.肉后初始经验;
                    Ileve= LC.LConfig.肉后初始等级;
                }
                else
                {
                    Iexp = LC.LConfig.肉前初始经验;
                    Ileve = LC.LConfig.肉前初始等级;
                }
                if (Iexp < 0)
                    Iexp = 0;
                if (Ileve < 0)
                    Ileve = 0;

                try
                {
                    bool news = false;




                    using (var SQLWriter = TShock.DB.QueryReader("SELECT * FROM 饥荒RPG WHERE ID=@0", args.Player.Account.ID))
                    {
                        if (SQLWriter.Read())//如果读出来了，
                        {
                            exp = SQLWriter.Get<long>("经验");
                            leve = SQLWriter.Get<int>("等级");
                            string text = SQLWriter.Get<string>("状态");
                            if (text != "")
                                buff = text.Split(',').ToList().Select(int.Parse).ToList();
                            stopbuff= SQLWriter.Get<int>("停状态") == 1;
                            hidinginfo = SQLWriter.Get<int>("隐藏信息") == 1;
                            taskid= SQLWriter.Get<int>("任务");
                            tasknumber = SQLWriter.Get<int>("完成量");
                            dayexp= SQLWriter.Get<long>("日经验");
                            daytimetext = SQLWriter.Get<string>("日文本");

                            prefix = SQLWriter.Get<string>("前缀");
                            chatcolor = SQLWriter.Get<string>("颜色");
                            suffix = SQLWriter.Get<string>("后缀");

                            summonBN= SQLWriter.Get<int>("召唤");
                            invadeN= SQLWriter.Get<int>("入侵");


                            ds = SQLWriter.Get<int>("日召唤");
                            di = SQLWriter.Get<int>("日入侵");

                            os = SQLWriter.Get<int>("超召唤");

                            team = SQLWriter.Get<int>("小队");
                            texp = SQLWriter.Get<long>("贡献经验");
                            tdayexp = SQLWriter.Get<long>("日贡经验");


                            if (Main.hardMode && LC.LConfig.肉后经验补给)//肉后补充调到最低等级
                            {
                                if (exp < Iexp)
                                    exp = Iexp;

                                if (leve < Ileve)
                                    leve = Ileve;
                            }

                        }
                        else
                        {
                            news = true;
                        }
                    }

                    if (news)
                    {
                        int q = TShock.DB.Query("INSERT INTO 饥荒RPG (ID, 经验, 等级, 状态, 停状态, 隐藏信息, 任务, 完成量, 日经验, 日文本, 前缀, 颜色, 后缀, 召唤, 入侵, 日召唤, 日入侵, 超召唤, 小队, 贡献经验, 日贡经验) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14, @15, @16, @17, @18, @19, @20);",
                                   args.Player.Account.ID, Iexp, Ileve, "", 0, 0, 0, 0, 0, DateTime.Now.ToString("yyyy-MM-dd"), "", "", "", 0, 0, 0, 0, 0, -1, 0, 0);
                        if (q == 0) Console.WriteLine(args.Player.Name + "饥荒RPG数据库添加失败！");
                        else
                        {
                            leve = Ileve;//初始化成功定义为0
                            exp = Iexp;
                        }
                    }
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError(args.Player.Name + "的饥荒RPG数据库读取错误:" + ex.ToString());
                }



                lock (LC.LPlayers)
                    LC.LPlayers[args.Player.Index] = new LPlayer(args.Player.Index, leve, exp, buff, stopbuff, hidinginfo,taskid,tasknumber,dayexp,daytimetext,prefix,chatcolor,suffix, summonBN, invadeN,ds,di,os,team,texp,tdayexp);


                //if (LC.LPlayers[user.Index] != null) LC.LPlayers[user.Index].InitSpawn =true;

                //try
                //{

                //    user.SetBuff(26, 3600 * 5);

                //    using (var SQLWriter = TShock.DB.QueryReader("SELECT * FROM 饥荒模式 WHERE ID=@0", user.Account.ID))
                //    {
                //        if (SQLWriter.Read())//如果读出来了，
                //        {
                //            int b26 = SQLWriter.Get<int>("b26");
                //            int b206 = SQLWriter.Get<int>("b206");
                //            int b207 = SQLWriter.Get<int>("b207");
                //            if (b26 > 0) user.SetBuff(26, b26);
                //            else if (b206 > 0) user.SetBuff(206, b206);
                //            else if (b207 > 0) user.SetBuff(207, b207);
                //        }
                //        else user.SetBuff(26, 3600*15);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    TShock.Log.ConsoleError("饥荒模式数据库读取错误:"+ex.ToString());
                //}

                //Console.WriteLine("玩家产生服务器");


            }


            return false;//返回假代表没有处理，为真表示Handled=true

		}
		private static bool OnPlayerDeath(GetDataHandlerArgs args)//118玩家死亡
		{
            var id = args.Data .ReadByte();
            PlayerDeathReason playerDeathReason = PlayerDeathReason.FromReader(new BinaryReader(args.Data));
            var dmg = args.Data.ReadInt16();
            var direction = (byte)(args.Data.ReadByte() - 1);
            BitsByte bits = (BitsByte)args.Data.ReadByte();
            bool pvp = bits[0];
            if (pvp)
            {
                var winner = TShock.Players[playerDeathReason._sourcePlayerIndex];
                if (winner != null)
                {
                    if (winner.Name == args.Player.Name) return false;//取消自杀

                    lock (LC.LPlayers)
                    {
                        var wlpy = LC.LPlayers[winner.Index];
                        var dlpy = LC.LPlayers[args.Player.Index];
                        if (wlpy == null || dlpy == null) return false;//排除错误


                        if (wlpy.Leve <= LC.LConfig.PVP保护等级) return false;
                        if (dlpy.Leve <= LC.LConfig.PVP保护等级) return false;


                        int i = Math.Abs(wlpy.Leve - dlpy.Leve);
                        if (i > LC.LConfig.PVP保护级差) return false;

                        int wE = LC.LConfig.PVP胜利经验;
                        int dE = LC.LConfig.PVP失败经验;

                        if (Main.bloodMoon)//血月
                        {
                            wE *= LC.LConfig.血月PVP倍率;
                            dE *= LC.LConfig.血月PVP倍率;
                        }
                        if (wlpy != null)
                        {
                            wlpy.AddExp(wE);
                        }
                        if (dlpy != null)
                        {
                            dlpy.AddExp(dE);
                        }

                    }

                    //Console.WriteLine(_用户.Name + "被" + winner.Name + "pvp杀了！ ");
                }
            }
            else
            {
                lock (LC.LPlayers)
                {
                    var lpy = LC.LPlayers[args.Player.Index];
                    if (lpy == null) return false;

                    if (!lpy.IsMaxLeve())
                    {
                        List<LTask> ltask;
                        if (LC.LConfig.等级任务.TryGetValue(lpy.Leve, out ltask))//是否包含,并赋值到ltask
                        {
                            if (ltask.Count - 1 >= lpy.TaskID)
                            {
                                if (ltask[lpy.TaskID].类型 == 6)
                                {

                                    if (lpy.TaskNumber < ltask[lpy.TaskID].目标数量)
                                        lpy.TaskNumber++;

                                    //此处可以判定自动完成,但没必要
                                }
                            }

                        }

                    }

                }

            }


            return false;//返回假代表没有处理，为真表示Handled=true

        }
        private static bool OnPlayerUpdate(GetDataHandlerArgs args)//13玩家更新
        {
            byte playerID = args.Data.ReadInt8();
            ControlSet controls = new ControlSet((BitsByte)args.Data.ReadByte());
            MiscDataSet1 miscData1 = new MiscDataSet1((BitsByte)args.Data.ReadByte());
            MiscDataSet2 miscData2 = new MiscDataSet2((BitsByte)args.Data.ReadByte());
            MiscDataSet3 miscData3 = new MiscDataSet3((BitsByte)args.Data.ReadByte());
            byte selectedItem = args.Data.ReadInt8();
            Vector2 position = args.Data.ReadVector2();

            //Console.WriteLine("持物类型" + args.Player.TPlayer.inventory[selectedItem].type);
            //Console.WriteLine("前缀类型" + args.Player.TPlayer.inventory[selectedItem].prefix);

            var item = new Item();
            item.netDefaults(args.Player.TPlayer.inventory[selectedItem].type);
            if (LC.LConfig.不受前缀影响)//均不受影响
            {
                item.Prefix(0);
            }
            else
            {
                if (item.headSlot > -1 || item.bodySlot > -1 || item.legSlot > -1)//盔甲不受影响
                    item.Prefix(0);
                else if (item.accessory || item.vanity)//饰品盔甲不受影响
                    item.Prefix(0);
                else item.Prefix(args.Player.TPlayer.inventory[selectedItem].prefix);
            }

            if (item == null) return false;
            if (item.netID == 0) return false;
            

            lock (LC.LPlayers)
            {
                var lpy = LC.LPlayers[args.Player.Index];
                if (lpy == null) return false;
                bool newitem = false;

                if (lpy.SelectedItem != selectedItem && selectedItem != 58)
                {
                    //Console.WriteLine("切换到了新物品");
                    lpy.SelectedItem = selectedItem;
                    newitem = true;
                }
                int rare = 0;
                if (!LC.LConfig.替换品级表.TryGetValue(item.netID, out rare))
                    rare = item.rare;
                rare *= LC.LConfig.装备品质系数;

                int leve = 0;
                if (LC.LConfig.启动公共等级)
                {
                    if (LC.ServeLeve > lpy.Leve)
                        leve = LC.ServeLeve;
                    else leve = lpy.Leve;
                }
                else leve = lpy.Leve;
                

                if (rare > leve && !args.Player.HasPermission("忽略使用等级"))
                {
                    var prefixname = TShock.Utils.GetPrefixById(item.prefix);
                    if (prefixname != "") prefixname += " ";

                    if (controls.IsUsingItem)
                    {
                        if (LC.LConfig.记录跨级使用) TShock.Log.Info("{0} 越级使用 {1}",  args.Player.Name, prefixname + item.Name);

                        bool disable = false;

                        if (item.fishingPole > 0)
                        {
                            args.Player.SendErrorMessage("您当前适用" + leve + "级,无法使用" + rare + "级的鱼竿:" + item.Name + "");
                            disable = true;
                        }
                        else if (item.hammer > 0 || item.axe > 0 || item.pick > 0)
                        {
                            args.Player.SendErrorMessage("您当前适用" + leve + "级,无法使用" + rare + "级的工具:" + prefixname+item.Name + "");
                            disable = true;
                        }
                        else if (item.sentry || item.summon || item.ranged || item.magic || item.melee)
                        {
                            args.Player.SendErrorMessage("您当前适用" + leve + "级,无法使用" + rare + "级的武器:" + prefixname+item.Name + "");
                            disable = true;
                        }
                        if (disable)
                        {
                            if (LC.LConfig.提醒跨级使用)
                                foreach (var pl in TShock.Players)
                                {
                                    if (pl == null) continue;
                                    if (args.Player.Name != pl.Name)
                                    {
                                        pl.SendInfoMessage("玩家 {0} 试图越级使用 {1}", args.Player.Name, prefixname + item.Name);

                                    }
                                }


                            args.Player.Disable("越级使用物品");
                            lpy.DisableItem = 0;
                            return true;//返回真取消处理

                        }


                    }
                    else if (newitem && !args.Player.HasPermission("忽略使用等级"))
                    {
                        
                        //args.Player.SendErrorMessage("您切换到了一个" + item.rare + "级的物品" + item.Name + ",您当前只有" + lpy.Leve + "级,请不要使用它！");

                        if (item.fishingPole > 0)
                            args.Player.SendErrorMessage("您切换到了一个" + rare + "级的鱼竿[" +  item.Name + ",]您当前适用" + leve + "级,请不要使用它！");
                        else if (item.hammer > 0 || item.axe > 0 || item.pick > 0)
                            args.Player.SendErrorMessage("您切换到了一个" + rare + "级的工具[" + prefixname + item.Name + "]您当前适用" + leve + "级,请不要使用它！");
                        else if (item.sentry || item.summon || item.ranged || item.magic || item.melee)
                            args.Player.SendErrorMessage("您切换到了一个" + rare + "级的武器[" + prefixname + item.Name + "]您当前适用" + leve + "级,请不要使用它！");

                    }
                }

                if (controls.IsUsingItem)
                {
                    if (!lpy.IsMaxLeve())
                    {
                        List<LTask> ltask;
                        if (LC.LConfig.等级任务.TryGetValue(lpy.Leve, out ltask))//是否包含,并赋值到ltask
                        {
                            if (ltask.Count - 1 >= lpy.TaskID)
                            {
                                if (ltask[lpy.TaskID].类型 == 2 && item.netID == ltask[lpy.TaskID].目标ID)
                                {

                                    if (lpy.TaskNumber < ltask[lpy.TaskID].目标数量)
                                        lpy.TaskNumber++;
                                    //此处可以判定自动完成,但没必要
                                }

                            }

                        }

                    }
                       

                }

            }


            return false;//返回假代表没有处理，为真表示Handled=true

        }
        private static bool OnPlayerSlot(GetDataHandlerArgs args)//5玩家更新槽
        {
            byte plr = args.Data.ReadInt8();
            short slot = args.Data.ReadInt16();
            short stack = args.Data.ReadInt16();
            byte prefix = args.Data.ReadInt8();
            short type = args.Data.ReadInt16();

            var item = new Item();
            item.netDefaults(type);

            if (LC.LConfig.不受前缀影响)//均不受影响
            {
                item.Prefix(0);
            }
            else
            {
                if (item.headSlot > -1 || item.bodySlot > -1 || item.legSlot > -1)//盔甲不受影响
                    item.Prefix(0);
                else if (item.accessory || item.vanity)//饰品盔甲不受影响
                    item.Prefix(0);
                else item.Prefix(prefix);
            }
            



            if (item == null) return false;
            if (item.netID == 0) return false;


            if (slot == 58)
            {
                //Console.WriteLine("等级:" + item.rare);

                //Console.WriteLine("渔力:" + item.fishingPole);//大于0则为鱼竿
                ////Console.WriteLine("盔甲:" + item.wornArmor);
                //Console.WriteLine("头部:" + item.headSlot);
                //Console.WriteLine("身体:" + item.bodySlot);//大于-1则为盔甲
                //Console.WriteLine("腿部:" + item.legSlot);
                ////Console.WriteLine("染料:" + item.dye);
                //Console.WriteLine("饰品:" + item.accessory);
                //Console.WriteLine("哨兵:" + item.sentry);
                //Console.WriteLine("召唤:" + item.summon);
                //Console.WriteLine("远程:" + item.ranged);
                //Console.WriteLine("魔法:" + item.magic);
                //Console.WriteLine("近战:" + item.melee);
                //Console.WriteLine("虚荣:" + item.vanity);
                ////Console.WriteLine("方式:" + item.useStyle);
                ////Console.WriteLine("字长:" + item.Name.Length);
                ////Console.WriteLine("字符:" + item.Name.Substring(item.Name.Length - 2, 2));
                //Console.WriteLine("锤力:" + item.hammer);
                //Console.WriteLine("斧力:" + item.axe);
                //Console.WriteLine("镐力:" + item.pick);
                //Console.WriteLine("字符:" + item.Name.Substring(item.Name.Length - 3, 3));
                // Dye
                //item.wingSlot
                //item.fishingPole//渔力
                //    item.legSlot//腿槽
                //item.vanity虚荣？
                //    item.material//材料
                //    item.hammer//锤力？
                //    item.axe//斧子力量
                //    item.bodySlot//身体槽
                //    item.consumable//消耗品
                //    item.accessory//附件配饰
                //    item.sentry//哨兵
                //    item.summon//召唤
                //    item.ranged//远程
                //    item.magic//魔法
                //    item.melee//近战
                //    item.wornArmor//盔甲
                //    item.canBePlacedInVanityRegardlessOfConditions



                lock (LC.LPlayers)
                {
                    //if (item.rare > lpy.Leve)
                    //    args.Player.SendErrorMessage("您手持了一个" + item.rare + "级的物品" + item.Name + ",您当前只有" + lpy.Leve + "级,请不要使用它！");

                    var lpy = LC.LPlayers[args.Player.Index];
                    if (lpy == null) return false;
                    int rare = 0;
                    if (!LC.LConfig.替换品级表.TryGetValue(item.netID, out rare))
                        rare = item.rare;
                    rare *= LC.LConfig.装备品质系数;

                    int leve = 0;
                    if (LC.LConfig.启动公共等级)
                    {
                        if (LC.ServeLeve > lpy.Leve)
                            leve = LC.ServeLeve;
                        else leve = lpy.Leve;
                    }
                    else leve = lpy.Leve;

                    var prefixname = TShock.Utils.GetPrefixById(item.prefix);
                    if (prefixname != "") prefixname += " ";
                    if (rare > lpy.Leve)//此乃不受公共等级影响的道具
                    {
                        if (!args.Player.HasPermission("忽略染料等级") && item.Name.Length >= 3)
                        {
                            if (item.Name.Substring(item.Name.Length - 2, 2) == "染料" || item.Name.Substring(item.Name.Length - 3, 3) == "Dye")
                                args.Player.SendErrorMessage("您手持了一个" + rare + "级的染料[" + item.Name + "]您当前只有" + lpy.Leve + "级,请不要装备它！");
                        }
                        else if((item.headSlot > -1 || item.bodySlot > -1 || item.legSlot > -1) && !args.Player.HasPermission("忽略时装等级"))
                            args.Player.SendErrorMessage("您手持了一个" + rare + "级的时装[" + item.Name + "]您当前只有" + lpy.Leve + "级,请不要放入时装栏！");
                        else if ((item.accessory || item.vanity) && !args.Player.HasPermission("忽略装备等级"))
                            args.Player.SendErrorMessage("您手持了一个" + rare + "级的饰品[" + item.Name + "]您当前只有" + lpy.Leve + "级,请不要装备它！");
                        else if ((item.accessory || item.vanity) && !args.Player.HasPermission("忽略时装等级"))
                            args.Player.SendErrorMessage("您手持了一个" + rare + "级的时装[" + item.Name + "]您当前只有" + lpy.Leve + "级,请不要放入时装栏！");
                    }
                    if (rare > leve)
                    {
                        if (item.fishingPole>0 && !args.Player.HasPermission("忽略使用等级"))
                            args.Player.SendErrorMessage("您手持了一个" + rare + "级的鱼竿[" + item.Name + "]您当前适用" + leve + "级,请不要使用它！");
                        else if ((item.headSlot > -1 || item.bodySlot > -1 || item.legSlot > -1) && !args.Player.HasPermission("忽略盔甲等级"))
                            args.Player.SendErrorMessage("您手持了一个" + rare + "级的盔甲[" + item.Name + "]您当前适用" + leve + "级,请不要装备它！");
                        else if ((item.hammer>0 || item.axe>0 || item.pick>0) && !args.Player.HasPermission("忽略使用等级"))
                            args.Player.SendErrorMessage("您手持了一个" + rare + "级的工具[" + prefixname + item.Name + "]您当前适用" + leve + "级,请不要使用它！");
                        else if ((item.sentry || item.summon || item.ranged || item.magic|| item.melee) && !args.Player.HasPermission("忽略使用等级"))
                            args.Player.SendErrorMessage("您手持了一个" + rare + "级的武器[" + prefixname + item.Name + "]您当前适用" + leve + "级,请不要使用它！");
                    }

                }
            }

            return false;
        }


    }
}

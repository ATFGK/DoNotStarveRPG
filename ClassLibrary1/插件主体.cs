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
    [ApiVersion(2, 1)]//api版本
    public class TestPlugin : TerrariaPlugin
    {
        public override string Name => "饥荒RPG";
        public override string Author => "GK 阁下";
        public override string Description => "饥饿+等级！";
        public override Version Version => new Version(1, 0, 0, 8);
        public TestPlugin(Main game) : base(game) { Order = 5; LC.RI(); }//定义一个优先级
        static readonly System.Timers.Timer Update = new System.Timers.Timer(1000);//创建一个1秒的时钟
        public static bool ULock = false;//时钟的单行锁



        public override void Initialize()// 插件启动时，用于初始化各种狗子
        {

            ServerApi.Hooks.NpcSpawn.Register(this, NpcSpawn);//钩住NPC产生
            ServerApi.Hooks.NpcKilled.Register(this, NpcKilled);//钩住NPC死掉
            ServerApi.Hooks.NpcStrike.Register(this, NpcStrike);//钩住NPC伤害
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);//钩住游戏初始化时
            ServerApi.Hooks.GamePostInitialize.Register(this, PostInitialize);//地图读入后（服务器完全开启）
            ServerApi.Hooks.NetGetData.Register(this, GetData);//钩住收到数据
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);//玩家退出服务器
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);//玩家进入服务器
            ServerApi.Hooks.ServerChat.Register(this, OnChat);//钩住聊天消息

        }
        protected override void Dispose(bool disposing)// 插件关闭时，关闭各种钩子
        {
            if (disposing)
            {
                LC.SaveSE();//保存服务器经验

                Update.Elapsed -= OnUpdate; Update.Stop();//销毁时钟

                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);//销毁游戏初始化狗子
                ServerApi.Hooks.GamePostInitialize.Deregister(this, PostInitialize);//销毁载入地图后钩子
                ServerApi.Hooks.NetGetData.Deregister(this, GetData);//销毁收到数据狗子
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);//销毁玩家退出服务器
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);//销毁玩家进入服务器
                ServerApi.Hooks.ServerChat.Deregister(this, OnChat);//销毁聊天狗子
                ServerApi.Hooks.NpcSpawn.Deregister(this, NpcSpawn);//销毁NPC产生
                ServerApi.Hooks.NpcKilled.Deregister(this, NpcKilled);//销毁NPC死亡
                ServerApi.Hooks.NpcStrike.Deregister(this, NpcStrike);//销毁NPC受伤
            }
            base.Dispose(disposing);
        }
        private void OnInitialize(EventArgs args)//游戏初始化的狗子
        {
            LC.RC(); LC.RD(); LC.RSE();
            GetDataHandlers.InitGetDataHandler();//初始受到数据化代理
                                                 //Adding a command is as simple as adding a new ``Command`` object to the ``ChatCommands`` list.
                                                 //The ``Commands` object is available after including TShock in the file (`using TShockAPI;`)
                                                 //第一个是权限，第二个是子程序，第三个是名字

            Commands.ChatCommands.Add(new Command("", OnStopBuffCmd, "切换状态") { HelpText = "输入/切换状态 可以停止/启动更新等级状态" });
            Commands.ChatCommands.Add(new Command("", OnHidingInfoCmd, "切换显示") { HelpText = "输入/切换显示 可以隐藏/显示实时信息显示" });
            Commands.ChatCommands.Add(new Command("", OnFulfill, "完成任务") { HelpText = "输入/完成任务 可以完成当前任务" });
            Commands.ChatCommands.Add(new Command("", OnQueryLeve, "查等级") { HelpText = "输入/查等级 物品名或ID 查询对于的物品的等级" });
            Commands.ChatCommands.Add(new Command("重读饥荒权限", CMD, "重读饥荒配置") { HelpText = "输入/重读饥荒配置 重读饥荒配置" });
            Commands.ChatCommands.Add(new Command("", LeveInformation, "等级信息") { HelpText = "输入/等级信息 可以查看当前等级信息" });
            Commands.ChatCommands.Add(new Command("", OnIgnore, "跳过任务") { HelpText = "输入/跳过任务 可以忽略当前任务" });

        }
        private void OnIgnore(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn || args.Player.Account == null || args.Player.Account.ID == 0)
            { args.Player.SendErrorMessage("你必须登录才能使用此指令。"); return; }
            if (LC.LConfig.不允许跳过任务)
            { args.Player.SendErrorMessage("无法使用,因为此指令被服务器关闭了。"); return; }

            lock (LC.LPlayers)
            {
                var lpy = LC.LPlayers[args.Player.Index];
                if (lpy == null) return;

                if (Main.hardMode)//等级限制
                {

                    if (!NPC.downedGolemBoss)//石巨人没杀
                        if (LC.LConfig.巨人最高等级 != -1)
                            if (LC.LConfig.巨人最高等级 <= lpy.Leve)
                            { args.Player.SendErrorMessage("您已达到巨人最高级别，无法跳过任务！"); return; }


                    if (LC.LConfig.最高等级 != -1)
                        if (LC.LConfig.最高等级 <= lpy.Leve)
                        { args.Player.SendErrorMessage("您已达到最高级别，无法跳过任务！"); return; }

                }
                else if (LC.LConfig.肉前最高等级 != -1)
                {
                    if (LC.LConfig.肉前最高等级 <= lpy.Leve)
                    { args.Player.SendErrorMessage("您已达到肉前最高级别，无法跳过任务！"); return; }
                }


                List<LTask> ltask;
                if (LC.LConfig.等级任务.TryGetValue(lpy.Leve, out ltask))//是否包含,并赋值到ltask
                {
                    if (ltask.Count - 1 >= lpy.TaskID)
                    {
                        //if (ltask[lpy.TaskID].剧情 != "") args.Player.SendMessage(ltask[lpy.TaskID].剧情.Replace("{name}", args.Player.Name), new Color(255, 255, 255));
                        args.Player.SendSuccessMessage("成功跳过任务：{0}", ltask[lpy.TaskID].标题);
                        lpy.TaskID++;
                        lpy.TaskNumber = 0;
                        return;
                    }

                }

                args.Player.SendErrorMessage("你当前没用任务可跳过。");

            }

        }


        private void LeveInformation(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn || args.Player.Account == null || args.Player.Account.ID == 0)
            { args.Player.SendErrorMessage("你必须登录才能使用此指令。"); return; }
            string text = "";
            lock (LC.LPlayers)
                if (LC.LPlayers[args.Player.Index] == null) args.Player.SendErrorMessage("你必须登录才能使用此指令。");
                else
                {

                    if (!LC.LPlayers[args.Player.Index].IsMaxLeve())
                    {
                        List<LTask> ltask;

                        if (LC.LConfig.等级任务.TryGetValue(LC.LPlayers[args.Player.Index].Leve, out ltask))//是否包含,并赋值到ltask
                        {
                            if (ltask.Count - 1 >= LC.LPlayers[args.Player.Index].TaskID)
                            {
                                text += "当前任务:" + ltask[LC.LPlayers[args.Player.Index].TaskID].标题;

                                if (ltask[LC.LPlayers[args.Player.Index].TaskID].类型 == 1 || ltask[LC.LPlayers[args.Player.Index].TaskID].类型 == 2 || ltask[LC.LPlayers[args.Player.Index].TaskID].类型 == 5 || ltask[LC.LPlayers[args.Player.Index].TaskID].类型 == 6)
                                {
                                    text += " (" + LC.LPlayers[args.Player.Index].TaskNumber + "/" + ltask[LC.LPlayers[args.Player.Index].TaskID].目标数量 + ")";
                                }

                                text += "\n";

                            }

                        }
                    }


                    text += "您当前等级:" + LC.LPlayers[args.Player.Index].Leve + " 经验:" + LC.LPlayers[args.Player.Index].Exp + " 比例:" + Sundry.ComputeExpP(LC.LPlayers[args.Player.Index].Leve, LC.LPlayers[args.Player.Index].Exp) + "%";
                    text += "\n";
                    if (!LC.LPlayers[args.Player.Index].IsMaxLeve())
                    {
                        if (LC.LPlayers[args.Player.Index].IsMaxExp()) text += "您今天杀怪经验奖励已满，无法通过杀怪获取经验";
                        else text += "您最低击杀 " + Sundry.ComputeNoExpByLeve(LC.LPlayers[args.Player.Index].Leve) + " 血量的怪才能获得完整经验，今日剩" + LC.LPlayers[args.Player.Index].MaxExpP() + " 杀怪经验余量";
                    }
                    else text += "您现阶段已达到最大等级，无法获得经验";

                    if (LC.LConfig.启动公共等级)
                    {
                        text += "\n";
                        text += "当前公共等级:" + LC.ServeLeve + " 经验:" + LC.ServeExp + " 比例:" + Sundry.ComputeExpP(LC.ServeLeve, LC.ServeExp) + "%";
                        text += "\n";

                        if (!LC.SELM())
                        {
                            if (LC.SEDM()) text += "今天公共经验已满，无法继续累积公共经验";
                            //else text += "您最低击杀 " + Sundry.ComputeNoExpByLeve(LC.ServeLeve) + " 血量的怪即可累积公共经验，今日剩" + LC.SEDMP() + "杀怪经验余量";
                            else text += "击杀怪物即可额外累积公共经验，今日剩" + LC.SEDMP() + " 经验余量";
                        }
                        else text += "现阶段公共等级已达到最大等级";

                    }

                    if (LC.LConfig.启动小队部分 && LC.LPlayers[args.Player.Index].TeamID > 0)
                    {
                        text += "\n";
                        text += "所在小队:" + LC.LPlayers[args.Player.Index].TeamName + " 适用小队等级:" + LC.LPlayers[args.Player.Index].TeamLeve + " 贡献经验:" + LC.LPlayers[args.Player.Index].TeamExp;

                        if (LC.LPlayers[args.Player.Index].TeamDayExp < LC.LConfig.小队日最高经验)
                        {
                            text += "\n";
                            text += "击杀怪物即可额外累积小队经验，今日剩" + (100 - LC.LPlayers[args.Player.Index].TeamDayExp * 100 / LC.LConfig.小队日最高经验) + "% 小队经验余量";
                        }
                        else text += "\n今天贡献经验已满，无法继续累积贡献经验";
                    }

                    text += "\n";
                    text += "实时坐标:" + args.Player.TileY + "," + args.Player.TileY + "";
                    if (LC.LConfig.启动召唤限制 && args.Player.HasPermission(Permissions.summonboss) && !args.Player.HasPermission("忽略召唤限制"))
                        text += ",剩余老怪召唤次数:" + (LC.LPlayers[args.Player.Index].SummonBN + Sundry.MaxDaySummon(args.Player) - LC.LPlayers[args.Player.Index].DaySummonBN);
                    if (LC.LConfig.启动入侵限制 && args.Player.HasPermission(Permissions.startinvasion) && !args.Player.HasPermission("忽略入侵限制"))
                        text += ",剩余入侵召唤次数:" + (LC.LPlayers[args.Player.Index].InvadeN + Sundry.MaxDayInvade(args.Player) - LC.LPlayers[args.Player.Index].DayInvadeN);
                    if (LC.LConfig.启动召唤限制 && args.Player.HasPermission(Permissions.summonboss) && !args.Player.HasPermission("忽略召唤时间限制") && !args.Player.HasPermission("忽略召唤限制") && LC.LPlayers[args.Player.Index].OverSummon > 0)
                        text += ",任意时间召唤老怪机会:" + LC.LPlayers[args.Player.Index].OverSummon;
                }

            args.Player.SendSuccessMessage(text);

        }
        private void CMD(CommandArgs args)
        {
            LC.RC();
            args.Player.SendSuccessMessage("重读饥荒配置完毕!");
        }
        private void OnQueryLeve(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("格式错误,正确格式: /查等级 <物品名或ID>");
                return;
            }
            string itemName = string.Join(" ", args.Parameters);

            Item item;
            List<Item> matchedItems = TShock.Utils.GetItemByIdOrName(itemName);


            if (matchedItems.Count == 0)
            {
                args.Player.SendErrorMessage("指定物品不存在!");
                return;
            }
            else if (matchedItems.Count > 1)
            {
                args.Player.SendMultipleMatchError(matchedItems.Select(i => $"[i:{i.netID}]:{i.Name}(ID{i.netID})"));
                return;
            }
            else
            {
                item = matchedItems[0];
            }
            if (item.type < 1 && item.type >= Main.maxItemTypes)
            {
                args.Player.SendErrorMessage("指定物品 {0} 无效.", itemName);
                return;
            }


            int rare = 0;
            if (!LC.LConfig.替换品级表.TryGetValue(item.netID, out rare))
                rare = item.rare;
            rare *= LC.LConfig.装备品质系数;

            if (item.Name.Length >= 3)
                if (item.Name.Substring(item.Name.Length - 2, 2) == "染料" || item.Name.Substring(item.Name.Length - 3, 3) == "Dye")
                {
                    args.Player.SendSuccessMessage("染料 [i:" + item.netID + "]:" + item.Name + " 等级为:" + rare);
                    return;
                }

            if (item.fishingPole > 0)
                args.Player.SendSuccessMessage("鱼竿 [i:" + item.netID + "]:" + item.Name + " 等级为:" + rare);
            else if ((item.headSlot > -1 || item.bodySlot > -1 || item.legSlot > -1))
                args.Player.SendSuccessMessage("盔甲 [i:" + item.netID + "]:" + item.Name + " 等级为:" + rare);
            else if ((item.accessory || item.vanity))
                args.Player.SendSuccessMessage("饰品 [i:" + item.netID + "]:" + item.Name + " 等级为:" + rare);
            else if (item.hammer > 0 || item.axe > 0 || item.pick > 0)
                if (LC.LConfig.不受前缀影响)
                    args.Player.SendSuccessMessage("工具 [i:" + item.netID + "]:" + item.Name + " 等级为:" + rare);
                else
                { args.Player.SendSuccessMessage("无前缀的工具 [i:" + item.netID + "]:" + item.Name + " 等级为:" + rare); }
            else if (item.summon || item.ranged || item.magic || item.melee)
                if (LC.LConfig.不受前缀影响)
                    args.Player.SendSuccessMessage("武器 [i:" + item.netID + "]:" + item.Name + " 等级为:" + rare);
                else
                { args.Player.SendSuccessMessage("无前缀的武器 [i:" + item.netID + "]:" + item.Name + " 等级为:" + rare); }
            else
                args.Player.SendSuccessMessage("物品 [i:" + item.netID + "]:" + item.Name + " 无等级限制");

        }
        private void OnFulfill(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn || args.Player.Account == null || args.Player.Account.ID == 0)
            { args.Player.SendErrorMessage("你必须登录才能使用此指令。"); return; }
            lock (LC.LPlayers)
            {
                var lpy = LC.LPlayers[args.Player.Index];
                if (lpy == null) return;

                if (Main.hardMode)//等级限制
                {


                    if (!NPC.downedGolemBoss)//石巨人没杀
                        if (LC.LConfig.巨人最高等级 != -1)
                            if (LC.LConfig.巨人最高等级 <= lpy.Leve)
                            { args.Player.SendErrorMessage("您已达到巨人最高级别，无法完成任务！"); return; }


                    if (LC.LConfig.最高等级 != -1)
                        if (LC.LConfig.最高等级 <= lpy.Leve)
                        { args.Player.SendErrorMessage("您已达到最高级别，无法完成任务！"); return; }

                }
                else if (LC.LConfig.肉前最高等级 != -1)
                {
                    if (LC.LConfig.肉前最高等级 <= lpy.Leve)
                    { args.Player.SendErrorMessage("您已达到肉前最高级别，无法完成任务！"); return; }
                }



                List<LTask> ltask;
                if (LC.LConfig.等级任务.TryGetValue(lpy.Leve, out ltask))//是否包含,并赋值到ltask
                {
                    if (ltask.Count - 1 >= lpy.TaskID)
                    {
                        if (ltask[lpy.TaskID].类型 == 1 || ltask[lpy.TaskID].类型 == 2 || ltask[lpy.TaskID].类型 == 5 || ltask[lpy.TaskID].类型 == 6)
                        {
                            if (lpy.TaskNumber < ltask[lpy.TaskID].目标数量)
                            { args.Player.SendErrorMessage("您无法完成任务，因为您还没有达成任务条件！"); return; }

                            if (ltask[lpy.TaskID].剧情 != "") args.Player.SendMessage(ltask[lpy.TaskID].剧情.Replace("{name}", args.Player.Name), new Color(255, 255, 255));
                            args.Player.SendSuccessMessage("成功完成任务：{0}", ltask[lpy.TaskID].标题);

                            int exp = ltask[lpy.TaskID].经验;
                            lpy.TaskID++;
                            lpy.TaskNumber = 0;
                            lpy.AddExp(exp);

                            return;
                        }
                        else if (ltask[lpy.TaskID].类型 == 0)
                        {
                            for (int i = 0; i < 58; i++)// 计次循环检查玩家背包库存，只有50个
                            {
                                if (args.TPlayer.inventory[i].netID == ltask[lpy.TaskID].目标ID)//若此物品id与提供的复活币id相等
                                {
                                    //slot = i;// 此时循环数i为槽位，定义此槽位稍后用

                                    if (args.TPlayer.inventory[i].stack < ltask[lpy.TaskID].目标数量)
                                    {
                                        args.Player.SendErrorMessage("您无法完成任务，因为您拥有的单组数量不足！");
                                        return;
                                    }

                                    //args.TPlayer.inventory[i].stack -= ltask[lpy.TaskID].目标数量;
                                    //args.Player.SendData(PacketTypes.PlayerSlot, "", args.Player.Index, i, args.TPlayer.inventory[i].prefix);
                                    if (ltask[lpy.TaskID].剧情 != "") args.Player.SendMessage(ltask[lpy.TaskID].剧情, new Color(255, 255, 255));
                                    args.Player.SendSuccessMessage("成功完成任务：{0}", ltask[lpy.TaskID].标题);


                                    int exp = ltask[lpy.TaskID].经验;
                                    lpy.TaskID++;
                                    lpy.TaskNumber = 0;
                                    lpy.AddExp(exp);




                                    return;//返回
                                }
                            }
                            args.Player.SendErrorMessage("您无法完成任务，因为您没有指定物品！");
                            return;

                        }
                        else if (ltask[lpy.TaskID].类型 == 3)
                        {
                            if (args.TPlayer.armor.Count(p => p.netID == ltask[lpy.TaskID].目标ID) <= 0)
                            { args.Player.SendErrorMessage("您无法完成任务，因为您还没有达成任务条件！"); return; }

                            if (ltask[lpy.TaskID].剧情 != "") args.Player.SendMessage(ltask[lpy.TaskID].剧情, new Color(255, 255, 255));
                            args.Player.SendSuccessMessage("成功完成任务：{0}", ltask[lpy.TaskID].标题);

                            int exp = ltask[lpy.TaskID].经验;
                            lpy.TaskID++;
                            lpy.TaskNumber = 0;
                            lpy.AddExp(exp);

                            return;
                        }
                        else if (ltask[lpy.TaskID].类型 == 4)
                        {
                            for (int i = 0; i < 58; i++)// 计次循环检查玩家背包库存，只有50个
                            {
                                if (args.TPlayer.inventory[i].netID == ltask[lpy.TaskID].目标ID)//若此物品id与提供的复活币id相等
                                {
                                    //slot = i;// 此时循环数i为槽位，定义此槽位稍后用

                                    if (args.TPlayer.inventory[i].stack < ltask[lpy.TaskID].目标数量)
                                    {
                                        args.Player.SendErrorMessage("您无法完成任务，因为您拥有的单组数量不足！");
                                        return;
                                    }

                                    args.TPlayer.inventory[i].stack -= ltask[lpy.TaskID].目标数量;
                                    args.Player.SendData(PacketTypes.PlayerSlot, "", args.Player.Index, i, args.TPlayer.inventory[i].prefix);

                                    if (ltask[lpy.TaskID].剧情 != "") args.Player.SendMessage(ltask[lpy.TaskID].剧情, new Color(255, 255, 255));

                                    args.Player.SendSuccessMessage("成功完成任务：{0}", ltask[lpy.TaskID].标题);

                                    int exp = ltask[lpy.TaskID].经验;
                                    lpy.TaskID++;
                                    lpy.TaskNumber = 0;
                                    lpy.AddExp(exp);

                                    return;//返回
                                }
                            }
                            args.Player.SendErrorMessage("您无法完成任务，因为您没有指定物品！");
                            return;


                        }


                    }



                }

                args.Player.SendErrorMessage("你当前没用任务可完成。");

            }


        }
        private void OnStopBuffCmd(CommandArgs args)//指令
        {
            if (!args.Player.IsLoggedIn || args.Player.Account == null || args.Player.Account.ID == 0)
            { args.Player.SendErrorMessage("你必须登录才能使用此指令。"); return; }
            lock (LC.LPlayers)
                if (LC.LPlayers[args.Player.Index] == null) args.Player.SendErrorMessage("你必须登录才能使用此指令。");
                else
                {
                    if (LC.LPlayers[args.Player.Index].StopBuff)
                    {
                        LC.LPlayers[args.Player.Index].StopBuff = false;
                        args.Player.SendSuccessMessage("您的等级BUFF已恢复更新！");
                    }
                    else
                    {
                        LC.LPlayers[args.Player.Index].StopBuff = true;
                        args.Player.SendSuccessMessage("您的等级BUFF已停止更新！");

                    }
                    LC.LPlayers[args.Player.Index].UpDataDB(false);
                }
        }
        private void OnHidingInfoCmd(CommandArgs args)//指令
        {
            if (!args.Player.IsLoggedIn || args.Player.Account == null || args.Player.Account.ID == 0)
            { args.Player.SendErrorMessage("你必须登录才能使用此指令。"); return; }
            lock (LC.LPlayers)
                if (LC.LPlayers[args.Player.Index] == null) args.Player.SendErrorMessage("你必须登录才能使用此指令。");
                else
                {
                    if (LC.LPlayers[args.Player.Index].HidingInfo)
                    {
                        LC.LPlayers[args.Player.Index].HidingInfo = false;
                        args.Player.SendSuccessMessage("您的实时信息已恢复！");
                    }
                    else
                    {
                        LC.LPlayers[args.Player.Index].HidingInfo = true;
                        args.Player.SendSuccessMessage("您的实时信息已隐藏！");
                        args.Player.SendData(PacketTypes.Status, "", 1, 1);
                    }
                    LC.LPlayers[args.Player.Index].UpDataDB(false);
                }
        }
        public void PostInitialize(EventArgs e)
        {
            //RH(); 
            Update.Elapsed += OnUpdate; Update.Start();//只有读入地图后才能取得地图ID等地图信息否则地图ID为空//启动时钟
        }

        private void OnGreetPlayer(GreetPlayerEventArgs e)//进入游戏时
        {
            //lock (LC.LPlayers)//锁定
            //    LC.LPlayers[e.Who] = new LPlayer(e.Who);

            //Console.WriteLine("进入"+e.Who);
            //if (LC.LConfig.启动饥荒)
            //    TShock.Players[e.Who].SetBuff(26, 3600 * LC.LConfig.进入饱食);//暂时无法记录玩家的buff，因此只能为上线时获得5分钟buff

        }
        private void OnLeave(LeaveEventArgs e)//退出游戏
        {
            lock (LC.LPlayers)//锁定
                if (LC.LPlayers[e.Who] != null)
                {
                    LC.LPlayers[e.Who].UpDataDB(true);
                    LC.LPlayers[e.Who] = null;
                }



            //var user = TShock.Players[e.Who];
            //if (user == null) return; //若用户不存在直接返回
            //int b26 = 0;
            //int b206 = 0;
            //int b207 = 0;

            //for (int i = 0; i < user.TPlayer.buffType.Count(); i++)
            //{
            //    if (user.TPlayer.buffType[i] == 26) b26 = user.TPlayer.buffTime[i];
            //    if (user.TPlayer.buffType[i] == 206) b206 = user.TPlayer.buffTime[i];
            //    if (user.TPlayer.buffType[i] == 207) b207 = user.TPlayer.buffTime[i];
            //}


            //try
            //{
            //    using (var SQLWriter = TShock.DB.QueryReader("SELECT * FROM 饥荒模式 WHERE ID=@0", user.Account.ID))
            //    {
            //        if (SQLWriter.Read())//如果读出来了，只需要更新
            //        {
            //            int q = TShock.DB.Query("UPDATE 饥荒模式 SET b26=@0, b206=@1, b207=@2 WHERE ID=@3",
            //                      b26, b206, b207, user.Account.ID);
            //            if (q == 0) Console.WriteLine(user.Name + "饥荒数据库更新失败！");

            //        }
            //        else//不然直接添加新的
            //        {
            //            int q = TShock.DB.Query("INSERT INTO 饥荒模式 (ID, b26, b206, b207) VALUES (@0, @1, @2, @3);",
            //                            user.Account.ID, b26, b206, b207);
            //            if (q == 0)Console.WriteLine(user.Name + "饥荒数据库添加失败！");

            //        }

            //    }



            //}
            //catch (Exception ex)
            //{
            //    TShock.Log.ConsoleError("饥荒模式数据库写入错误:" + ex.ToString());
            //}

        }


        private void OnChat(ServerChatEventArgs args)//聊天的狗子程序
        {
            //string prefix = "";
            //try
            //{
            //    using (var SQLWriter = TShock.DB.QueryReader("SELECT * FROM GroupList WHERE GroupName=@0", TShock.Players[args.Who].Group.Name))
            //    {
            //        if (SQLWriter.Read())//如果读出来了
            //        {
            //            prefix = SQLWriter.Get<string>("Prefix");

            //        }
            //        else//不然判断是否是超管
            //        {
            //            if (TShock.Players[args.Who].Group.Name == "superadmin")
            //                prefix = TShock.Config.SuperAdminChatPrefix;
            //            else
            //                TShock.Log.ConsoleError(TShock.Players[args.Who].Group.Name + "饥荒RPG前缀未知！" );
            //        }

            //    }

            //}
            //catch (Exception ex)
            //{
            //    TShock.Log.ConsoleError("饥荒RPG前缀读取错误:" + ex.ToString());
            //}
            //以上内容可以考虑在进入服务器的时候只读一遍，但是代价是中途如果调整分组则不会及时显示前缀


            lock (LC.LPlayers)
                if (LC.LPlayers[args.Who] != null)
                {
                    if (LC.LPlayers[args.Who].Leve != -1)//一旦不是-1就不可能是-1
                    {
                        LC.LPlayers[args.Who].GetChatCustom();

                        TShock.Players[args.Who].Group.Prefix = "[LV" + LC.LPlayers[args.Who].Leve + "]" + LC.LPlayers[args.Who].Prefix;
                        TShock.Players[args.Who].Group.ChatColor = LC.LPlayers[args.Who].ChatColor;
                        TShock.Players[args.Who].Group.Suffix = LC.LPlayers[args.Who].Suffix;
                    }

                    //else
                }


            //由于每个人都定义了前缀所以也没啥不妥的
        }

        private void NpcSpawn(NpcSpawnEventArgs args)
        {
            if (args.Handled) return;

            if (Main.npc[args.NpcId].active)
            {
                lock (LC.LNpcs)//锁定
                    LC.LNpcs[args.NpcId] = new LNPC(args.NpcId);
            }
            //Console.WriteLine("怪物" + Main.npc[args.NpcId].netID + "产生");
        }
        private void NpcKilled(NpcKilledEventArgs args)
        {
            lock (LC.LNpcs)//锁定
                if (LC.LNpcs[args.npc.whoAmI] != null)
                {
                    Dictionary<string, int> damage = LC.LNpcs[args.npc.whoAmI].Damage;
                    int time = (int)(DateTime.UtcNow - LC.LNpcs[args.npc.whoAmI].StartTiem).TotalSeconds;

                    //Console.WriteLine(args.npc.lastInteraction);

                    TSPlayer user;

                    if (args.npc.lastInteraction == 255 || args.npc.lastInteraction < 0)
                        user = null;
                    else
                        user = TShock.Players[args.npc.lastInteraction];//最后互动玩家




                    if (args.npc.boss)//只有boss才宣读伤害
                    {
                        if (LC.LConfig.宣读老怪击杀)
                            if (user != null)
                                TShock.Utils.Broadcast("" + user.Name + "终结了[" + args.npc.FullName + "]共" + damage.Count + "人参加了这场战斗,耗时" + time + "秒.", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));
                            else
                                TShock.Utils.Broadcast("" + "[" + args.npc.FullName + "]被终结了,共" + damage.Count + "人参加了这场战斗,耗时" + time + "秒.", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));


                        if (LC.LConfig.追踪老怪击杀 && user != null) TShock.Log.Info("{0} 击杀了 {1}", user.Name, args.npc.FullName);



                        int num = 0;
                        int othernum = 0;
                        List<LDM> list = new List<LDM>();
                        foreach (var key in damage.Keys)
                        {
                            //Console.WriteLine(key);

                            if (user != null)
                                if (key != user.Name)
                                { othernum += damage[key]; }//排除击杀者的伤害

                            num += damage[key];
                            list.Add(new LDM(key, damage[key]));
                        }

                        //Console.WriteLine("含击杀总伤害{0},除外{1}",num, othernum);

                        list.Sort((LDM p1, LDM p2) => p1.Damage.CompareTo(p2.Damage));
                        list.Reverse();
                        var nlist = new List<string>();
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (user != null)
                            {
                                if (list[i].Name != user.Name)
                                    list[i].Proportion = list[i].Damage * 100 / othernum;//只有没击杀的才写入比例
                            }
                            else
                                list[i].Proportion = list[i].Damage * 100 / othernum;//如果击杀者消失,那么所有人按比例


                            if (i + 1 > LC.LConfig.输出表单名数) continue;
                            string text = string.Format("{0}.[{1}]输出{2}%为{3}", i + 1, list[i].Name, list[i].Damage * 100 / num, list[i].Damage);
                            if (LC.LConfig.追踪输出名单) TShock.Log.Info(text);
                            nlist.Add(text);
                        }
                        //Console.WriteLine(num);
                        foreach (var pl in TShock.Players)
                        {
                            if (pl == null) continue;
                            if (!list.Exists(t => t.Name == pl.Name)) continue;//只有参与击杀的才有提示
                            if (LC.LConfig.宣读老怪击杀)
                                foreach (var txt in nlist)
                                    pl.SendInfoMessage(txt);
                        }

                        for (int i = 0; i < list.Count; i++)
                        {
                            bool handle = false;
                            if (user != null)
                            {
                                if (list[i].Name != user.Name)
                                    handle = true;
                            }
                            else handle = true;//否则所有人按比例


                            if (handle)
                                foreach (var pl in TShock.Players)
                                {
                                    if (pl == null) continue;
                                    if (list[i].Name == pl.Name)
                                    {
                                        //Console.WriteLine("玩家{0}比例{1}",list[i].Name, list[i].Proportion);
                                        lock (LC.LPlayers)//对其按比例添加经验
                                        {
                                            var lpy = LC.LPlayers[pl.Index];
                                            if (lpy == null) break;
                                            //Console.WriteLine(pl.Name+"添加血量{0}的经验", args.Npc.lifeMax);
                                            lpy.AddExp(args.npc, list[i].Proportion);
                                            break;
                                        }
                                    }
                                }

                        }

                    }

                    if (user != null)
                        lock (LC.LPlayers)
                        {
                            var lpy = LC.LPlayers[user.Index];
                            if (lpy == null) return;

                            if (!lpy.IsMaxLeve())
                            {
                                List<LTask> ltask;
                                if (LC.LConfig.等级任务.TryGetValue(lpy.Leve, out ltask))//是否包含,并赋值到ltask
                                {
                                    if (ltask.Count - 1 >= lpy.TaskID)
                                    {
                                        if (ltask[lpy.TaskID].类型 == 1 && args.npc.netID == ltask[lpy.TaskID].目标ID)
                                        {
                                            if (lpy.TaskNumber < ltask[lpy.TaskID].目标数量)
                                                lpy.TaskNumber++;

                                            //此处可以判定自动完成,但没必要
                                        }
                                    }
                                }
                            }


                            //此处可以实际上可以添加杀怪统计

                            lpy.AddExp(args.npc, 100);//自动计算
                            lpy.AddTeamExp(LC.LConfig.小队杀怪经验);//杀怪经验
                            LC.AddSE();//如果击杀的是正常的怪添加服务器公共经验
                        }


                    LC.LNpcs[args.npc.whoAmI] = null;
                }

        }



        private void NpcStrike(NpcStrikeEventArgs args)
        {
            if (args.Handled) return;
            var user = TShock.Players[args.Player.whoAmI];
            if (user == null) return;//若用户不存在直接返回
            if (args.Damage < 1) return;
            if (!args.Npc.active) return;


            int dm = args.Damage;

            if (!user.HasPermission("忽略伤害限制"))
                if (LC.LConfig.伤害误判次数 > 0)
                {
                    int dpsL = 0;
                    int dL = 0;
                    int dk = 0;
                    long dps = 0;
                    int lv = 0;
                    lock (LC.LPlayers)
                    {
                        var lpy = LC.LPlayers[user.Index];
                        if (lpy != null)
                        {
                            int leve = 0;//公共等级
                            if (LC.LConfig.启动公共等级)
                            {
                                if (LC.ServeLeve > lpy.Leve)
                                    leve = LC.ServeLeve;
                                else leve = lpy.Leve;
                            }
                            else leve = lpy.Leve;

                            lv = leve;

                            dpsL = Sundry.getDPSbyLeve(leve);
                            dL = Sundry.getDamagebyLeve(leve);

                            if ((DateTime.UtcNow - lpy.DTiem).TotalMilliseconds > 60000)//60秒重置一次
                            { lpy.DKick = 0; lpy.DTiem = DateTime.UtcNow; }

                            if (dpsL > 0)
                            {
                                if ((DateTime.UtcNow - lpy.DPSLastTiem).TotalMilliseconds < 1000) lpy.DPS += dm;
                                else { lpy.DPS = dm; lpy.DPSLastTiem = DateTime.UtcNow; }

                                if (lpy.DPS > dpsL)
                                    lpy.DKick++;
                            }
                            if (dL > 0)
                            {
                                if (dm > dL)
                                    lpy.DKick++;
                            }




                            dk = lpy.DKick;
                            dps = lpy.DPS;
                        }
                    }

                    //Console.WriteLine("调试{0},dm{1},dk{2},dps{3}dpsl{4}dl{5}.", user.Name,dm,dk,dps,dpsL ,dL );

                    if (dk > 0 && dk >= LC.LConfig.伤害误判次数)
                    {
                        TShock.Log.Info("{0} 对 {1}({2}) 造成越级伤害 {3} (DPS {4}) 此时手持物品 {5}({6}) 适用等级 {7}", user.Name, args.Npc.FullName, args.Npc.netID, dm, dps, user.SelectedItem.Name, user.SelectedItem.netID, lv);

                        string txt = string.Format("玩家 {0} 伤害超过其{1}级限制:", user.Name, lv);
                        if (dm > dL && dL > 0)
                        {
                            txt += string.Format(" 伤害 {0} > {1}", dm, dL);
                        }

                        if (dps > dpsL && dpsL > 0)
                        {
                            txt += string.Format(" DPS {0} > {1}", dps, dpsL);
                        }


                        user.Kick(txt, true, false, "Server");
                        args.Handled = true;
                        return;
                    }

                }

            if (args.Critical) dm *= 2;//伤害限制不计入暴击,所以放到后面

            if (args.Npc.boss) //此乃没有击杀，但只有boss才记录伤害
                lock (LC.LNpcs)
                {
                    if (LC.LNpcs[args.Npc.whoAmI] == null) return;

                    if (LC.LNpcs[args.Npc.whoAmI].Dead) return;//死了就不再加了

                    if (LC.LNpcs[args.Npc.whoAmI].Damage.ContainsKey(args.Player.name))
                    {
                        LC.LNpcs[args.Npc.whoAmI].Damage[args.Player.name] += dm;
                    }
                    else LC.LNpcs[args.Npc.whoAmI].Damage.Add(args.Player.name, dm);


                    if (dm >= args.Npc.life) LC.LNpcs[args.Npc.whoAmI].Dead = true;

                }

            if (LC.LConfig.显示击打提示 && args.Npc.life >= LC.LConfig.击打提示最低 && dm < args.Npc.life)
            {
                lock (LC.LPlayers)
                {
                    if (LC.LPlayers[args.Player.whoAmI] == null) return;
                    if ((DateTime.UtcNow - LC.LPlayers[args.Player.whoAmI].LastStrikeTime).TotalMilliseconds < 900) return;
                    LC.LPlayers[args.Player.whoAmI].LastStrikeTime = DateTime.UtcNow;
                }

                Color c = new Color(0, 255, 255);
                int a = args.Npc.life * 100 / args.Npc.lifeMax;
                //string text = "[" + args.Npc.FullName + "]剩" + a + "%血:" + args.Npc.life + "";
                string text = args.Npc.FullName + "▕";
                int b = a % 5;
                if (b == 0)
                    b = a / 5;
                else
                    b = (a / 5) + 1;
                for (int i = 1; i <= 20; i++)
                {
                    if (i <= b)
                        text += "▮";
                    else
                        text += "▯";
                }
                text += "▏" + a + "%";

                user.SendData(PacketTypes.CreateCombatTextExtended, text, (int)c.PackedValue, user.X, user.Y);
            }




            #region 旧判定方式
            //if ((args.Damage == -1 || args.Damage >= args.Npc.life) && args.Npc.active)//此乃击杀
            //{
            //    //LNPC npc;
            //    Dictionary<string, int> damage;
            //    bool dead;
            //    int time ;
            //    lock (LC.LNpcs)
            //    {
            //        if (LC.LNpcs[args.Npc.whoAmI] == null) return;
            //        if (args.Npc.boss)//只有boss才记录伤害
            //            if (args.Damage > 0)
            //            {
            //                int dm = args.Damage;
            //                if (args.Critical) dm *= 2;
            //                if (LC.LNpcs[args.Npc.whoAmI].Damage.ContainsKey(args.Player.name))
            //                {
            //                    LC.LNpcs[args.Npc.whoAmI].Damage[args.Player.name] += dm;
            //                }
            //                else LC.LNpcs[args.Npc.whoAmI].Damage.Add(args.Player.name, dm);
            //            }
            //        //npc = LC.LNpcs[args.Npc.whoAmI];
            //        damage = LC.LNpcs[args.Npc.whoAmI].Damage;
            //        dead = LC.LNpcs[args.Npc.whoAmI].Dead;
            //        LC.LNpcs[args.Npc.whoAmI].Dead = true;

            //        time = (int)(DateTime.UtcNow - LC.LNpcs[args.Npc.whoAmI].StartTiem).TotalSeconds;
            //    }

            //    //if (npc == null) return;
            //    if (dead) return;//跳过鞭尸


            //    if (args.Npc.boss)//只有boss才宣读伤害
            //    {

            //        if (LC.LConfig.宣读老怪击杀)
            //            TShock.Utils.Broadcast("" + args.Player.name + "终结了[" + args.Npc.FullName + "]共" + damage.Count + "人参加了这场战斗,耗时"+ time + "秒.", Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(100));


            //        if (LC.LConfig.追踪老怪击杀) TShock.Log.Info("{0} 击杀了 {1}", args.Player.name, args.Npc.FullName);
            //        int num = 0;
            //        int othernum = 0;
            //        List<LDM> list = new List<LDM>();
            //        foreach (var key in damage.Keys)
            //        {
            //            //Console.WriteLine(key);
            //            if (key != args.Player.name)
            //            { othernum+= damage[key]; }//排除击杀者的伤害

            //            num += damage[key];
            //            list.Add(new LDM(key, damage[key]));
            //        }

            //        //Console.WriteLine("含击杀总伤害{0},除外{1}",num, othernum);

            //        list.Sort((LDM p1, LDM p2) => p1.Damage.CompareTo(p2.Damage));
            //        list.Reverse();
            //        var nlist = new List<string>();
            //        for (int i = 0; i < list.Count; i++)
            //        {
            //            if (list[i].Name != args.Player.name)
            //                list[i].Proportion = list[i].Damage * 100 / othernum;//只有没击杀的才写入比例

            //            if (i + 1 > LC.LConfig.输出表单名数) continue;
            //            string text = string.Format("{0}.[{1}]输出{2}%为{3}", i + 1, list[i].Name, list[i].Damage * 100 / num, list[i].Damage);
            //            if (LC.LConfig.追踪输出名单) TShock.Log.Info(text);
            //            nlist.Add(text);
            //        }
            //        //Console.WriteLine(num);
            //        foreach (var pl in TShock.Players)
            //        {
            //            if (pl == null) continue;
            //            if (!list.Exists(t => t.Name == pl.Name)) continue;//只有参与击杀的才有提示
            //            if (LC.LConfig.宣读老怪击杀)
            //                foreach (var txt in nlist)
            //                    pl.SendInfoMessage(txt);
            //        }

            //        for (int i = 0; i < list.Count; i++)
            //        {
            //            if (list[i].Name != args.Player.name)
            //                foreach (var pl in TShock.Players)
            //                {
            //                    if (pl == null) continue;
            //                    if (list[i].Name == pl.Name)
            //                    {
            //                        //Console.WriteLine("玩家{0}比例{1}",list[i].Name, list[i].Proportion);
            //                        lock (LC.LPlayers)//对其按比例添加经验
            //                        {
            //                            var lpy = LC.LPlayers[pl.Index];
            //                            if (lpy == null) break;
            //                            //Console.WriteLine(pl.Name+"添加血量{0}的经验", args.Npc.lifeMax);
            //                            lpy.AddExp(args.Npc, list[i].Proportion);
            //                            break;
            //                        }
            //                    }
            //                }

            //        }


            //    }

            //    lock (LC.LPlayers)
            //    {
            //        var lpy = LC.LPlayers[args.Player.whoAmI];
            //        if (lpy == null) return;



            //        if (!lpy.IsMaxLeve())
            //        {
            //            List<LTask> ltask;
            //            if (LC.LConfig.等级任务.TryGetValue(lpy.Leve, out ltask))//是否包含,并赋值到ltask
            //            {
            //                if (ltask.Count - 1 >= lpy.TaskID)
            //                {
            //                    if (ltask[lpy.TaskID].类型 == 1 && args.Npc.netID == ltask[lpy.TaskID].目标ID)
            //                    {

            //                        if (lpy.TaskNumber < ltask[lpy.TaskID].目标数量)
            //                            lpy.TaskNumber++;

            //                        //此处可以判定自动完成,但没必要
            //                    }

            //                }

            //            }
            //        }




            //        //if (args.Npc.lifeMax < Sundry.ComputeNoExpByLeve(lpy.Leve)) return;//跳过最小等级多此一举

            //        //Console.WriteLine(args.Player.name+"添加经验" + Sundry.ComputeExpByKillLife(args.Npc.lifeMax));

            //        lpy.AddExp(args.Npc, 100);//自动计算

            //        //lpy.AddExp(Sundry.ComputeExpByKillLife(args.Npc.lifeMax));//根据击杀的怪物血量计算得到经验


            //        //if (args.Npc.lifeMax >= Sundry.ComputeNoExpByLeve(LC.ServeLeve))//还是不加限制比较好
            //            LC.AddSE();//如果击杀的是正常的怪添加服务器公共经验


            //    }

            //}
            //else
            //{
            //    if (args.Npc.boss) //此乃没有击杀，但只有boss才记录伤害
            //        lock (LC.LNpcs)
            //        {
            //            if (LC.LNpcs[args.Npc.whoAmI] == null) return;
            //            int dm = args.Damage;
            //            if (args.Critical) dm *= 2;
            //            if (LC.LNpcs[args.Npc.whoAmI].Damage.ContainsKey(args.Player.name))
            //            {
            //                LC.LNpcs[args.Npc.whoAmI].Damage[args.Player.name] += dm;
            //            }
            //            else LC.LNpcs[args.Npc.whoAmI].Damage.Add(args.Player.name, dm);
            //        }

            //    if (LC.LConfig.显示击打提示 && args.Npc.lifeMax >= LC.LConfig.击打提示最低)
            //    {
            //        lock (LC.LPlayers)
            //        {
            //            if (LC.LPlayers[args.Player.whoAmI] == null) return;
            //            if ((DateTime.UtcNow - LC.LPlayers[args.Player.whoAmI].LastStrikeTime).TotalMilliseconds < 900) return;
            //            LC.LPlayers[args.Player.whoAmI].LastStrikeTime = DateTime.UtcNow;
            //        }

            //        Color c = new Color(30, 150, 255);
            //        int a = args.Npc.life * 100 / args.Npc.lifeMax;
            //        string text = "[" + args.Npc.FullName + "]剩" + a + "%血:" + args.Npc.life + "";
            //        user.SendData(PacketTypes.CreateCombatTextExtended, text, (int)c.PackedValue, user.X, user.Y);
            //    }


            //}


            #endregion



        }




        #region 收到数据

        private void GetData(GetDataEventArgs args)//收到数据的狗子程序
        {
            if (args.Handled) return;
            //Console.WriteLine("收到数据" + args.MsgID);
            var user = TShock.Players[args.Msg.whoAmI];
            if (user == null) return; //若用户不存在直接返回
            if (!user.ConnectionAlive) return; //若已丢失连接直接返回

            using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
            {
                try { if (GetDataHandlers.HandlerGetData(args.MsgID, user, data)) args.Handled = true; }
                catch (Exception ex) { TShock.Log.Error("饥荒RPG插件错误传递时出错:" + ex.ToString()); }
            }


        }
        #endregion
        #region 时钟事件
        public void OnUpdate(object sender, ElapsedEventArgs e)//时钟更新数据时
        {

            //Console.WriteLine("时钟进行1");

            if (ULock) return;//若关闭了提示或时钟正在执行过程中，则这次放弃执行
            ULock = true; var Start = DateTime.Now;//Console.WriteLine("时钟正常启动" );

            bool check = false;

            //Console.WriteLine("时钟进行2");
            if (LC.CheckTime < 3) LC.CheckTime++;//装备检测周期
            else
            {
                LC.CheckTime = 0;//重置
                check = true;
            }


            //lock (TShock.Players)
            foreach (TSPlayer user in TShock.Players)
            {
                if (user == null) continue;//空白跳过
                if (Timeout(Start)) { ULock = false; return; }//检测耗时超时

                //Console.WriteLine("测试1:"+ user.Index);
                //Console.WriteLine("时钟进行3");

                lock (LC.LPlayers)
                {
                    //Console.WriteLine("时钟进行4");

                    var lpy = LC.LPlayers[user.Index];
                    if (lpy == null) continue;

                    if (lpy.DisableItem < 5)
                    {
                        //Console.WriteLine("实施了清理");
                        lpy.ClearProj();//只要被禁止所有子弹都会被清掉，防止手速快的召唤师
                        lpy.DisableItem++;
                    }

                    if (LC.LConfig.实时信息类型 != 0)
                        if (!lpy.HidingInfo)
                        {
                            string text = "";


                            if (!lpy.IsMaxLeve())
                            {
                                List<LTask> ltask;

                                if (LC.LConfig.等级任务.TryGetValue(lpy.Leve, out ltask))//是否包含,并赋值到ltask
                                {
                                    if (ltask.Count - 1 >= lpy.TaskID)
                                    {
                                        text += "任务:" + ltask[lpy.TaskID].标题;

                                        if (ltask[lpy.TaskID].类型 == 1 || ltask[lpy.TaskID].类型 == 2 || ltask[lpy.TaskID].类型 == 5 || ltask[lpy.TaskID].类型 == 6)
                                        {
                                            text += " (" + lpy.TaskNumber + "/" + ltask[lpy.TaskID].目标数量 + ")";
                                        }

                                    }

                                }
                            }



                            if (text == "")
                                text = "饥荒RPG信息:";

                            if (LC.LConfig.实时信息类型 == 1)
                            {
                                string space = "                                                                           \n";//75
                                text += space;
                                text += "实时坐标:" + user.TileY + "," + user.TileY + "" + space;
                                text += "等级:" + lpy.Leve + " 经验:" + lpy.Exp + " (" + Sundry.ComputeExpP(lpy.Leve, lpy.Exp) + "%UP)" + space;//方案1


                                if (!lpy.IsMaxLeve())
                                {
                                    if (lpy.IsMaxExp()) text += "今天击杀经验奖励已满";//+ space;
                                    else text += "最低怪血:" + Sundry.ComputeNoExpByLeve(lpy.Leve) + lpy.MaxExpP();//+ space;
                                }
                                else text += "现阶段已达到最大等级";//+ space;

                            }
                            else if (LC.LConfig.实时信息类型 == 2)
                            {
                                text = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" + text;//19
                                text += "\n";
                                text += "实时坐标:" + user.TileX + "," + user.TileY + "\n";
                                text += "等级:" + lpy.Leve + " 经验:" + lpy.Exp + " (" + Sundry.ComputeExpP(lpy.Leve, lpy.Exp) + "%UP)\n";//方案2
                                if (!lpy.IsMaxLeve())
                                {
                                    if (lpy.IsMaxExp()) text += "今天击杀经验奖励已满" + "";
                                    else text += "最低怪血:" + Sundry.ComputeNoExpByLeve(lpy.Leve) + lpy.MaxExpP() + "";
                                }
                                else text += "现阶段已达到最大等级" + "";
                            }
                            else if (LC.LConfig.实时信息类型 == 3)
                            {
                                text += "\n";
                                text += "实时坐标:" + user.TileX + "," + user.TileY + "\n";
                                text += "等级:" + lpy.Leve + " 经验:" + lpy.Exp + " (" + Sundry.ComputeExpP(lpy.Leve, lpy.Exp) + "%UP)\n";//方案3//适用于手机

                                if (!lpy.IsMaxLeve())
                                {
                                    if (lpy.IsMaxExp()) text += "今天击杀经验奖励已满" + "";
                                    else text += "最低怪血:" + Sundry.ComputeNoExpByLeve(lpy.Leve) + lpy.MaxExpP() + "";
                                }
                                else text += "现阶段已达到最大等级" + "";

                            }
                            //Console.WriteLine("时钟进行6.1");

                            if (text != "") user.SendData(PacketTypes.Status, text, 0, 1);

                            //if (text != "") user.SendData(PacketTypes.Status, "ceshi", 55555555,555555555);//防闪
                        }


                    if (user.Dead || user.TPlayer.statLife < 1) continue;//跳过死掉的玩家



                    if (lpy.AutExpTime < LC.LConfig.在线奖励经验秒数)
                    {
                        lpy.AutExpTime++;
                        if (lpy.AutExpTime >= LC.LConfig.在线奖励经验秒数)
                        {
                            lpy.AutExpTime = 0;
                            lpy.AddExp(lpy.Leve, false);
                        }

                    }

                    if (lpy.AutTeamExpTime < LC.LConfig.在线小队经验秒数)
                    {
                        lpy.AutTeamExpTime++;
                        if (lpy.AutTeamExpTime >= LC.LConfig.在线小队经验秒数)
                        {
                            lpy.AutTeamExpTime = 0;
                            lpy.AddTeamExp(lpy.Leve);
                        }
                    }


                    if (check) //对玩家装备进行检查
                    {

                        int leve = 0;//公共等级
                        if (LC.LConfig.启动公共等级)
                        {
                            if (LC.ServeLeve > lpy.Leve)
                                leve = LC.ServeLeve;
                            else leve = lpy.Leve;
                        }
                        else leve = lpy.Leve;


                        bool disable = false;
                        if (!user.HasPermission("忽略染料等级"))//无权限则检查
                        {
                            foreach (var item in user.TPlayer.dye)
                            {
                                if (item == null) continue; if (item.netID == 0) continue;
                                int rare = 0;
                                if (!LC.LConfig.替换品级表.TryGetValue(item.netID, out rare))
                                    rare = item.rare;
                                rare *= LC.LConfig.装备品质系数;
                                if (rare > lpy.Leve)
                                {
                                    user.SendErrorMessage("您当前" + lpy.Leve + "级,无法穿戴" + rare + "级的染料 [i:" + item.netID + "] 请卸下它！");
                                    user.Disable("越级装备染料");
                                    lpy.DisableItem = 0;
                                    disable = true;
                                    break;
                                }
                            }
                            if (!disable)
                                foreach (var item in user.TPlayer.miscDyes)
                                {
                                    if (item == null) continue; if (item.netID == 0) continue;
                                    int rare = 0;
                                    if (!LC.LConfig.替换品级表.TryGetValue(item.netID, out rare))
                                        rare = item.rare;
                                    rare *= LC.LConfig.装备品质系数;
                                    if (rare > lpy.Leve)
                                    {
                                        user.SendErrorMessage("您当前" + lpy.Leve + "级,无法穿戴" + rare + "级的设备染料 [i:" + item.netID + "] 请卸下它！");
                                        user.Disable("越级装备设备染料");
                                        lpy.DisableItem = 0;
                                        disable = true;
                                        break;
                                    }
                                }
                        }
                        //foreach (var item in user.TPlayer.armor)
                        //    Console.WriteLine("测试1armor:" + item.Name);

                        if (!user.HasPermission("忽略盔甲等级") && !disable)//无权限则和没冻结时检查
                        {
                            for (int i = 0; i < user.TPlayer.armor.Count(); ++i)
                            {//前3是装备,但i是从0开始的
                                if (i >= 3) break;

                                if (user.TPlayer.armor[i] == null) continue; if (user.TPlayer.armor[i].netID == 0) continue;
                                var item = new Item();
                                item.netDefaults(user.TPlayer.armor[i].type);
                                item.Prefix(0);//忽略前缀影响
                                int rare = 0;
                                if (!LC.LConfig.替换品级表.TryGetValue(item.netID, out rare))
                                    rare = item.rare;
                                rare *= LC.LConfig.装备品质系数;
                                if (rare > leve)//盔甲吃公共等级
                                {
                                    if (LC.LConfig.记录违规穿戴) TShock.Log.Info("{0} 越级盔甲 {1}", user.Name, user.TPlayer.armor[i].Name);
                                    user.SendErrorMessage("您适用" + leve + "级,无法穿戴" + rare + "级的盔甲 [i:" + user.TPlayer.armor[i].netID + "] 请卸下它！");
                                    user.Disable("越级穿戴盔甲");
                                    lpy.DisableItem = 0;
                                    disable = true;


                                    if (LC.LConfig.提醒跨级使用)
                                        foreach (var pl in TShock.Players)
                                        {
                                            if (pl == null) continue;
                                            if (user.Name != pl.Name)
                                            {
                                                pl.SendInfoMessage("玩家 {0} 试图越级穿戴 {1}", user.Name, item.Name);
                                            }
                                        }

                                    break;
                                }

                            }
                        }


                        if (!user.HasPermission("忽略装备等级") && !disable)//无权限则和没冻结时检查
                        {
                            for (int i = 0; i < user.TPlayer.armor.Count(); ++i)
                            {//前10是装备,但i是从0开始的
                                if (i >= 10)
                                    break;
                                if (i < 3) continue;//3之前是防具

                                if (user.TPlayer.armor[i] == null) continue; if (user.TPlayer.armor[i].netID == 0) continue;
                                var item = new Item();
                                item.netDefaults(user.TPlayer.armor[i].type);
                                item.Prefix(0);//忽略前缀影响
                                int rare = 0;
                                if (!LC.LConfig.替换品级表.TryGetValue(item.netID, out rare))
                                    rare = item.rare;
                                rare *= LC.LConfig.装备品质系数;
                                if (rare > lpy.Leve)
                                {
                                    if (LC.LConfig.记录违规穿戴) TShock.Log.Info("{0} 越级装备 {1}", user.Name, user.TPlayer.armor[i].Name);
                                    user.SendErrorMessage("您当前" + lpy.Leve + "级,无法穿戴" + rare + "级的装备 [i:" + user.TPlayer.armor[i].netID + "] 请卸下它！");
                                    user.Disable("越级穿戴装备");
                                    lpy.DisableItem = 0;
                                    disable = true;

                                    if (LC.LConfig.提醒跨级使用)
                                        foreach (var pl in TShock.Players)
                                        {
                                            if (pl == null) continue;
                                            if (user.Name != pl.Name)
                                            {
                                                pl.SendInfoMessage("玩家 {0} 试图越级装备 {1}", user.Name, item.Name);
                                            }
                                        }


                                    break;
                                }

                            }
                        }
                        if (!user.HasPermission("忽略时装等级") && !disable)//无权限则和没冻结时检查
                        {
                            for (int i = 0; i < user.TPlayer.armor.Count(); ++i)
                            {//后10是时装
                                if (i < 10) continue; if (user.TPlayer.armor[i] == null) continue; if (user.TPlayer.armor[i].netID == 0) continue;
                                var item = new Item();
                                item.netDefaults(user.TPlayer.armor[i].type);
                                item.Prefix(0);//忽略前缀影响
                                int rare = 0;
                                if (!LC.LConfig.替换品级表.TryGetValue(item.netID, out rare))
                                    rare = item.rare;
                                rare *= LC.LConfig.装备品质系数;
                                if (rare > lpy.Leve)
                                {
                                    if (LC.LConfig.记录违规穿戴) TShock.Log.Info("{0} 越级装备时装 {1}", user.Name, user.TPlayer.armor[i].Name);
                                    user.SendErrorMessage("您当前" + lpy.Leve + "级,无法穿戴" + rare + "级的时装 [i:" + user.TPlayer.armor[i].netID + "] 请卸下它！");
                                    user.Disable("越级装备时装");
                                    lpy.DisableItem = 0;
                                    disable = true;
                                    break;
                                }
                            }
                        }
                        //if (!user.HasPermission("忽略设备等级") && !disable)//无权限则和没冻结时检查//不检测设备
                        //{
                        //    foreach (var item in user.TPlayer.miscEquips)
                        //    {
                        //        if (item == null) continue; if (item.netID == 0) continue;
                        //        if (item.rare > lpy.Leve)
                        //        {
                        //            if (LC.LConfig.记录违规穿戴) TShock.Log.Info("{0} 越级装备设备 {1}", user.Name, item.Name);
                        //            user.SendErrorMessage("您当前" + lpy.Leve + "级,无法穿戴" + item.rare + "级的设备:" + item.Name + "请卸下它！");
                        //            user.Disable("越级装备设备");
                        //            disable = true;
                        //            break;
                        //        }
                        //    }
                        //}



                    }



                    if (!lpy.StopBuff)
                    {
                        for (int i = 0; i < lpy.Buff.Count; i++)
                        {
                            if (lpy.Buff[i] > 0)
                                user.SetBuff(lpy.Buff[i], 333);
                        }
                        for (int i = 0; i < lpy.TeamBuff.Count; i++)
                        {
                            if (lpy.TeamBuff[i] > 0 || !lpy.Buff.Contains(lpy.TeamBuff[i]))//不能包含在自己状态里
                                user.SetBuff(lpy.TeamBuff[i], 333);
                        }
                    }




                    //Console.WriteLine("时钟进行6");


                    //Console.WriteLine("时钟进行7");

                    if (lpy.Leve <= LC.LConfig.饥饿保护等级)
                        continue;//因为下面是判断饥饿的，所以此处直接可以跳过


                    if (lpy.HungerTime > 0)
                    {
                        lpy.HungerTime--;
                        if (lpy.HungerTime == 0)
                            if (LC.LConfig.启动饥荒 && LC.LConfig.进入饱食 > 0)
                                user.SetBuff(26, 3600 * LC.LConfig.进入饱食);//暂时无法记录玩家的buff，因此只能为上线时获得5分钟buff

                        continue;//进服后n秒内无视饥饿
                    }

                }

                //Console.WriteLine("时钟进行8");
                if (LC.LConfig.睡觉不饥饿 && user.TPlayer.sleeping.isSleeping)
                    continue;
                if (LC.LConfig.启动饥荒 && !user.TPlayer.buffType.Contains(26) && !user.TPlayer.buffType.Contains(206) && !user.TPlayer.buffType.Contains(207) && !user.TPlayer.buffType.Contains(21))
                {
                    user.SetBuff(68, 60);
                    Color c = new Color(95, 150, 160);
                    user.SendData(PacketTypes.CreateCombatTextExtended, "饥饿使你感到窒息!", (int)c.PackedValue, user.X, user.Y);
                }



                //else if (!LC.LPlayers[user.Index].InitSpawn) continue;



                //Console.WriteLine("测试2");





                //user.SetBuff 

                //for (int i = 0; i < user.TPlayer.buffType.Count(); i++)
                //{
                //    Console.WriteLine("buff{0}时间{1}", user.TPlayer.buffType[i],user.TPlayer.buffTime[i].ToString());
                //}



            }

            ULock = false;
        }
        public static bool Timeout(DateTime Start, bool warn = true, int ms = 500)//参数1启始时间，参数2通告，3，超时时间
        {
            bool ret = (DateTime.Now - Start).TotalMilliseconds >= ms; if (ret) ULock = false;//超时自动解锁
            if (warn && ret) { TShock.Log.Error("饥荒RPG插件处理超时,已抛弃部分处理!"); }
            return ret;
        }
        #endregion
    }
}

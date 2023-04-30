using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Terraria;

namespace TestPlugin
{
    public class ConfigFile//json配置文件
    {
        public bool 启动饥荒 = true;
        public int 复活饱食 = 5;
        public int 进入饱食 = 5;
        public int 饥饿延迟 = 10;

        public bool 启动召唤限制 = false;
        public bool 启动入侵限制 = false;
        public int 每日召唤次数 = 1;
        public int 每日入侵次数 = 1;
        public int 叠加召唤上限 = 3;
        public int 叠加入侵上限 = 1;

        public int 日召唤起时间 = 1170;//包括
        public int 日召唤终时间 = 1320;//不包括,0点为24
        public int 日入侵起时间 = 1170;//包括
        public int 日入侵终时间 = 1320;//不包括,0点为24

        public int 周末额外召唤起时间 = 810;//包括
        public int 周末额外召唤终时间 = 930;//不包括,0点为24,若两者均为0则为全天
        public int 周末额外入侵起时间 = 810;//包括
        public int 周末额外入侵终时间 = 930;//不包括,0点为24


        public bool 追踪老怪击杀 = true;
        public bool 宣读老怪击杀 = true;
        public bool 追踪输出名单 = true;
        public int 输出表单名数 = 5;
        public bool 显示击打提示 = true;
        public int 击打提示最低 = 1500;
        public int 实时信息类型 = 2;//0关闭//1类型1//2类型2//3类型3（适用于手机）

        public int 肉前初始等级 = 0;
        public long 肉前初始经验 = 0;
        public bool 肉后经验补给 = false;
        public int 肉后初始等级 = 15;
        public long 肉后初始经验 = 3150;

        public int 最高等级 = 99;//达到后不吃经验-1表示无限
        public int 肉前最高等级 = 18;//达成后不吃经验-1表示不限
        public int 巨人最高等级 = 25;

        public long 肉前日最高经验 = 355;
        public long 巨人日最高经验 = 755;
        public long 日最高经验 = 1155;//-1不限
        public int 不限制经验级别 = 30;

        public bool 启动公共等级 = true;
        public int 公共最高等级 = 30;
        public int 公共肉前最高等级 = 18;//达成后不吃经验-1表示不限
        public int 公共巨人最高等级 = 25;

        public long 公共肉前日最高经验 = 755;//700=5,900=3.5
        public long 公共巨人日最高经验 = 1555;//1100=5,1500=3.5
        public long 公共日最高经验 = 1955;//-1不限//1300=5,1700=3.5

        public bool 启动小队部分 = false;
        public int 小队日最高经验 = 300;
        public int 小队杀怪经验 = 1;

        public bool 启动渔夫任务经验 = true;
        public int 在线奖励经验秒数 = 390;//0以上为开
        public int 在线小队经验秒数 = 390;//0以上为开

        public int PVP胜利经验 = 7;
        public int PVP失败经验 = -5;
        public int PVP保护等级 = 9;
        public int PVP保护级差 = 0;//两者相差最多N级才能获得经验，0表示两者等级必须相等才能获得经验，1表示两者等级相差1以内能相互获得-1禁用pvp经验

        public int 饥饿保护等级 = 2;//此等级以下包括此等级不受饥饿影响，-1表示不限
        public bool 记录违规穿戴 = true;
        public bool 记录跨级使用 = true;
        public bool 提醒跨级使用 = true;
        public int 装备品质系数 = 3;//装备品质要乘此值才为等级
        public bool 不受前缀影响 = true;

        public Dictionary<int, int> 替换品级表 = new Dictionary<int, int> { };
        public bool 睡觉不饥饿 = true;
        public int 老怪经验最低比例 = 3;
        public int 血月PVP倍率 = 3;

        public int 伤害误判次数 = 3;//每分钟清理一次,3表示第三次时踢,1表示第一次就踢,0表示不启动伤害限制
        public List<LDL> 等级伤害限制 = new List<LDL>();

        public bool 不允许跳过任务 = false;
        public Dictionary<int, int> 替换经验表 = new Dictionary<int, int> { };
        public Dictionary<int, List<LTask>> 等级任务 = new Dictionary<int, List<LTask>> { };




        public static ConfigFile Read(string Path)//给定文件进行读
        {
            if (!File.Exists(Path))
            {
                var lconfig = new ConfigFile();
                lconfig.替换品级表= new Dictionary<int, int>
                {
                    {5000,5 },
                    {908,5 },
                    {1309,2 },
                    {1862,5 },
                    {3017,4 },
                    {4989,8 },
                    {1164,5 },
                    {3999,3 },
                    {4004,3 },
                    {4003,4 },
                    {4881,5 },

                };
                lconfig.替换经验表 = new Dictionary<int, int>
                {
                    {5,0 },
                    {25,0 },
                    {30,0 },
                    {33,0 },
                    {112,0 },
                    {115,0 },
                    {116,0 },
                    {117,0 },
                    {118,0 },
                    {119,0 },
                    {139,0 },
                    {210,0 },
                    {211,0 },
                    {261,0 },
                    {264,0 },
                    {265,0 },
                    {267,0 },
                    {378,0 },
                    {384,0 },
                    {401,0 },
                    {406,0 },
                    {440,0 },
                    {454,0 },
                    {455,0 },
                    {456,0 },
                    {457,0 },
                    {458,0 },
                    {459,0 },
                    {472,0 },
                    {488,0 },
                    {516,0 },
                    {519,0 },
                    {521,0 },
                    {522,0 },
                    {523,0 },
                    {535,0 },
                    {566,0 },
                    {567,0 },
                    {594,0 },
                    {658,0 },
                    {659,0 },
                    {660,0 },
                };
                lconfig.等级伤害限制 = new List<LDL>()
                {
                    new LDL(0,8,95,1000),
                    new LDL(9,11,135,1500),
                    new LDL(12,14,155,3000),
                    new LDL(15,20,375,30000),
                    new LDL(21,26,1300,90000),
                    new LDL(27,30,1900,150000),
                };




                lconfig.等级任务 = new Dictionary<int, List<LTask>>
                {
                    //{1,ltask },


                };




                List<LTask> ltask = new List<LTask>();
                ltask.Add(new LTask("穿戴巨型蝴蝶结", 3,1906, 1, 1));
                lconfig.等级任务.Add(0, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "{name}:你好向导，我是{name}\n向导:欢迎来到这个世界!"));
                ltask.Add(new LTask("放置工作台", 2, 36, 1, 1));
                ltask.Add(new LTask("穿着木胸甲", 3, 728, 1, 1));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:很不错的盔甲，看来你准备好征服这个世界了!"));
                lconfig.等级任务.Add(1, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("挥动3下糖棒镐", 2, 1917, 3, 1));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "向导:很威武的敲击，现在我们需要火把来度过夜晚，击杀那些蓝史莱姆!"));
                ltask.Add(new LTask("击杀蓝史莱姆", 1, 1, 1, 2));
                ltask.Add(new LTask("制作/拥有1个火把", 0, 8, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "向导:看来你已经成功掌握了生存的经验了!"));
                lconfig.等级任务.Add(2, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("食用苹果", 2, 4009, 1, 1,"提示:当你饥饿时可以通过吃食物解决饥饿!"));
                ltask.Add(new LTask("击杀蓝史莱姆", 1, 1, 1, 1));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "向导:干的漂亮，看看我们在能得到些什么!"));
                ltask.Add(new LTask("穿着脚镣", 3, 216, 1, 1));
                ltask.Add(new LTask("搜集1个晶状体", 4, 38, 1, 1));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "向导:非常完美，让我们再找些夜晚的落星!"));
                ltask.Add(new LTask("拥有5颗坠落之星", 0, 75, 5, 1, "提示:这些落星可以合成魔力水晶!"));
                ltask.Add(new LTask("使用魔力水晶", 2, 109, 1, 2, "提示:魔力水晶可以永久提升你的魔力上限!"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "向导:你现在看起来充满了魔力!"));
                lconfig.等级任务.Add(3, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("拥有15枚银币", 0, 72, 15, 1));
                ltask.Add(new LTask("与商人交谈", 5, 17, 1, 2, "{name}:你好商人，我是{name}\n商人:如果需要买卖欢迎来找我!"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "{name}:镇子里来了商人\n向导: 我们要更加强大了!看看我们能得到些什么？"));
                ltask.Add(new LTask("拥有15个绳", 0, 965, 15, 2));
                ltask.Add(new LTask("拥有3个蘑菇", 0, 5, 3, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:看来你已经准备好探索地下了，去寻找一个宝箱吧！"));
                ltask.Add(new LTask("搜集1个宝箱", 4, 48, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:你的实力真的非同一般呀!"));
                ltask.Add(new LTask("拥有1个炸弹", 0, 166, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:居然还找到了炸弹，简直不敢想象...\n向导:不过受服务器强大的力量限制..."));
                ltask.Add(new LTask("拥有1个凝胶", 0, 23, 1, 2));
                ltask.Add(new LTask("拥有1个粘性炸弹", 0, 235, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "{name}:是这样吗？\n向导: 哇你居然找到了解决的方法！我们要所向睥睨了！"));
                lconfig.等级任务.Add(4, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("与爆破专家交谈", 5, 38, 1, 2 , "{name}:你好，我是{name}\n爆破专家:啊啊啊！我要炸毁一切!"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "{name}:镇子里来了爆破专家\n向导: 我们去问问商人认识吗？"));
                ltask.Add(new LTask("与商人交谈", 5, 17, 1, 2, "{name}:镇子里来了爆破专家\n商人: 他虽然叫爆破专家但是不会到处破坏的！"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导: 看来是朋友不是敌人！"));
                ltask.Add(new LTask("与爆破专家交谈", 5, 38, 1, 2, "{name}:欢迎你爆破专家，恶意破坏世界会被封永久的\n爆破专家:了解了谢谢，我是不会到处破坏的!"));
                ltask.Add(new LTask("拥有3个手榴弹", 0, 168, 3, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2,"{name}:接下来我们要干什么？\n向导:我需要个玻璃瓶,而第一步我们需要造个熔炉"));
                ltask.Add(new LTask("拥有30个石块", 0, 3, 30, 2));
                ltask.Add(new LTask("拥有1个熔炉", 0, 33, 1, 2));
                ltask.Add(new LTask("放置熔炉", 2, 33, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:现在我们要去浩瀚的沙漠"));
                ltask.Add(new LTask("击杀秃鹰", 1, 61, 1, 2));
                ltask.Add(new LTask("搜集1个仙人掌", 4, 276, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:呃，不是仙人掌是需要些沙子"));
                ltask.Add(new LTask("拥有5个沙块", 0, 169, 5, 2));
                ltask.Add(new LTask("制作/拥有1个玻璃", 0, 170, 1, 2));
                ltask.Add(new LTask("制作/搜集1个玻璃瓶", 4, 31, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:哇，这完美的瓶子，简直是件艺术品！谢谢你！"));
                lconfig.等级任务.Add(5, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("拥有生命水晶", 0, 29, 1, 1));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "{name}:我找到了生命水晶，这是什么？\n向导: 吃下它你可以变的更强大！"));
                ltask.Add(new LTask("使用生命水晶", 2, 29, 1, 1));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "向导:是不是变的更强大了，现在你可以去试试砍树，看看有没有变的轻松！"));
                ltask.Add(new LTask("拥有100个木材", 0, 9, 100, 1));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "向导:要记得边砍树边种树哦~"));
                ltask.Add(new LTask("种下3颗橡实", 2, 27, 3, 1));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "向导:好像又有人来了，去看看是什么目的"));
                ltask.Add(new LTask("与护士交谈", 5, 18, 1, 1, "{name}:你好，我是{name}\n护士:你好，我是护士,可以治疗你的疾病!"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "{name}:镇子里来了护士\n向导: 太棒了，这下我们不怕生病了！看看有什么需要帮忙的吗？"));
                ltask.Add(new LTask("与护士交谈", 5, 18, 1, 1, "护士:需要个床来招待病人"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "{name}:护士需要个床要怎么做\n向导: 首先我们需要个锯木机"));
                ltask.Add(new LTask("拥有链条", 0, 85, 1, 2));
                ltask.Add(new LTask("拥有锯木机", 0, 363, 1, 2));
                ltask.Add(new LTask("放置锯木机", 2, 363, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:然后我们需要造个织布机"));
                ltask.Add(new LTask("拥有织布机", 0, 332, 1, 2));
                ltask.Add(new LTask("放置织布机", 2, 332, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:接下来我们需要蛛网制作丝绸"));
                ltask.Add(new LTask("拥有50个蛛网", 0, 150, 50, 2));
                ltask.Add(new LTask("拥有5个丝绸", 0, 225, 5, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:现在我们可以在锯木机造床了"));
                ltask.Add(new LTask("拥有床", 0, 224, 1, 2));
                ltask.Add(new LTask("放置床", 2, 224, 1, 2));
                ltask.Add(new LTask("与护士交谈", 5, 18, 1, 5, "护士:这下我们可以招待病人了，谢谢你~"));
                lconfig.等级任务.Add(6, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("搜集1本书", 4, 149, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "{name}:我找到一本神秘的书？\n向导: 里面写了写什么的内容我们找别人问问"));
                ltask.Add(new LTask("与爆破专家交谈", 5, 38, 1, 2, "{name}:你知道这上面写的什么吗？\n爆破专家: 我想商人应该知道"));
                ltask.Add(new LTask("与商人交谈", 5, 17, 1, 2, "{name}:你知道这上面写的什么吗？\n商人: 当然知道，不过我有个条件，我要黑曜石玫瑰"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "{name}:商人知道，但是他要黑曜石玫瑰\n向导: 黑曜石玫瑰非常难得，他在地狱，但你还没准备好"));
                ltask.Add(new LTask("拥有10个冰雪块", 0, 664, 10, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2,"{name}:这些冰块如何\n向导: 呃，地狱的温度不是冰块可以解除的"));
                ltask.Add(new LTask("搜集1个灰烬块", 4, 172, 1, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "{name}:看看这些灰烬块\n向导: 什么，你居然到达地狱了，不敢相信，看来你毅力真是坚强呀，现在我告诉你技巧"));
                ltask.Add(new LTask("拥有3个空桶", 0, 205, 3, 2));
                ltask.Add(new LTask("拥有3个水桶", 0, 206, 3, 2));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "{name}:为什么要准备水桶\n向导: 把它浇到岩浆里就可以凝结出黑曜石,然后用黑曜石制成头盔"));
                ltask.Add(new LTask("搜集1个黑曜石", 4, 173, 1, 2));
                ltask.Add(new LTask("制作/拥有1个黑曜石骷髅头", 0, 193, 1, 2));
                ltask.Add(new LTask("穿戴黑曜石骷髅头", 3, 193, 1, 3));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:黑曜石头颅可以让你免疫炽热的火焰，现在去找黑曜石玫瑰吧，它在火焰小鬼身上"));
                ltask.Add(new LTask("击杀3个火焰小鬼", 1, 24, 3, 2));
                ltask.Add(new LTask("拥有1个黑曜石玫瑰", 0, 1323, 1, 10));
                ltask.Add(new LTask("与商人交谈", 5, 17, 1, 2, "{name}:给，黑曜石玫瑰\n商人:什么？居然拿到了，太感谢了，可以帮我送给护士吗？"));
                ltask.Add(new LTask("与护士交谈", 5, 18, 1, 2, "{name}:有人让我把这个送给你\n护士:好漂亮花呀，是谁送的？\n{name}:商人\n护士:丢了它{name}:？？？"));
                ltask.Add(new LTask("与商人交谈", 5, 17, 1, 2, "{name}:她让我丢了它，看来你需要亲自去送了\n商人:我一定会让她心服口服"));
                ltask.Add(new LTask("与护士交谈", 5, 18, 1, 2, "商人:这个花送你\n护士一把扔掉花吼道:滚\n商人:555"));
                ltask.Add(new LTask("与商人交谈", 5, 17, 1, 2, "商人:555"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "{name}:看来我们要安慰下他"));
                ltask.Add(new LTask("与商人交谈", 5, 17, 1, 2, "向导:别伤心了\n商人:555"));
                lconfig.等级任务.Add(7, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:听说你们发现了个渔夫？"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 2, "{name}:你好，我是{name}\n渔夫:你好，我是渔夫，感谢你们救了我!虽然我只是打个盹"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:看来没有什么毛病，可以向渔夫学习钓鱼的技能"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 2,"渔夫:如何钓鱼？首先你需要准备鱼饵，去问问商人看看有没有虫网"));
                ltask.Add(new LTask("与商人交谈", 5, 17, 1, 2, "商人:虫网？我这里恰好有这些东西，只要你有足够的钱"));
                ltask.Add(new LTask("拥有虫网", 0, 1991, 1, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:现在你需要鱼饵，击倒草，就有机会蹦出蚱蜢，你需要用虫网捕获它"));
                ltask.Add(new LTask("使用虫网", 2, 1991, 1, 3));
                ltask.Add(new LTask("拥有3个蚱蜢", 0, 2740, 3, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:你还可以破坏草地上的石堆找到蠕虫，再捕获一点蠕虫"));
                ltask.Add(new LTask("使用虫网", 2, 1991, 1, 3));
                ltask.Add(new LTask("拥有3个蠕虫", 0, 2002, 3, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:现在你需要鱼竿，准备一些金属锭制作强化鱼竿"));
                ltask.Add(new LTask("拥有强化钓竿", 0, 2291, 1, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 2, "渔夫:现在你可以前往一个大的水塘钓鱼了，试着钓一些鲈鱼"));
                ltask.Add(new LTask("使用强化钓竿", 2, 2291, 1, 3));
                ltask.Add(new LTask("搜集1个鲈鱼", 4, 2290, 1, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:不错，看你已经掌握了钓鱼的技巧了\n渔夫:钓鱼除了有鱼还可以有箱子，试着去搜集些箱子"));
                ltask.Add(new LTask("使用强化钓竿", 2, 2291, 1, 3));
                ltask.Add(new LTask("拥有3个木匣", 0, 2334, 3, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 2, "渔夫:很棒，在我这里每天都有特定任务，如果你完成它就会得到奖励\n渔夫:试着完成5个我的任务，你会得到坐骑绒毛胡萝卜"));
                ltask.Add(new LTask("拥有绒毛胡萝卜", 0, 2428, 1, 15));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3));
                lconfig.等级任务.Add(8, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "向导:食物不多了？也许渔夫可以帮我们"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 1, "渔夫:关于制作美味的鱼呀？\n渔夫:我们从最简单生鱼片开始做起吧，先准备一些鱼到工作台"));
                ltask.Add(new LTask("拥有5个鳟鱼", 0, 2297, 5, 3));
                ltask.Add(new LTask("拥有5个生鱼片", 0, 2427, 5, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:没想到你做的这么好，快尝尝"));
                ltask.Add(new LTask("使用生鱼片", 2, 2427, 1, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:还真是美味呀，接下来就开始准备吃熟的了\n渔夫:我们需要口锅，不知哪里能得到"));
                ltask.Add(new LTask("与商人交谈", 5, 17, 1, 3, "商人:锅？我可没有那玩意，你大概可以问问向导"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "向导:锅呀，你大概需要些金属锭和木材来制作"));
                ltask.Add(new LTask("拥有烹饪锅", 0, 345, 1, 3));
                ltask.Add(new LTask("放置烹饪锅", 2, 345, 1, 3));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "向导:太棒了，看来要远离饥荒了"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:要开始烹饪了，先准备点食材"));
                ltask.Add(new LTask("拥有7个鲈鱼", 0, 2290, 7, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:然后放到锅里"));
                ltask.Add(new LTask("拥有7个熟鱼", 0, 2425, 7, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:没想到你做的这么好，快尝尝"));
                ltask.Add(new LTask("使用熟鱼", 2, 2425, 1, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:还真是美味呀，还想吃什么？\n渔夫:虾？可以的，只需要准备..."));
                ltask.Add(new LTask("拥有3个虾", 0, 2316, 3, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:然后放到锅里"));
                ltask.Add(new LTask("拥有3个熟虾", 0, 2426, 3, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:没想到你做的这么好，快尝尝"));
                ltask.Add(new LTask("使用熟虾", 2, 2426, 1, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:真是美味呀，如果能有一杯柠檬汁就更好了\n渔夫:柠檬怎么得？当然是砍树了"));
                ltask.Add(new LTask("拥有3个玻璃瓶", 0, 31, 3, 3));
                ltask.Add(new LTask("拥有3个柠檬", 0, 4291, 3, 3));
                ltask.Add(new LTask("拥有3杯柠檬汁", 0, 4616, 3, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:真是一种享受啊~"));
                ltask.Add(new LTask("使用柠檬汁", 2, 4616, 1, 3));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "向导:愉快的生活开始了~"));
                lconfig.等级任务.Add(9, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("与军火商交谈", 5, 19, 1, 3, "{name}:你好，我是{name}\n军火商:嘘，我这里有些大家伙"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "{name}:镇子里来了神秘人\n向导: 去问问有人认识吗？"));
                ltask.Add(new LTask("与商人交谈", 5, 17, 1, 3, "商人: 同行"));
                ltask.Add(new LTask("与爆破专家交谈", 5, 38, 1, 3, "爆破专家:同行"));
                ltask.Add(new LTask("与护士交谈", 5, 18, 1, 3, "护士:军火商？"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "{name}:是一个军火商\n向导:看来时代要大变了"));
                ltask.Add(new LTask("与发型师交谈", 5, 353, 1, 3, "{name}:你好，我是{name}\n发型师:谢谢你救了我"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "{name}:镇子里来了发型师\n向导: 我们的人口丰富了起来"));
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 3, "{name}:你好，我是{name}\n酒馆老板:谢谢你救了我"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "{name}:镇子里来了酒馆老板\n向导: 我们的人口丰富了起来"));
                ltask.Add(new LTask("与高尔夫球手交谈", 5, 588, 1, 3, "{name}:你好，我是{name}\n高尔夫球手:谢谢你救了我"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "{name}:镇子里来了高尔夫球手\n向导: 我们多了一种娱乐设施了"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:快快找高尔夫手玩一局，我已经迫不及待了"));
                ltask.Add(new LTask("与高尔夫球手交谈", 5, 588, 1, 3, "{name}:如何玩高尔夫？\n高尔夫球手:这个我最在行了，让我们开始吧\n高尔夫球手:首先需要准备个球座和球洞，当然我这里就有卖"));
                ltask.Add(new LTask("拥有1个高尔夫球座", 0, 4089, 1, 3));
                ltask.Add(new LTask("拥有1个高尔夫球洞", 0, 4040, 1, 3));
                ltask.Add(new LTask("与高尔夫球手交谈", 5, 588, 1, 3, "\n高尔夫球手:然后再准备一个红三角旗，依然可以在我这里得到"));
                ltask.Add(new LTask("拥有1个红三角旗", 0, 4084, 1, 3));
                ltask.Add(new LTask("与高尔夫球手交谈", 5, 588, 1, 3, "\n高尔夫球手:好了现在可以把他们放到地上，红三角旗可以插在球洞上来提示目标"));
                ltask.Add(new LTask("使用高尔夫球座", 2, 4089, 1, 3));
                ltask.Add(new LTask("使用高尔夫球洞", 2, 4040, 1, 3));
                ltask.Add(new LTask("使用红三角旗", 2, 4084, 1, 3));
                ltask.Add(new LTask("与高尔夫球手交谈", 5, 588, 1, 3, "\n高尔夫球手:现在你需要一个球杆"));
                ltask.Add(new LTask("拥有1个高尔夫球杆铁杆", 0, 4587, 1, 3));
                ltask.Add(new LTask("与高尔夫球手交谈", 5, 588, 1, 3, "\n高尔夫球手:铁杆适合中短距离射击，试试"));
                ltask.Add(new LTask("使用高尔夫球杆铁杆", 2, 4587, 1, 3));
                ltask.Add(new LTask("与高尔夫球手交谈", 5, 588, 1, 3, "\n高尔夫球手:在高尔夫球座上挥杆即可打出球"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:看看是时候切磋切磋了"));
                ltask.Add(new LTask("与高尔夫球手交谈", 5, 588, 1, 3, "\n高尔夫球手:在高尔夫球座上挥杆即可打出球"));
                ltask.Add(new LTask("拥有铜高尔夫纪念章", 0, 4599, 1, 5));
                ltask.Add(new LTask("与高尔夫球手交谈", 5, 588, 1, 15, "\n高尔夫球手:一位强大的高尔夫手诞生了"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 5, "渔夫:漂亮，就知道你行"));
                ltask.Add(new LTask("与高尔夫球手交谈", 5, 588, 1, 10, "\n高尔夫球手:随时可以来玩"));
                lconfig.等级任务.Add(10, ltask);




                ltask = new List<LTask>();
                ltask.Add(new LTask("与染料商交谈", 5, 207, 1, 3, "{name}:你好，我是{name}\n染料商:你有什么需要染的吗？"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "{name}:镇子里来了染料商\n向导: 有一株植物困惑着我正好可以问问他"));
                ltask.Add(new LTask("拥有1个黄万寿菊", 0, 1110, 1, 3));
                ltask.Add(new LTask("与染料商交谈", 5, 207, 1, 3, "染料商:这个嘛，可以制作成染料，但你需要个染缸，恰好我这里有（邪恶的微笑）"));
                ltask.Add(new LTask("拥有1个染缸", 0, 1120, 1, 3));
                ltask.Add(new LTask("摆放染缸", 2, 1120, 1, 3));
                ltask.Add(new LTask("拥有1个黄染料", 0, 1009, 1, 3));
                ltask.Add(new LTask("与染料商交谈", 5, 207, 1, 3, "染料商:看来很有天赋嘛"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导: 这么漂亮的染料呀，快问问有没有绿的"));
                ltask.Add(new LTask("与染料商交谈", 5, 207, 1, 3, "染料商:绿染料？那个简单，你只需要绿蘑菇，它生长在洞穴和地下层中"));
                ltask.Add(new LTask("拥有1个绿蘑菇", 0, 1108, 1, 3));
                ltask.Add(new LTask("拥有1个绿染料", 0, 1011, 1, 3));
                ltask.Add(new LTask("与染料商交谈", 5, 207, 1, 3, "染料商:看来很有天赋嘛"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导: 太棒，我喜欢这个"));
                ltask.Add(new LTask("与油漆工交谈", 5, 227, 1, 3, "{name}:你好，我是{name}\n油漆工:我是一个粉刷匠"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "{name}:镇子里来了粉刷匠\n向导: 哦？看看他有什么本事"));
                ltask.Add(new LTask("与油漆工交谈", 5, 227, 1, 3, "油漆工:实际上我还是个画家"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "向导: 画家？带我去看看"));
                ltask.Add(new LTask("与油漆工交谈", 5, 227, 1, 3, "向导:哇真实漂亮呀！"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 3, "向导:那幅叫黎明的画真的很棒，如果你能买下它"));
                ltask.Add(new LTask("拥有1幅黎明", 0, 1490, 1, 15));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 15, "向导:哇是要送给我的吗？谢谢~"));
                lconfig.等级任务.Add(11, ltask);





                ltask = new List<LTask>();
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 1, "{name}:你好，我是{name}\n树妖:大自然的力量与你同在"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "{name}:镇子里来了树妖\n向导:就是那个传说中的树妖？带我去看看"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 1, "树妖:这个世界有许多需要做的"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 1, "向导:我们又多了一位强大的伙伴"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 1, "{name}:你叫我吗？\n渔夫:是的，听说树妖来了？我知道她有南瓜种子\n渔夫:吃了那么多海味我想换换胃口，不知道能不能帮我"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 1, "树妖:南瓜种子？我有好多的"));
                ltask.Add(new LTask("拥有10个南瓜子", 0, 1828, 10, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 5, "渔夫:把南瓜种子种到地下，然后静静等待"));
                ltask.Add(new LTask("种植10个南瓜子", 2, 1828, 10, 3));
                ltask.Add(new LTask("拥有30个南瓜", 0, 1725, 30, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 3, "渔夫:终于收获了，我都等不及了，现在把南瓜放到炉子里..."));
                ltask.Add(new LTask("拥有3个南瓜饼", 0, 1787, 3, 3));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 5, "渔夫:哇这真实美味呀"));
                ltask.Add(new LTask("使用南瓜饼", 2, 1787, 1, 5));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 5, "渔夫:知道嘛，如果南瓜饼配上蜂蜜的话是最绝的\n渔夫:你可以用玻璃瓶在蜂蜜旁边灌上一瓶或者..."));
                ltask.Add(new LTask("拥有3个蜂蜜瓶", 0, 1134, 3, 5));
                ltask.Add(new LTask("使用蜂蜜瓶", 2, 1134, 1, 5));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 5, "渔夫:是不是绝配吧！"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:我感受到血腥的力量，莫非血月要来了？击败他们"));
                ltask.Add(new LTask("击杀10个滴滴怪", 1, 490, 10, 5));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:太强大了，如果你留意你会发钱币槽"));
                ltask.Add(new LTask("拥有钱币槽", 0, 3213, 1, 5));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:这是一个随身仓库，使用它就可以保护你的物品"));
                ltask.Add(new LTask("使用钱币槽", 2, 3213, 1, 5));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:你更加强大了"));
                ltask.Add(new LTask("与派对女孩交谈", 5, 208, 1, 5, "{name}:你好，我是{name}\n派对女孩:我有股想开派对的冲动"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:派对？当然喜欢，让我们准备吧~"));
                ltask.Add(new LTask("与派对女孩交谈", 5, 208, 1, 5, "派对女孩:彩纸枪？当然有"));
                ltask.Add(new LTask("拥有3个彩纸枪", 0, 1000, 3, 5));
                ltask.Add(new LTask("使用彩纸枪", 2, 1000, 3, 5));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 10, "向导:愉快的派对呀！"));
                lconfig.等级任务.Add(12, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("与动物学家交谈", 5, 633, 1, 2, "{name}:你好，我是{name}\n动物学家:你好~"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "{name}:镇子里来了动物学家\n向导:动物学家？"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 2, "树妖:一位热爱自然的伙伴"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:我们更加强大了！"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 2, "{name}:你好，我是{name}\n巫医:大自然的律动！"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "{name}:镇子里又来了个自然界的使者\n向导:啥？"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 2, "树妖:一位自然的法师"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:看看他能干啥？"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 2, "巫医:巫术与魔法，还有药剂\n{name}:可否瞬间移动？\n巫医:我有一种药剂，可以让你瞬间移动\n{name}:如何？\n巫医:你需要准备个炼金桌"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 2, "向导:炼金桌？你将玻璃瓶放到桌子上就可以了\n{name}:这么随意？向导:还要如何？"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 2, "{name}:然后呢？\n巫医:你需要准备些素材"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 2, "渔夫:镜面鱼？我大概在森林或雪原生物群落的地下洞穴见过"));
                ltask.Add(new LTask("拥有1个镜面鱼", 0, 2309, 1, 2));
                ltask.Add(new LTask("拥有1个闪耀根", 0, 315, 1, 2));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 2, "{name}:然后呢？\n巫医:你需要用一瓶水溶解他们就可以了\n巫医:水瓶？你可以直接在水边用玻璃瓶取"));
                ltask.Add(new LTask("拥有1个水瓶", 0, 126, 1, 2));
                ltask.Add(new LTask("拥有1个虫洞药水", 0, 2997, 1, 5));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 5, "巫医:啥？这么快就成功了？厉害呀\n巫医:怎么用？在地图上点击同一个队伍的队友图标就能传送过去了"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 5, "渔夫:你既然学会了制药，那可否帮我做一个？也当是报答我刚刚告诉你镜面鱼的回报\n{name}:呃，好吧"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 5, "{name}:可否教我制作钓鱼药水？\n巫医:这个嘛很简单但是关键的松脆蜂蜜块很难得，建议问问向导"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:松脆蜂蜜块？这个嘛首先你需要让水和蜂蜜相遇"));
                ltask.Add(new LTask("拥有3个水桶", 0, 206, 3, 5));
                ltask.Add(new LTask("拥有3个蜂蜜桶", 0, 1128, 3, 5));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:然后把水倒进蜂蜜里"));
                ltask.Add(new LTask("使用水桶", 2, 206, 3, 5));
                ltask.Add(new LTask("使用蜂蜜桶", 2, 1128, 3, 5));
                ltask.Add(new LTask("拥有3个蜂蜜块", 0, 1125, 3, 5));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:然后把蜂蜜块放进熔炉烤"));
                ltask.Add(new LTask("拥有3个松脆蜂蜜块", 0, 1127, 3, 5));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:非常完美"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 5, "{name}:我拿到松脆蜂蜜块了\n巫医:很强大，那么我们继续"));
                ltask.Add(new LTask("拥有3个幌菊", 0, 317, 3, 5));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 5, "巫医:让我们重复刚刚制作虫洞药水的步骤"));
                ltask.Add(new LTask("拥有3个水瓶", 0, 126, 3, 5));
                ltask.Add(new LTask("拥有1个钓鱼药水", 0, 2354, 1, 5));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 5, "巫医:太完美了！"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 15, "渔夫:哈哈，谢谢你，特别喜欢这个药水"));
                lconfig.等级任务.Add(13, ltask);


                ltask = new List<LTask>();
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 5, "酒馆老板:要喝酒吗？我这里有上好的麦芽酒"));
                ltask.Add(new LTask("使用3杯麦芽酒", 2, 353, 3, 5));
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 5, "{name}:上好的麦芽酒，来干杯！\n酒馆老板:在我的家乡，这只是饮料...咯..."));
                ltask.Add(new LTask("使用3杯麦芽酒", 2, 353, 3, 5));
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 5, "{name}:再来，来干杯！\n酒馆老板:他们说你很强壮，我可知道什么是真正的强壮。让我们来看看你是否名副其实...咯..."));
                ltask.Add(new LTask("使用3杯麦芽酒", 2, 353, 3, 5));
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 5, "{name}:再来，来干杯！\n酒馆老板:曾经...埃特尼亚水晶...撒旦军队...老将军..."));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:埃特尼亚水晶?撒旦军队?"));
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 5, "酒馆老板:埃特尼亚水晶?撒旦军队?你喝多了吧"));
                ltask.Add(new LTask("与爆破专家交谈", 5, 38, 1, 5, "爆破专家:埃特尼亚水晶?撒旦军队?那是老将军..."));
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 5, "酒馆老板:看来你已经知道了，那是个强大的军团，如果你想要..."));
                ltask.Add(new LTask("拥有5个护卫奖章", 0, 3817, 5, 5));
                ltask.Add(new LTask("击杀10个天国哥布林", 1, 552, 10, 5));
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 15, "酒馆老板:看来你也是强壮的英雄...来，干杯"));
                ltask.Add(new LTask("拥有1个破布", 0, 362, 1, 5));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:你发现了破布？看来哥布林要大举进攻了"));
                ltask.Add(new LTask("击杀10个哥布林战士", 1, 28, 10, 15));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 15, "向导:伟大的英雄你成功抵抗了哥布林"));
                lconfig.等级任务.Add(14, ltask);





                ltask = new List<LTask>();
                ltask.Add(new LTask("与哥布林工匠交谈", 5, 107, 1, 5, "{name}:你好，我是{name}\n哥布林工匠:谢谢你救了我"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:什么你居然救了哥布林？你忘记他们的入侵了吗？"));
                ltask.Add(new LTask("与染料商交谈", 5, 207, 1, 5, "染料商:哥布林工匠？那是一个老朋友了，感谢你救了他"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:并不是那种哥布林呀，抱歉"));
                ltask.Add(new LTask("与染料商交谈", 5, 207, 1, 5, "染料商:他掌握核心科技，是一个工程师"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:那他能飞吗？"));
                ltask.Add(new LTask("与染料商交谈", 5, 207, 1, 5, "染料商:他设计的火箭靴可以让你一飞冲天"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:那一定是个强大的装备"));
                ltask.Add(new LTask("与哥布林工匠交谈", 5, 107, 1, 5, "哥布林工匠:火箭靴？当然我很乐意，只需要成本价钱..."));
                ltask.Add(new LTask("拥有火箭靴", 0, 128, 1, 5));
                ltask.Add(new LTask("穿着火箭靴", 3, 128, 1, 5));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:飞一般的感觉？"));
                ltask.Add(new LTask("与哥布林工匠交谈", 5, 107, 1, 5, "哥布林工匠:穿起来如何？当然我还可以让它更快，只需要用的工匠作坊..."));
                ltask.Add(new LTask("拥有幽灵靴", 0, 405, 1, 5));
                ltask.Add(new LTask("穿着幽灵靴", 3, 405, 1, 5));
                ltask.Add(new LTask("与哥布林工匠交谈", 5, 107, 1, 5, "哥布林工匠:你还想更快？就像闪电一样？只需要...\n{name}:呃...够了..."));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 5, "向导:你的鞋很炫嘛"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 5, "渔夫:鞋子真棒，我知道一种东西可以强化它，你可以钓鱼...\n{name}:呃...你也...好吧..."));
                ltask.Add(new LTask("拥有蛙腿", 0, 2423, 1, 5));
                ltask.Add(new LTask("穿着蛙腿", 3, 2423, 1, 5));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 5, "渔夫:是不是很配？\n{name}:呃...还会..."));
                ltask.Add(new LTask("与哥布林工匠交谈", 5, 107, 1, 5, "哥布林工匠:我在逃亡的过程遗失了一些装置，如果你找到就归你了"));
                ltask.Add(new LTask("拥有深度计", 0, 18, 1, 10));
                ltask.Add(new LTask("拥有罗盘", 0, 393, 1, 10));
                ltask.Add(new LTask("拥有全球定位系统", 0, 395, 1, 15));
                ltask.Add(new LTask("拥有哥布林数据仪", 0, 3121, 1, 15));
                ltask.Add(new LTask("与哥布林工匠交谈", 5, 107, 1, 10, "哥布林工匠:还有更强大的科技，如果你有耐心会发现的"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 15, "向导:真是令人叹服的高科技呀"));
                ltask.Add(new LTask("拥有史莱姆王冠", 0, 560, 1, 10));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 15, "向导:史莱姆之王？一个强大的首领！"));
                ltask.Add(new LTask("拥有皇家凝胶", 0, 3090, 1, 5));
                ltask.Add(new LTask("穿着皇家凝胶", 3, 3090, 1, 15));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 30, "向导:哇，史莱姆之王的象征？"));
                lconfig.等级任务.Add(15, ltask);





                ltask = new List<LTask>();
                ltask.Add(new LTask("使用神锤", 2, 367, 1, 21,"旁边:你使用了蕴含古老力量的锤子！"));
                ltask.Add(new LTask("与服装商交谈", 5, 54, 1, 21, "{name}:我的力量已不可匹敌了！\n服装商:啊..还没有,你..我快要暴走了..你要为向导的死负责..\n{name}:你是无法阻止这一切的去死吧！"));
                ltask.Add(new LTask("装备服装商巫毒娃娃", 3, 1307, 1, 21));
                ltask.Add(new LTask("拥有骷髅王宝藏袋", 0, 3323, 1, 21));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 21, "{name}:听说你又活了？\n向导:你个*泰拉脏话*，你知道你在干什么吗，裁缝你别拦着我我今天就要把他弄死......\n向导:什么裁缝也被你杀了？\n{name}:我是无敌的！\n向导:无敌？恐怕你摊上麻烦了，还记得你曾经干掉的史莱姆王吗？"));
                ltask.Add(new LTask("死3次", 6, 0, 3, 21));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 21, "向导:哈哈哈哈，看着你狼狈的样子真爽了，算了，之前的事就已经不重要了，你未来仍然有很多危机..."));
                ltask.Add(new LTask("死3次", 6, 0, 3, 21));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 21, "向导:哈哈哈哈，怎么样？还无敌吗？...\n向导:唉，唉，但是我还是那个好向导，还是会帮你变强的~"));
                ltask.Add(new LTask("与服装商交谈", 5, 54, 1, 21, "{name}:...\n服装商:哼~"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 21, "{name}:服装商不理我了怎么办？\n向导:你得给他带点他喜欢的货嘛，听说他想吃蘑菇什么的"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 21, "{name}:什么嘛，呦吼，巫医你也在这里呀\n巫医:蘑菇？很好吃的那种吗？\n巫医:我曾经见过一种会动的蘑菇，一定好吃"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 21, "巫医:我可不是那么随便就开口的，你必须找3个憎恶之蜂"));
                ltask.Add(new LTask("拥有3个憎恶蜂", 0, 1133, 3, 21));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 21, "巫医:实力可以嘛，那种好吃的蘑菇在一片蓝色的地方\n巫医:但是不在地下，你可以需要造个房子引过来"));
                ltask.Add(new LTask("与松露人交谈", 5, 160, 1, 21, "{name}:好吃的~\n松露人:你不要过来啊~~~\n{name}:居然能说话？哼，原来如此，看我怎么收拾他们"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 21, "巫医:啊不要怪我，我不是故意的"));
                ltask.Add(new LTask("与服装商交谈", 5, 54, 1, 21, "{name}:哼，居然要吃那么可爱的蘑菇人！\n服装商:啊什么？你找到了？让我尝尝..\n{name}:让你见识我不可匹敌的力量，看招\n服装商:啊，不要，啊，我以后不敢了，啊"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 21, "向导:怎么？\n{name}:哼\n向导:...抱歉..这怪我，我这就告诉你变强的途径..."));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 21, "向导:相传血肉墙曾经封印了那些远古的魂灵，其中便有象征天空与飞翔的魂灵——飞翔之魂。\n向导:“如今肉山已经被击败，远古的魂灵也被释放，那些飞翔之魂应该也回归天空。\n向导: 去空岛看看吧，说不定会有意想不到的收获。"));
                ltask.Add(new LTask("拥有20个飞翔之魂", 0, 575, 20, 21));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 21, "向导:这些飞翔之魂融入某些物品中时即可让你拥有飞翔的力量，如果你找到的话"));
                lconfig.等级任务.Add(18, ltask);



                ltask = new List<LTask>();
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 15, "{name}:你好，我是{name}\n巫师:谢谢你救了我，乐意为你效劳"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 15, "向导:巫师？去看看他有什么好东西"));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 15, "巫师:我有一本魔法书，记载万物，只需一些金币"));
                ltask.Add(new LTask("拥有魔法书", 0, 531, 1, 15, "巫师:豪爽，我喜欢"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 21, "向导:一本魔法书？是看不懂的文字"));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 21, "巫师:实际上我也看不懂这书"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 21, "向导:看来去问问巫医吧，都是玩魔法的"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 21, "巫医:也许需要点特殊装备，比如狼人血"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 21, "向导:狼人？那可是强大的，兴许军火商有主意"));
                ltask.Add(new LTask("与军火商交谈", 5, 19, 1, 21, "军火商:来的正巧，过来，给你看个宝贝！"));
                ltask.Add(new LTask("拥有霰弹枪", 0, 534, 1, 30));
                ltask.Add(new LTask("与军火商交谈", 5, 19, 1, 30, "军火商:这是一把霰弹枪，我好不容易才搞到，另外你需要些子弹\n军火商:什么？狼人？那你你或许需要银子弹，没错就像电影那样狼人害怕银子弹，买一些！"));
                ltask.Add(new LTask("拥有100个银子弹", 0, 278, 100, 30));
                ltask.Add(new LTask("与军火商交谈", 5, 19, 1, 30, "军火商:好极了现在去寻找你的目标射爆它的头！"));
                ltask.Add(new LTask("使用霰弹枪", 2, 534, 1, 30));
                ltask.Add(new LTask("与军火商交谈", 5, 19, 1, 30, "军火商:不要乱开枪！不然我会让护士在你的身体里塞一个针管"));
                ltask.Add(new LTask("击杀一只狼人", 1, 104, 1, 30));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 30, "{name}:给你\n巫医:真是太强大了，我伟大的药剂就要出炉了\n{name}:？？书上写的什么？？\n巫医:什么书？\n{name}:魔法书\n巫医:什么书法？\n{name}:昨天让你看的魔法书\n巫医:魔什么术？\n{name}:你个*泰拉脏话*，我砍死你\n巫医:啊啊啊，大侠饶命啊，树妖一定知道"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 50, "树妖:原来如此，肉山被击败后巨大的力量改变世界的面貌...\n树妖:有许多新生矿石诞生于世界，另外，可要小心...咦？人呢？"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 50, "向导:了解了我去挖矿去了~"));
                lconfig.等级任务.Add(19, ltask);




                ltask = new List<LTask>();
                ltask.Add(new LTask("拥有15个光明之魂", 0, 520, 15, 15, "{name}:呃...我是怎么把这玩意儿收进背包的..."));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 15, "向导:据我了解能制作成某种钥匙"));
                ltask.Add(new LTask("拥有光明钥匙", 0, 3092, 1, 15));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 15, "{name}:那这玩意有何用？\n向导:呃，也许需要问问树妖"));
                ltask.Add(new LTask("与树妖对话", 5, 20, 1, 15, "树妖:光明钥匙？你怎么拿到它的\n{name}:他们用来打开箱子吗？\n树妖：不，这是远古召唤仪式的关键物品\n树妖：相传把他放到箱子里，神秘的力量将会占据箱子，诞生可怕的生物"));
                ltask.Add(new LTask("击杀神圣宝箱怪", 1, 475, 1, 35));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 35, "树妖:你获得了一把强大的武器"));
                ltask.Add(new LTask("拥有海盗地图", 0, 1315, 1, 35));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 35, "向导:一张神秘的地图，去问问商人"));
                ltask.Add(new LTask("与商人对话", 5, 17, 1, 35, "商人：地图？我看看，目的地是…正是我们这里！ 我觉得大事不妙。。。"));
                ltask.Add(new LTask("击杀3个海盗神射手", 1, 214, 3, 35));
                ltask.Add(new LTask("与商人对话", 5, 17, 1, 35, "商人：唉，虚惊一场"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 35, "向导:刚刚发生了什么？"));
                ltask.Add(new LTask("与海盗对话", 5, 229, 1, 35, "{name}:你丫又来捣乱？\n海盗：我赌你枪里没有子弹\n海盗：哈哈哈哈，不错呀，小子。你居然打败了我的手下。"));
                ltask.Add(new LTask("与商人对话", 5, 17, 1, 35, "{name}:海盗来了\n商人：啊啊啊快跑"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 35, "{name}:海盗来了\n向导:也许是个人才"));
                ltask.Add(new LTask("与海盗对话", 5, 229, 1, 35, "海盗：看看这门大炮，一下就可以轰飞一名入侵者！好吧，没弹药就只能被入侵者轰飞了\n{name}:好吧\n海盗：小伙子去探索下这片辽阔的土地吧，它已经发生了巨大的变化了"));
                lconfig.等级任务.Add(20, ltask);



                ltask = new List<LTask>();
                ltask.Add(new LTask("击杀35个鸟妖", 1, 48, 35, 15));
                ltask.Add(new LTask("击杀15个飞鱼", 1, 224, 15, 15));
                ltask.Add(new LTask("穿着雨衣", 3, 1136, 1, 15));
                ltask.Add(new LTask("击杀3个雨伞史莱姆", 1, 225, 3, 15));
                ltask.Add(new LTask("击杀5个冰雪史莱姆", 1, 147, 5, 15));
                ltask.Add(new LTask("击杀35个流星头", 1, 23, 35, 15));
                ltask.Add(new LTask("击杀10个鬼", 1, 316, 10, 15));
                ltask.Add(new LTask("击杀3个红魔鬼", 1, 156, 3, 15));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 15, "{name}:在地狱我见到一个痛苦亡魂，哀求我救救它\n向导:也许应该问问树妖"));
                ltask.Add(new LTask("与树妖对话", 5, 20, 1, 15, "{name}:我看到地狱有一个痛苦亡魂？\n树妖：我这里有净化粉，拿去试试"));
                ltask.Add(new LTask("拥有10包净化粉", 0, 66, 10, 15));
                ltask.Add(new LTask("使用净化粉", 2, 66, 1, 15));
                ltask.Add(new LTask("与税收官对话", 5, 441, 1, 15, "{name}:你好我是{name}\n税收官：谢谢你救了我，我将为你收税"));
                ltask.Add(new LTask("与旅商交谈", 5, 368, 1, 15, "旅商：我是旅行的商人，这里相当富饶呀，要不要尝尝我这里的清酒"));
                ltask.Add(new LTask("喝清酒", 2, 2266, 1, 15));
                ltask.Add(new LTask("与旅商交谈", 5, 368, 1, 30, "{name}:味道棒极了\n旅商：哈哈，如果有缘以后再见"));
                ltask.Add(new LTask("击杀3个武装史莱姆僵尸", 1, 433, 3, 30));
                ltask.Add(new LTask("拥有僵尸臂", 0, 1304, 1, 30));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 30, "{name}:僵尸臂\n向导:神奇的物件"));
                ltask.Add(new LTask("与骷髅商人交谈", 5, 453, 1, 30, "{name}:哇骷髅会说话？\n骷髅商人:大惊小怪，来给你看看我的宝贝，这些货可是只有我这里有的"));
                ltask.Add(new LTask("与派对女孩交谈", 5, 208, 1, 30, "{name}:好累\n派对女孩:去玩玩这个吧"));
                ltask.Add(new LTask("挥舞泡泡魔棒", 2, 1450, 15, 30));
                ltask.Add(new LTask("与派对女孩交谈", 5, 208, 1, 90, "派对女孩:要主意休息哦~"));
                lconfig.等级任务.Add(21, ltask);





                ltask = new List<LTask>();
                ltask.Add(new LTask("拥有松露虫", 0, 2673, 1, 35));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 35, "{name}:这个可以当鱼饵吗？我刚发现的\n渔夫:啊快扔了他，它会引来怪物的"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 35, "向导:好像大地在震动？"));
                ltask.Add(new LTask("击杀探测怪", 1, 139, 1, 55));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 55, "向导:好大一条钢铁巨虫，没见过的科技呀"));
                ltask.Add(new LTask("与机械师交谈", 5, 124, 1, 55, "机械师:估计要找来我的一位朋友"));
                ltask.Add(new LTask("与蒸汽朋克人交谈", 5, 178, 1, 55, "蒸汽朋克人:这是一种古老的科技"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 55, "向导:要找他的源能量"));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 55, "巫师:注意，某些天体要遮住太阳了"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 55, "向导:日食？曾经殊死奋战的时刻要来了"));
                ltask.Add(new LTask("击杀沼泽怪", 1, 166, 1, 55));
                ltask.Add(new LTask("拥有断裂英雄剑", 0, 1570, 1, 55));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 55, "向导:没想到你就是传说那位英雄"));
                ltask.Add(new LTask("拥有断钢剑", 0, 368, 1, 55));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 55, "向导:你要重铸断剑？"));
                ltask.Add(new LTask("拥有原版断钢剑", 0, 674, 1, 55));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 55, "向导:英雄即将崛起"));
                lconfig.等级任务.Add(25, ltask);






                ltask = new List<LTask>();
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 15, "酒馆老板:英雄？我还有接下来的故事是否要来听听？"));
                ltask.Add(new LTask("使用3杯麦芽酒", 2, 353, 3, 15));
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 15, "酒馆老板:还记得埃特尼亚水晶吗？好像受到机械的影响有所改变"));
                ltask.Add(new LTask("与蒸汽朋克人交谈", 5, 178, 1, 15, "蒸汽朋克人:古老的科技共鸣？"));
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 15, "酒馆老板:那边树妖喊你，水晶的事如果感兴趣可以来找我"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 35, "树妖:你可见过一种丛林花？"));
                ltask.Add(new LTask("搜集大自然的恩赐", 4, 223, 1, 55));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 55, "树妖:感谢你的花，不过不是这种花"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 55, "巫医:莫非是世纪之花？那是一种传说中极其罕见的，可怕的植物"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 55, "树妖:我预感到他们的诞世"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 55, "向导:是好是坏"));
                ltask.Add(new LTask("与酒馆老板交谈", 5, 550, 1, 55, "酒馆老板:将来带危机"));
                ltask.Add(new LTask("与海盗对话", 5, 229, 1, 55, "海盗：小意思而已"));
                ltask.Add(new LTask("拥有世纪之花宝藏袋", 0, 3328, 1, 55));
                ltask.Add(new LTask("与海盗对话", 5, 229, 1, 55, "海盗：我就说嘛"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 55, "向导:不愧是英雄"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 55, "树妖:不妙"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 55, "巫医:也许某些事情必须正面面对而非躲避"));
                ltask.Add(new LTask("与服装商交谈", 5, 54, 1, 55, "服装商:或许，这就是注定"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 55, "向导:英雄，做好迎接的准备"));
                lconfig.等级任务.Add(26, ltask);




                ltask = new List<LTask>();
                ltask.Add(new LTask("与机器侠交谈", 5, 209, 1, 15, "机器侠:你好我是机械侠"));
                ltask.Add(new LTask("与爆破专家交谈", 5, 38, 1, 15, "爆破专家:是位大佬"));
                ltask.Add(new LTask("与机器侠交谈", 5, 209, 1, 15, "机器侠:如果有兴趣记得试试我的高科技"));
                ltask.Add(new LTask("与服装商交谈", 5, 54, 1, 15, "服装商:我感受到可怕的力量越来越集中起来了，他凝聚于地牢"));
                ltask.Add(new LTask("与机器侠交谈", 5, 209, 1, 15, "机器侠:也许终极爆弹可以展示下拳脚"));
                ltask.Add(new LTask("击杀巨型诅咒骷髅头", 1, 289, 1, 30));
                ltask.Add(new LTask("与服装商交谈", 5, 54, 1, 30, "服装商:果然，他们更加强大了"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 30, "向导:英雄，没有什么好惧怕的，战胜他们"));
                ltask.Add(new LTask("击杀骷髅突击手", 1, 293, 1, 30));
                ltask.Add(new LTask("击杀5个地牢幽魂", 1, 288, 5, 30));
                ltask.Add(new LTask("击杀骷髅狙击手", 1, 291, 1, 30));
                ltask.Add(new LTask("击杀蓝装甲骷髅", 1, 273, 1, 30));
                ltask.Add(new LTask("击杀圣骑士", 1, 290, 1, 30));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 30, "向导:圣骑士？他们为什么在地牢？"));
                ltask.Add(new LTask("与服装商交谈", 5, 54, 1, 30, "服装商:那种无法抵抗的力量"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 30, "树妖:神圣的格局被打破"));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 30, "巫师:鄙人夜观天象，隐隐约约看到巨大的南瓜飘过"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 30, "渔夫:你那是想吃南瓜了吧"));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 30, "巫师:你个小屁孩渔夫，那可是危险的征兆"));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 30, "渔夫:天上飘吃的，啥危险？"));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 30, "巫师:准备些材料，想办法把他引下来"));
                ltask.Add(new LTask("拥有30南瓜", 0, 1725, 30, 30));
                ltask.Add(new LTask("拥有5灵气", 0, 1508, 5, 30));
                ltask.Add(new LTask("拥有10神圣锭", 0, 1225, 10, 30));
                ltask.Add(new LTask("拥有南瓜月勋章", 0, 1844, 1, 30));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 30, "巫师:面对敌人吧"));
                ltask.Add(new LTask("击杀地狱犬", 1, 329, 1, 30));
                ltask.Add(new LTask("击杀无头骑士", 1, 315, 1, 30));
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 30, "渔夫:可怕的南瓜"));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 30, "巫师:果然非同一般"));
                ltask.Add(new LTask("拥有30阴森木", 0, 1729, 30, 30));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 30, "巫师:这木材拥有强大的力量"));
                lconfig.等级任务.Add(27, ltask);



                ltask = new List<LTask>();
                ltask.Add(new LTask("与渔夫交谈", 5, 369, 1, 5, "渔夫:月亮上好像有个诡异的笑脸"));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 5, "巫师:...快快准备迎战"));
                ltask.Add(new LTask("击杀姜饼人", 1, 342, 1, 15));
                ltask.Add(new LTask("击杀坎卜斯", 1, 351, 1, 15));
                ltask.Add(new LTask("击杀精灵直升机", 1, 347, 1, 15));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 50, "巫医:这...不正常"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 50, "树妖:罕见的越来越频繁"));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 50, "巫师:近日的事件堪称震撼"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 50, "向导:这个世界将迎来巨大的变化，我看到群星汇聚日边，无穷的力量涌向丛林的地下"));
                ltask.Add(new LTask("与树妖交谈", 5, 20, 1, 50, "树妖:它们开始在丛林疯长，最终将使得古老的力量觉醒"));
                ltask.Add(new LTask("与巫师交谈", 5, 108, 1, 50, "巫师:魔法力量，是古老的动力，他们在丛林，我看到了"));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 50, "巫医:是时候了，去哪个地方启动哪个装置"));
                ltask.Add(new LTask("拥有神庙钥匙", 0, 1141, 1, 50));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 50, "向导:你必须面对哪个无比强大的敌人了，请做好准备"));
                ltask.Add(new LTask("与机械师交谈", 5, 124, 1, 50, "机械师:你将会面对许多陷阱，带上钢丝钳会有很大帮助"));
                ltask.Add(new LTask("拥有钢丝钳", 0, 510, 1, 50));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 50, "向导:开始行动，让我们搜集哪个装置的能量源"));
                ltask.Add(new LTask("拥有丛林蜥蜴电池", 0, 1293, 1, 50));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 50, "巫医:蜥蜴祭坛激活"));
                ltask.Add(new LTask("拥有石巨人宝藏袋", 0, 3329, 1, 50));
                ltask.Add(new LTask("与巫医交谈", 5, 228, 1, 50, "巫医:已经不可阻挡了"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 50, "向导:准备好了吗"));
                ltask.Add(new LTask("与服装商交谈", 5, 54, 1, 50, "服装商:他们要回来了，那可怕的力量，我感受到了"));
                ltask.Add(new LTask("与向导交谈", 5, 22, 1, 50, "向导:开始迎接真正的敌人了，做好一切准备，目标地牢再次进发！"));
                lconfig.等级任务.Add(28, ltask);











                




                //ltask = new List<LTask>();
                //lconfig.等级任务.Add(2, ltask);






                //此处可以初始化最开始的配置文件

                return lconfig;
            }
            using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            { return Read(fs); }
        }
        public static ConfigFile Read(Stream stream)//给定流文件进行读取
        {
            using (var sr = new StreamReader(stream))
            {
                var cf = JsonConvert.DeserializeObject<ConfigFile>(sr.ReadToEnd());
                if (ConfigR != null)
                    ConfigR(cf);
                return cf;
            }
        }
        public void Write(string Path)//给定路径进行写
        {
            using (var fs = new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.Write))
            { Write(fs); }
        }
        public void Write(Stream stream)//给定流文件写
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            { sw.Write(str); }
        }
        public static Action<ConfigFile> ConfigR;//定义为常量
    }



    public class OperateIniFile//读ini配置项相关
    {
        // API函数声明
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);
        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,
             string def, StringBuilder retVal, int size, string filePath);


        //读ini
        public static string ReadIniData(string iniFilePath, string Section, string Key, string NoText)
        {
            if (File.Exists(iniFilePath))
            {
                StringBuilder temp = new StringBuilder(1024);
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);
                return temp.ToString();
            }
            else
            {
                //return String.Empty;
                return NoText;//路径不存在等于没读到
            }
        }
        //写ini,key和Section可以使用null空值来删节删键
        public static bool WriteIniData(string iniFilePath,string Section, string Key, string Value)
        {
            if (!File.Exists(iniFilePath))
            {
                File.Create(iniFilePath).Close();
            }
            if (File.Exists(iniFilePath))
            {
                long OpStation = WritePrivateProfileString(Section, Key, Value, iniFilePath);
                if (OpStation == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
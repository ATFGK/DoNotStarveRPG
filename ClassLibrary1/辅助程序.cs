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
using System.Text.RegularExpressions;

namespace TestPlugin
{
    public class Sundry
    {
        public static int getDPSbyLeve(int leve)
        {
            if (LC.LConfig.等级伤害限制 != null)
                foreach (var ldl in LC.LConfig.等级伤害限制)
                {
                    if (ldl.起始等级 != 0 && leve < ldl.起始等级) continue;
                    if (ldl.结束等级 != 0 && leve > ldl.结束等级) continue;

                    return ldl.DPS限制;
                }
            return 0;
        }
        public static int getDamagebyLeve(int leve)
        {
            if (LC.LConfig.等级伤害限制 != null)
                foreach (var ldl in LC.LConfig.等级伤害限制)
                {
                    if (ldl.起始等级 != 0 && leve < ldl.起始等级) continue;
                    if (ldl.结束等级 != 0 && leve > ldl.结束等级) continue;

                    return ldl.伤害限制;
                }
            return 0;
        }

        public static long ComputeExp(int leve)//估算经验
        {
            //long l = (long)((30 * Math.Pow((double)leve / 3, 3)) + (3 * Math.Pow((double)leve / 3, 2)));
            long l = (long)((15 * Math.Pow((double)leve / 3, 3)) + leve);
            if (l < 1) l = 1;
            return l;
        }

        public static int ComputeExpP(int leve,long exp)//估算经验百分比
        {

            long l = ComputeExp(leve+1);//下一级经验
            long a = ComputeExp(leve);//当前等级经验0
            long b = exp - a;//当前等级的目前经验
            l -= a;//升级所需经验
            if (l == 0) return 100;
            int i = (int) (b*100/l);

            return i;
        }

        public static int ComputeExpByKillLife(int life)//根据血量估算经验
        {
            if (life <= 15) return 0;
            int e = (int)(Math.Pow(life, (1 / 3.5)) + Math.Log(life, 3.5) - 7.7);
            if (e < 1) e = 1;
            return e;
        }

        public static int ComputeNoExpByLeve(int leve)//根据等级估算最低得经验血量
        {
            //long l = (long)(Math.Atan(leve * 0.5 - 4.4) * 17950 + 23750);
            int i = (int)(Math.Atan(leve - 27.7) * 16355 + 25100);//怪物血量应该突破不了int
            if (i < 0) i = 0;

            return i;
        }

        public static int MaxDaySummon(TSPlayer ply)//取最大每日召唤
        {
            for (int i = 0; i < ply.Group.permissions.Count; i++)
            {
                var perm = ply.Group.permissions[i];
                Match Match = Regex.Match(perm, @"^饥荒\.日召唤\.(\d{1,9})$");//正则
                if (Match.Success) return Convert.ToInt32(Match.Groups[1].Value);
            }

            return LC.LConfig.每日召唤次数;//没有权限指定则返回配置的 内容
        }
        public static int MaxDayInvade(TSPlayer ply)//取最大每日入侵
        {
            for (int i = 0; i < ply.Group.permissions.Count; i++)
            {
                var perm = ply.Group.permissions[i];
                Match Match = Regex.Match(perm, @"^饥荒\.日入侵\.(\d{1,9})$");//正则
                if (Match.Success) return Convert.ToInt32(Match.Groups[1].Value);
            }

            return LC.LConfig.每日入侵次数;//没有权限指定则返回配置的 内容
        }

        public static int DaySummonT(TSPlayer ply)//取每日召唤叠加上限
        {
            for (int i = 0; i < ply.Group.permissions.Count; i++)
            {
                var perm = ply.Group.permissions[i];
                Match Match = Regex.Match(perm, @"^饥荒\.日召限\.(\d{1,9})$");//正则
                if (Match.Success) return Convert.ToInt32(Match.Groups[1].Value);
            }

            return LC.LConfig.叠加召唤上限;//没有权限指定则返回配置的 内容
        }
        public static int DayInvadeT(TSPlayer ply)//取每日入侵叠加上限
        {
            for (int i = 0; i < ply.Group.permissions.Count; i++)
            {
                var perm = ply.Group.permissions[i];
                Match Match = Regex.Match(perm, @"^饥荒\.日侵限\.(\d{1,9})$");//正则
                if (Match.Success) return Convert.ToInt32(Match.Groups[1].Value);
            }

            return LC.LConfig.叠加入侵上限;//没有权限指定则返回配置的 内容
        }


    }

}

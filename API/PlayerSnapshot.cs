using System.Collections.Generic;
using PlayerRoles;
using UnityEngine;

namespace Replacer.API
{
    public sealed class PlayerSnapshot
    {
        public string Nickname { get; set; }

        public RoleTypeId Role { get; set; }

        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }

        public float Health { get; set; }

        public float MaxHealth { get; set; }

        public float HumeShield { get; set; }

        public List<ItemType> Items { get; } = new List<ItemType>(8);

        public Dictionary<ItemType, ushort> Ammo { get; } = new Dictionary<ItemType, ushort>();

        public List<EffectSnapshot> Effects { get; } = new List<EffectSnapshot>();

        public bool IsScp079 { get; set; }

        public int Scp079Level { get; set; }

        public int Scp079Experience { get; set; }

        public float Scp079Energy { get; set; }
    }
}

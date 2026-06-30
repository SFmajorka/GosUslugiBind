using System;

namespace GosUslugiBind.Models
{
    [Serializable]
    public class BinderItem
    {
        public string Key { get; set; } = "";
        public string Action { get; set; } = "";
        public string Condition { get; set; } = "all";
        public int Uses { get; set; } = 0;
        public DateTime LastUsed { get; set; } = DateTime.Now;
    }
}